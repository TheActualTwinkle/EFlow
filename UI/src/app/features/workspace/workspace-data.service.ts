import { Injectable } from '@angular/core';
import { forkJoin, map, Observable, of, switchMap } from 'rxjs';

import type { components } from '../../api/contracts';
import { ApiService } from '../../core/api.service';
import { AdminView, WorkspaceData } from './workspace.models';

type Schemas = components['schemas'];
type CurrentUser = Schemas['CurrentUserView'];
type GroupView = Schemas['GroupView'];
type StudentView = Schemas['StudentView'];
type SubjectView = Schemas['SubjectView'];

@Injectable({ providedIn: 'root' })
export class WorkspaceDataService {
  constructor(private readonly api: ApiService) {}

  loadOverview(user: CurrentUser, current: WorkspaceData, roles: RoleFlags): Observable<Partial<WorkspaceData>> {
    if (roles.admin) {
      return forkJoin({
        groups: this.api.getGroups(),
        students: this.api.getStudents(),
        teachers: this.api.getTeachers(),
        subjects: this.api.getSubjects(),
        slots: this.api.getSlots(),
      }).pipe(switchMap((workspace) => this.withBookings(user.id, false, workspace.slots, workspace)));
    }

    if (roles.student) {
      return this.loadStudentSlots(user, current);
    }

    if (roles.teacher) {
      return forkJoin({
        teacher: this.api.getTeacher(user.id),
        slots: this.api.getSlotsByTeacher(user.id),
      }).pipe(switchMap((workspace) => this.withBookings(user.id, false, workspace.slots, workspace)));
    }

    return this.api.getSlots().pipe(switchMap((slots) => this.withBookings(user.id, false, slots, { slots })));
  }

  loadSlots(user: CurrentUser, current: WorkspaceData, roles: RoleFlags): Observable<Partial<WorkspaceData>> {
    if (roles.admin) {
      return forkJoin({
        groups: this.api.getGroups(),
        students: this.api.getStudents(),
        teachers: this.api.getTeachers(),
        subjects: this.api.getSubjects(),
        slots: this.api.getSlots(),
      }).pipe(switchMap((workspace) => this.withBookings(user.id, false, workspace.slots, workspace)));
    }

    if (roles.teacher) {
      return forkJoin({
        teacher: this.api.getTeacher(user.id),
        subjects: this.api.getSubjectsByTeacher(user.id),
        slots: this.api.getSlotsByTeacher(user.id),
      }).pipe(
        switchMap((workspace) =>
          this.withBookings(user.id, false, workspace.slots, {
            groups: groupsFromSubjects(workspace.subjects),
            teachers: [workspace.teacher],
            subjects: workspace.subjects,
            slots: workspace.slots,
          }),
        ),
      );
    }

    return this.loadStudentSlots(user, current);
  }

  loadBookings(user: CurrentUser, current: WorkspaceData, roles: RoleFlags): Observable<Partial<WorkspaceData>> {
    if (roles.student) {
      return this.loadStudentSlots(user, current);
    }

    if (roles.teacher) {
      return forkJoin({
        teacher: this.api.getTeacher(user.id),
        slots: this.api.getSlotsByTeacher(user.id),
      }).pipe(map((workspace) => ({ teachers: [workspace.teacher], slots: workspace.slots, bookings: [] })));
    }

    return this.api.getSlots().pipe(map((slots) => ({ slots, bookings: [] })));
  }

  loadAdminSection(section: AdminView): Observable<Partial<WorkspaceData>> {
    if (section === 'groups') {
      return this.api.getGroups().pipe(map((groups) => ({ groups })));
    }

    if (section === 'users') {
      return forkJoin({
        groups: this.api.getGroups(),
        students: this.api.getStudents(),
        teachers: this.api.getTeachers(),
      });
    }

    return forkJoin({
      groups: this.api.getGroups(),
      teachers: this.api.getTeachers(),
      subjects: this.api.getSubjects(),
    });
  }

  private withBookings(
    userId: string,
    isStudent: boolean,
    slots: WorkspaceData['slots'],
    workspace: Partial<WorkspaceData>,
  ): Observable<Partial<WorkspaceData>> {
    if (!slots.length) {
      return of({ ...workspace, bookings: [] });
    }

    return this.api.loadBookingsForUser(userId, isStudent, slots).pipe(map((bookings) => ({ ...workspace, bookings })));
  }

  private loadStudentSlots(user: CurrentUser, current: WorkspaceData): Observable<Partial<WorkspaceData>> {
    const cachedStudent = current.students.find((student) => student.id === user.id);

    if (cachedStudent) {
      return this.api.getSlots().pipe(
        switchMap((slots) => this.withBookings(user.id, true, visibleSlotsForStudent(slots, cachedStudent, user.id), { slots })),
      );
    }

    return forkJoin({
      student: this.api.getStudent(user.id),
      slots: this.api.getSlots(),
    }).pipe(
      switchMap((workspace) =>
        this.withBookings(user.id, true, visibleSlotsForStudent(workspace.slots, workspace.student, user.id), {
          students: [workspace.student],
          slots: workspace.slots,
        }),
      ),
    );
  }
}

export interface RoleFlags {
  admin: boolean;
  teacher: boolean;
  student: boolean;
}

function groupsFromSubjects(subjects: SubjectView[]): GroupView[] {
  const groups = new Map<string, GroupView>();
  for (const subject of subjects) {
    for (const group of subject.groups ?? []) {
      groups.set(group.id, group);
    }
  }
  return [...groups.values()];
}

function visibleSlotsForStudent(slots: WorkspaceData['slots'], student: StudentView, studentId: string): WorkspaceData['slots'] {
  const groupId = student.group?.id;

  return slots.filter((slot) => {
    if (slot.allowAllGroups || slot.admittedStudents?.some((admittedStudent) => admittedStudent.id === studentId)) {
      return true;
    }

    if (!groupId) {
      return false;
    }

    return (
      slot.allowedGroups?.some((group) => group.id === groupId) ||
      slot.subject?.groups?.some((group) => group.id === groupId)
    );
  });
}
