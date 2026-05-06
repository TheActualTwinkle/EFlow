import { CommonModule } from '@angular/common';
import { Component, computed, effect, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { filter, finalize, forkJoin } from 'rxjs';

import type { components } from './api/contracts';
import { ApiService } from './core/api.service';
import { AuthService } from './core/auth.service';
import { ThemeService } from './core/theme.service';
import {
  AdminView,
  createPersonForm,
  createSlotForm,
  emptyWorkspaceData,
  ModalView,
  ToastMessage,
  UserTable,
  WorkspaceData,
  WorkspaceView,
} from './features/workspace/workspace.models';
import {
  addMinutesToDateTimeLocal,
  calendarTitle,
  formatDateTime,
  minutesPerStudentLabel,
  toApiDateTime,
  toDateTimeLocal,
  utcOffsetLabel,
} from './shared/utils/date-time.utils';
import { fullName, initials, remindTimeLabel, roleLabel } from './shared/utils/person.utils';
import { ErrorModalComponent } from './shared/ui/error-modal/error-modal.component';
import { ToastStackComponent } from './shared/ui/toast-stack/toast-stack.component';
import { WorkspaceDataService } from './features/workspace/workspace-data.service';

type Schemas = components['schemas'];
type BookingRecordView = Schemas['BookingRecordView'];
type GroupView = Schemas['GroupView'];
type StudentView = Schemas['StudentView'];
type SubjectView = Schemas['SubjectView'];
type SubmissionSlotView = Schemas['SubmissionSlotView'];
type TeacherView = Schemas['TeacherView'];
type SlotCompletionFilter = 'all' | 'active' | 'finished';
type ConfirmDialog = {
  title: string;
  summaryTitle?: string;
  summaryMeta?: string;
  message: string;
  confirmLabel: string;
  action: () => void;
};

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    ErrorModalComponent,
    LucideAngularModule,
    ToastStackComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.less',
})
export class App {
  readonly activeView = signal<WorkspaceView>('overview');
  readonly activeAdminView = signal<AdminView>('groups');
  readonly toasts = signal<ToastMessage[]>([]);
  readonly data = signal<WorkspaceData>(emptyWorkspaceData());
  readonly loaded = signal(false);
  readonly workspaceLoading = computed(() => this.auth.isAuthenticated() && !this.loaded());
  readonly fieldErrors = signal<Record<string, string>>({});
  readonly activeModal = signal<ModalView>(null);
  readonly confirmDialog = signal<ConfirmDialog | null>(null);
  readonly userTable = signal<UserTable>('teachers');
  readonly selectedSlotId = signal<string | null>(null);
  readonly selectedGroup = signal<GroupView | null>(null);
  readonly selectedTeacher = signal<TeacherView | null>(null);
  readonly selectedStudent = signal<StudentView | null>(null);
  readonly selectedSubject = signal<SubjectView | null>(null);
  readonly editGroupDraft = signal<GroupView | null>(null);
  readonly editTeacherDraft = signal<TeacherView | null>(null);
  readonly editStudentDraft = signal<StudentView | null>(null);
  readonly editSubjectDraft = signal<SubjectView | null>(null);
  readonly editSlotDraft = signal<SubmissionSlotView | null>(null);
  selectedAdmissionStudentId = '';
  selectedBookingSlotId = '';
  selectedNotificationSlotId = '';
  readonly adminSearch = signal('');
  readonly slotSearch = signal('');
  readonly bookingSearch = signal('');
  readonly slotCompletionFilter = signal<SlotCompletionFilter>('all');
  readonly bookingCompletionFilter = signal<SlotCompletionFilter>('all');
  readonly slotOnlyAdmitted = signal(false);
  readonly bookingOnlyAdmitted = signal(false);
  readonly admissionSearch = signal('');
  readonly slotCreateGroupSearch = signal('');
  readonly expandedBookingSlotIds = signal<string[]>([]);
  readonly loadedBookingSlotIds = signal<string[]>([]);
  readonly loadingBookingSlotIds = signal<string[]>([]);
  readonly admissionTooltip = signal({ visible: false, text: '', x: 0, y: 0 });
  readonly loginForm = { username: '', password: '' };
  readonly groupForm = { name: '' };
  readonly personForm = createPersonForm();
  readonly subjectForm = { name: '', teacherId: '', groupIds: [] as string[] };
  readonly slotForm = createSlotForm();
  readonly notificationForm = {
    remindTimes: ['OneWeek', 'TwoDays'],
    bookingMode: 'All',
  };
  readonly personRoles = ['Student', 'Teacher'] as const;
  readonly slotGroupSearch = signal('');
  readonly loginReadonly = signal(true);
  readonly passwordReadonly = signal(true);
  readonly dateTimePickerOpen = signal<string | null>(null);
  readonly customSelectOpen = signal<string | null>(null);
  readonly pickerMonth = signal(new Date());
  private toastId = 0;
  private lastErrorToast: string | null = null;
  private lastLoadedKey = '';

  readonly todayIso = new Date().toISOString().slice(0, 10);
  private readonly noAdmissionHint = 'У вас нет допуска к этому окну защиты. Обратитесь к преподавателю.';
  private readonly reminderKeysByValue = ['TwoWeeks', 'OneWeek', 'TwoDays', 'FourHours'];
  private readonly bookingModeKeysByValue = ['None', 'All', 'OnlyCancellation', 'OnlyNewBooking'];
  readonly visibleSlots = computed(() => {
    const user = this.auth.user();

    if (!user || this.auth.hasRole('Admin')) {
      return this.data().slots;
    }

    if (this.auth.hasRole('Teacher')) {
      return this.data().slots.filter((slot) => slot.teacher?.id === user.id);
    }

    const student = this.currentStudent();
    const groupId = student?.group?.id;

    return this.data().slots.filter((slot) => {
      if (slot.allowAllGroups || this.isAdmitted(slot, user.id)) {
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
  });
  readonly freeSeats = computed(() =>
    this.visibleSlots().reduce((total, slot) => total + Math.max(0, this.slotCapacity(slot) - this.slotBookingCount(slot)), 0),
  );
  readonly filteredVisibleSlots = computed(() => {
    const query = this.slotSearch().trim().toLowerCase();
    return this.visibleSlots().filter((slot) => this.slotMatchesFilters(slot, query, this.slotCompletionFilter(), this.slotOnlyAdmitted()));
  });
  readonly filteredBookings = computed(() => {
    const query = this.bookingSearch().trim().toLowerCase();
    return this.data().bookings.filter((booking) => {
      const slot = booking.slot;
      if (slot && !this.slotMatchesStateFilters(slot, this.bookingCompletionFilter(), this.bookingOnlyAdmitted())) {
        return false;
      }

      if (!query) {
        return true;
      }

      return [this.fullName(booking.student), slot ? this.slotTitle(slot) : '', slot?.location]
        .filter(Boolean)
        .some((value) => value!.toLowerCase().includes(query));
    });
  });
  readonly bookingSlots = computed(() => {
    const slots = new Map<string, SubmissionSlotView>();

    for (const booking of this.filteredBookings()) {
      if (booking.slot?.id) {
        slots.set(booking.slot.id, booking.slot);
      }
    }

    return [...slots.values()];
  });
  readonly filteredBookingSlots = computed(() => {
    const query = this.bookingSearch().trim().toLowerCase();
    let slots = this.visibleSlots().filter(
      (slot) =>
        this.slotMatchesStateFilters(slot, this.bookingCompletionFilter(), this.bookingOnlyAdmitted()) &&
        this.slotMatchesBookingQuery(slot, query),
    );

    if (this.auth.hasRole('Student')) {
      const userId = this.auth.user()?.id;
      const bookedSlotIds = new Set(
        this.data()
          .bookings.filter((booking) => booking.student?.id === userId && booking.slot?.id)
          .map((booking) => booking.slot!.id),
      );

      slots = slots.filter((slot) => bookedSlotIds.has(slot.id));
    }
    return slots;
  });
  readonly selectedSlot = computed(() => this.data().slots.find((slot) => slot.id === this.selectedSlotId()) ?? this.visibleSlots()[0] ?? null);

  constructor(
    readonly auth: AuthService,
    readonly api: ApiService,
    readonly theme: ThemeService,
    private readonly workspaceData: WorkspaceDataService,
    private readonly router: Router,
  ) {
    this.syncRoute(this.router.url);
    this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd)).subscribe((event) => {
      this.syncRoute(event.urlAfterRedirects);
      this.loadActiveView();
    });

    effect(() => {
      const user = this.auth.user();

      if (!user && !this.auth.hasToken() && !this.router.url.startsWith('/login')) {
        this.router.navigateByUrl('/login');
        return;
      }

      if (user && this.router.url.startsWith('/login')) {
        this.router.navigateByUrl('/overview');
        return;
      }

      if (user && this.activeView() === 'admin' && !this.auth.hasRole('Admin')) {
        this.go('overview');
      }

      if (user) {
        this.loadActiveView();
      }
    });

    effect(() => {
      const error = this.api.error();

      if (error && error !== this.lastErrorToast) {
        this.lastErrorToast = error;
        this.showToast(error, 'error');
      }

      if (!error) {
        this.lastErrorToast = null;
      }
    });
  }

  login(): void {
    this.auth.login(this.loginForm.username.trim(), this.loginForm.password).subscribe((ok) => {
      if (!ok) {
        return;
      }

      this.auth.loadCurrentUser().subscribe((loaded) => {
        if (loaded) {
          this.go('overview');
        }
      });
    });
  }

  logout(): void {
    this.auth.logoutOnServer().subscribe(() => {
      this.loginForm.username = '';
      this.loginForm.password = '';
      this.loginReadonly.set(true);
      this.passwordReadonly.set(true);
      this.loaded.set(false);
      this.lastLoadedKey = '';
      this.fieldErrors.set({});
      this.activeModal.set(null);
      this.data.set(emptyWorkspaceData());
      this.router.navigateByUrl('/login');
    });
  }

  refresh(): void {
    this.loadActiveView(true);
  }

  private loadActiveView(force = false): void {
    const user = this.auth.user();

    if (!user) {
      return;
    }

    const view = this.activeView();
    const adminView = this.activeAdminView();
    const key = `${user.id}:${view}:${adminView}`;
    const keyChanged = this.lastLoadedKey !== key;

    if (!force && this.lastLoadedKey === key) {
      return;
    }

    this.lastLoadedKey = key;
    this.loaded.set(false);
    this.api.clearError();

    if (keyChanged) {
      this.resetBookingExpansion();
    }

    const roles = {
      admin: this.auth.hasRole('Admin'),
      teacher: this.auth.hasRole('Teacher'),
      student: this.auth.hasRole('Student'),
    };

    if (view === 'admin') {
      if (!this.ensureAdmin()) {
        return;
      }

      this.workspaceData.loadAdminSection(adminView).subscribe({
        next: (workspace) => this.applyWorkspace(workspace),
        error: () => this.loaded.set(true),
      });
      return;
    }

    const loader =
      view === 'slots'
        ? this.workspaceData.loadSlots(user, this.data(), roles)
        : view === 'bookings'
          ? this.workspaceData.loadBookings(user, this.data(), roles)
          : this.workspaceData.loadOverview(user, this.data(), roles);

    loader.subscribe({
      next: (workspace) => this.applyWorkspace(workspace),
      error: () => this.loaded.set(true),
    });
  }

  private applyWorkspace(partial: Partial<WorkspaceData>): void {
    const current = this.data();
    const next = { ...current, ...partial };
    this.data.set(next);
    if (partial.bookings && this.auth.hasRole('Admin')) {
      this.loadedBookingSlotIds.set(next.slots.map((slot) => slot.id));
    }
    this.loaded.set(true);
    this.syncDefaultSelections(next);
  }

  private syncDefaultSelections(data: WorkspaceData): void {
    const user = this.auth.user();
    const firstVisibleSlot = this.visibleSlots()[0];

    const visibleSlotIds = new Set(this.visibleSlots().map((slot) => slot.id));

    if (!this.selectedSlotId() || !visibleSlotIds.has(this.selectedSlotId()!)) {
      this.selectedSlotId.set(firstVisibleSlot?.id ?? null);
    }

    if (!this.selectedBookingSlotId || !visibleSlotIds.has(this.selectedBookingSlotId)) {
      this.selectedBookingSlotId = firstVisibleSlot?.id ?? '';
    }

    if (!this.selectedNotificationSlotId || !visibleSlotIds.has(this.selectedNotificationSlotId)) {
      this.selectedNotificationSlotId = firstVisibleSlot?.id ?? '';
    }

    if (!this.personForm.groupId || !data.groups.some((group) => group.id === this.personForm.groupId)) {
      this.personForm.groupId = data.groups[0]?.id ?? '';
    }

    if (!this.subjectForm.teacherId || !data.teachers.some((teacher) => teacher.id === this.subjectForm.teacherId)) {
      this.subjectForm.teacherId = data.teachers[0]?.id ?? '';
    }

    if (!this.slotForm.teacherId || !data.teachers.some((teacher) => teacher.id === this.slotForm.teacherId)) {
      this.slotForm.teacherId = data.teachers[0]?.id ?? user?.id ?? '';
    }

    if (!this.slotForm.subjectId || !data.subjects.some((subject) => subject.id === this.slotForm.subjectId)) {
      this.slotForm.subjectId = data.subjects[0]?.id ?? '';
    }
  }

  createGroup(): void {
    if (!this.ensureAdmin()) {
      return;
    }

    if (!this.validateGroupForm()) {
      return;
    }

    this.api.createGroup(this.groupForm.name.trim()).subscribe(() => {
      this.groupForm.name = '';
      this.showToast('Группа создана', 'success');
      this.closeModal();
      this.refresh();
    });
  }

  createPerson(): void {
    if (!this.ensureAdmin()) {
      return;
    }

    if (!this.validatePersonForm()) {
      return;
    }

    const request = {
      userName: this.personForm.userName,
      password: this.personForm.password,
      email: this.personForm.email,
      firstName: this.personForm.firstName,
      middleName: this.personForm.middleName,
      lastName: this.personForm.lastName,
      birthDate: this.personForm.birthDate,
    };

    const call =
      this.personForm.role === 'Teacher'
        ? this.api.createTeacher(request)
        : this.api.createStudent({ ...request, groupId: this.personForm.groupId });

    call.subscribe(() => {
      this.personForm.userName = '';
      this.personForm.password = '';
      this.personForm.email = '';
      this.personForm.firstName = '';
      this.personForm.middleName = '';
      this.personForm.lastName = '';
      this.personForm.birthDate = '';
      this.showToast(this.personForm.role === 'Teacher' ? 'Преподаватель создан' : 'Студент создан', 'success');
      this.closeModal();
      this.refresh();
    });
  }

  createSubject(): void {
    if (!this.ensureAdmin()) {
      return;
    }

    if (!this.validateSubjectForm()) {
      return;
    }

    this.api.createSubject(this.subjectForm.name, this.subjectForm.teacherId, this.subjectForm.groupIds).subscribe(() => {
      this.subjectForm.name = '';
      this.subjectForm.groupIds = [];
      this.showToast('Дисциплина создана', 'success');
      this.closeModal();
      this.refresh();
    });
  }

  updateGroup(group: GroupView): void {
    if (!this.ensureAdmin()) {
      return;
    }

    this.api.updateGroup(group.id, group.name).subscribe(() => {
      this.showToast('Группа обновлена', 'success');
      this.closeModal();
      this.refresh();
    });
  }

  updateTeacher(teacher: TeacherView): void {
    if (!this.ensureAdmin()) {
      return;
    }

    this.api
      .updateTeacher(teacher.id, {
        firstName: teacher.firstName,
        lastName: teacher.lastName,
        middleName: teacher.middleName,
        birthDate: teacher.birthDate,
      })
      .subscribe(() => {
        this.showToast('Преподаватель обновлён', 'success');
        this.closeModal();
        this.refresh();
      });
  }

  updateStudent(student: StudentView): void {
    if (!this.ensureAdmin()) {
      return;
    }

    this.api
      .updateStudent(student.id, {
        firstName: student.firstName,
        lastName: student.lastName,
        middleName: student.middleName,
        birthDate: student.birthDate,
      })
      .subscribe(() => {
        this.showToast('Студент обновлён', 'success');
        this.closeModal();
        this.refresh();
      });
  }

  updateSubject(subject: SubjectView): void {
    if (!this.ensureAdmin()) {
      return;
    }

    this.api
      .updateSubject(subject.id, {
        name: subject.name,
        teacherId: subject.teacher?.id,
        groupIds: subject.groups?.map((group) => group.id) ?? [],
      })
      .subscribe(() => {
        this.showToast('Дисциплина обновлена', 'success');
        this.closeModal();
        this.refresh();
      });
  }

  updateSlot(slot: SubmissionSlotView): void {
    if (!this.auth.hasRole('Admin', 'Teacher')) {
      this.showToast('Недостаточно прав для изменения окна защиты', 'warning');
      return;
    }

    const allowedGroupIds = this.slotGroupIds(slot);

    this.api
      .updateSlot(slot.id, {
        subjectId: slot.subject?.id,
        teacherId: slot.teacher?.id,
        startTime: toApiDateTime(slot.startTime),
        endTime: toApiDateTime(slot.endTime),
        maxStudents: this.slotCapacity(slot),
        allowAllGroups: slot.allowAllGroups,
        allowedGroupIds: slot.allowAllGroups ? [] : allowedGroupIds,
        location: slot.location,
        comment: slot.comment,
      })
      .subscribe(() => {
        this.showToast('Окно защиты обновлено', 'success');
        this.closeModal();
        this.refresh();
      });
  }

  slotDurationPerStudentLabelFor(slot: SubmissionSlotView): string {
    return minutesPerStudentLabel(slot.startTime, slot.endTime, this.slotCapacity(slot));
  }

  slotCapacity(slot: SubmissionSlotView): number {
    return Number(slot.maxStudents) || 0;
  }

  createSlot(): void {
    if (!this.auth.hasRole('Admin', 'Teacher')) {
      this.showToast('Недостаточно прав для создания окна защиты', 'warning');
      return;
    }

    if (!this.validateSlotForm()) {
      return;
    }

    const currentUser = this.auth.user();
    const teacherId = this.auth.hasRole('Teacher') ? currentUser?.id : this.slotForm.teacherId;

    if (!teacherId) {
      this.setFieldErrors({ slotTeacher: 'Выберите преподавателя.' });
      return;
    }

    this.api
      .createSlot({
        ...this.slotForm,
        startTime: toApiDateTime(this.slotForm.startTime),
        endTime: toApiDateTime(this.slotForm.endTime),
        teacherId,
        allowedGroupIds: this.slotForm.allowAllGroups ? [] : this.slotForm.allowedGroupIds,
        location: this.slotForm.location || undefined,
        comment: this.slotForm.comment || undefined,
      })
      .subscribe(() => {
        this.showToast('Окно защиты создано', 'success');
        this.closeModal();
        this.refresh();
      });
  }

  onSlotStartChanged(value: string): void {
    this.slotForm.startTime = value;

    if (!value || this.slotForm.endTime) {
      return;
    }

    this.slotForm.endTime = addMinutesToDateTimeLocal(value, 90);
  }

  onSlotStartInput(event: Event): void {
    this.onSlotStartChanged((event.target as HTMLInputElement).value);
  }

  openDateTimePicker(target: string): void {
    const value = this.getDateTimeValue(target);
    this.pickerMonth.set(value ? new Date(value) : new Date());
    this.dateTimePickerOpen.set(this.dateTimePickerOpen() === target ? null : target);
  }

  closeDateTimePicker(): void {
    this.dateTimePickerOpen.set(null);
  }

  toggleCustomSelect(id: string): void {
    this.customSelectOpen.set(this.customSelectOpen() === id ? null : id);
  }

  closeCustomSelect(): void {
    this.customSelectOpen.set(null);
  }

  selectPersonRole(role: 'Student' | 'Teacher'): void {
    this.personForm.role = role;
    this.closeCustomSelect();
  }

  personRoleLabel(): string {
    return this.roleLabel(this.personForm.role);
  }

  selectPersonGroup(group: GroupView): void {
    this.personForm.groupId = group.id;
    this.closeCustomSelect();
  }

  personGroupLabel(): string {
    return this.data().groups.find((group) => group.id === this.personForm.groupId)?.name ?? 'Выберите группу';
  }

  selectSubjectTeacher(teacher: TeacherView): void {
    this.subjectForm.teacherId = teacher.id;
    this.closeCustomSelect();
  }

  subjectTeacherLabel(): string {
    const teacher = this.data().teachers.find((item) => item.id === this.subjectForm.teacherId);
    return teacher ? this.fullName(teacher) : 'Выберите преподавателя';
  }

  selectSlotSubject(subject: SubjectView): void {
    this.slotForm.subjectId = subject.id;
    this.slotCreateGroupSearch.set('');
    const allowed = new Set((subject.groups ?? []).map((group) => group.id));
    this.slotForm.allowedGroupIds = this.slotForm.allowedGroupIds.filter((id) => allowed.has(id));
    this.closeCustomSelect();
  }

  slotSubjectLabel(): string {
    return this.data().subjects.find((subject) => subject.id === this.slotForm.subjectId)?.name ?? 'Выберите дисциплину';
  }

  selectSlotTeacher(teacher: TeacherView): void {
    this.slotForm.teacherId = teacher.id;
    this.closeCustomSelect();
  }

  slotTeacherLabel(): string {
    return this.data().teachers.find((teacher) => teacher.id === this.slotForm.teacherId) ? this.fullName(this.data().teachers.find((teacher) => teacher.id === this.slotForm.teacherId)) : 'Выберите преподавателя';
  }

  slotAllowedGroups(): GroupView[] {
    const subject = this.data().subjects.find((item) => item.id === this.slotForm.subjectId);
    return subject?.groups ?? [];
  }

  filteredSlotAllowedGroups(): GroupView[] {
    const query = this.slotCreateGroupSearch().trim().toLowerCase();
    const groups = this.slotAllowedGroups();

    if (!query) {
      return groups;
    }

    return groups.filter((group) => group.name.toLowerCase().includes(query));
  }

  slotDurationPerStudentLabel(): string {
    return minutesPerStudentLabel(this.slotForm.startTime, this.slotForm.endTime, this.slotForm.maxStudents);
  }

  deleteSlot(slotId: string): void {
    const slot = this.data().slots.find((item) => item.id === slotId);
    this.confirmDestructiveAction({
      title: 'Удалить окно защиты?',
      summaryTitle: slot ? this.slotTitle(slot) : undefined,
      summaryMeta: slot ? this.formatDateTime(slot.startTime) : undefined,
      message: slot
        ? 'Связанные записи и допуски станут недоступны.'
        : 'Окно защиты будет удалено из расписания.',
      confirmLabel: 'Удалить окно защиты',
      action: () => {
        this.api.deleteSlot(slotId).subscribe(() => {
          this.showToast('Окно защиты удалено', 'success');
          this.refresh();
        });
      },
    });
  }

  addAdmission(): void {
    const slot = this.selectedSlot();

    if (!slot || !this.selectedAdmissionStudentId) {
      this.setFieldErrors({ admissionStudent: 'Выберите студента.' });
      this.showToast('Выберите студента для допуска', 'warning');
      return;
    }

    this.api.addAdmission(slot.id, this.selectedAdmissionStudentId).subscribe(() => {
      this.refresh();
    });
  }

  toggleAdmission(slot: SubmissionSlotView, student: StudentView): void {
    const admitted = this.isAdmitted(slot, student.id);
    const onSaved = () => {
      this.refreshSlot(slot.id);
    };

    if (admitted) {
      this.api.removeAdmission(slot.id, student.id).subscribe(onSaved);
      return;
    }

    this.api.addAdmission(slot.id, student.id).subscribe(onSaved);
  }

  removeAdmission(slotId: string, studentId: string): void {
    this.api.removeAdmission(slotId, studentId).subscribe(() => {
      this.refresh();
    });
  }

  bookSelectedSlot(): void {
    const user = this.auth.user();

    if (!user || !this.selectedBookingSlotId) {
      this.setFieldErrors({ bookingSlot: 'Выберите доступное окно защиты.' });
      this.showToast('Выберите доступное окно защиты', 'warning');
      return;
    }

    this.api.bookSlot(this.selectedBookingSlotId, user.id).subscribe(() => {
      const slot = this.data().slots.find((item) => item.id === this.selectedBookingSlotId);
      this.showToast('Запись создана', 'success');
      if (slot) {
        this.openNotificationsForSlot(slot);
      } else {
        this.closeModal();
      }
      this.refresh();
    });
  }

  bookSlot(slot: SubmissionSlotView): void {
    const reason = this.slotBookingDisabledReason(slot);
    if (reason) {
      this.showToast(reason, 'warning');
      return;
    }

    this.selectedBookingSlotId = slot.id;
    this.bookSelectedSlot();
  }

  cancelBooking(id: string): void {
    const booking = this.data().bookings.find((item) => item.id === id);
    const studentName = booking?.student ? this.fullName(booking.student) : 'Запись';
    const slotName = booking?.slot ? this.slotTitle(booking.slot) : 'окно защиты';
    this.confirmDestructiveAction({
      title: 'Отменить запись?',
      message: `${studentName} будет удалён из очереди на «${slotName}».`,
      confirmLabel: 'Отменить запись',
      action: () => {
        this.api.cancelBooking(id).subscribe(() => {
          this.showToast('Запись отменена', 'success');
          this.refresh();
        });
      },
    });
  }

  deleteGroup(group: GroupView): void {
    this.confirmDestructiveAction({
      title: 'Удалить группу?',
      message: `Группа «${group.name}» будет удалена. Это может повлиять на студентов, дисциплины и доступ к окнам защиты.`,
      confirmLabel: 'Удалить группу',
      action: () => {
        this.api.deleteGroup(group.id).subscribe(() => {
          this.showToast('Группа удалена', 'success');
          this.closeModal();
          this.refresh();
        });
      },
    });
  }

  deleteTeacher(teacher: TeacherView): void {
    this.confirmDestructiveAction({
      title: 'Удалить преподавателя?',
      message: `Профиль «${this.fullName(teacher)}» будет удалён вместе с доступом к системе.`,
      confirmLabel: 'Удалить преподавателя',
      action: () => {
        this.api.deleteTeacher(teacher.id).subscribe(() => {
          this.showToast('Преподаватель удалён', 'success');
          this.closeModal();
          this.refresh();
        });
      },
    });
  }

  deleteStudent(student: StudentView): void {
    this.confirmDestructiveAction({
      title: 'Удалить студента?',
      message: `Профиль «${this.fullName(student)}» будет удалён вместе с доступом к системе.`,
      confirmLabel: 'Удалить студента',
      action: () => {
        this.api.deleteStudent(student.id).subscribe(() => {
          this.showToast('Студент удалён', 'success');
          this.closeModal();
          this.refresh();
        });
      },
    });
  }

  deleteSubject(subject: SubjectView): void {
    this.confirmDestructiveAction({
      title: 'Удалить дисциплину?',
      message: `Дисциплина «${subject.name}» будет удалена из учебного процесса.`,
      confirmLabel: 'Удалить дисциплину',
      action: () => {
        this.api.deleteSubject(subject.id).subscribe(() => {
          this.showToast('Дисциплина удалена', 'success');
          this.closeModal();
          this.refresh();
        });
      },
    });
  }

  saveNotificationSettings(): void {
    const user = this.auth.user();

    if (!this.canConfigureSlotNotifications()) {
      this.showToast('Настройки уведомлений недоступны для администратора', 'warning');
      this.closeModal();
      return;
    }

    if (!user || !this.selectedNotificationSlotId) {
      this.setFieldErrors({ notificationSlot: 'Откройте уведомления у конкретного окна защиты.' });
      this.showToast('Откройте уведомления у конкретного окна защиты', 'warning');
      return;
    }

    this.api
      .updateNotificationSettings(
        this.selectedNotificationSlotId,
        user.id,
        this.notificationForm.remindTimes.map((time) => this.remindTimeValue(time)),
        this.bookingModeValue(this.notificationForm.bookingMode),
      )
      .subscribe(() => {
        this.showToast('Настройки уведомлений сохранены', 'success');
        this.closeModal();
        this.refresh();
      });
  }

  openModal(modal: Exclude<ModalView, null>): void {
    this.fieldErrors.set({});
    this.dateTimePickerOpen.set(null);
    this.customSelectOpen.set(null);
    this.activeModal.set(modal);
  }

  openSlotEditor(slot: SubmissionSlotView): void {
    this.selectedSlotId.set(slot.id);
    this.slotGroupSearch.set('');
    this.editSlotDraft.set(this.cloneSlot(slot));
    this.openModal('editSlot');
  }

  openAdmissions(slot: SubmissionSlotView): void {
    this.selectedSlotId.set(slot.id);
    this.selectedAdmissionStudentId = '';
    this.admissionSearch.set('');
    this.openModal('admissions');
    forkJoin({
      students: this.api.getStudents(),
      slot: this.api.getSlot(slot.id),
    }).subscribe(({ students, slot: freshSlot }) => {
      this.applyWorkspace({ students, slots: this.replaceSlot(this.data().slots, freshSlot) });
      this.selectedSlotId.set(freshSlot.id);
    });
  }

  openNotificationsForSlot(slot?: SubmissionSlotView): void {
    if (!this.canConfigureSlotNotifications()) {
      return;
    }

    const target = slot ?? this.selectedSlot() ?? this.visibleSlots()[0] ?? null;
    this.selectedNotificationSlotId = target?.id ?? '';
    this.prefillNotificationForm(target);
    this.openModal('notifications');
  }

  editGroup(group: GroupView): void {
    this.selectedGroup.set(group);
    this.editGroupDraft.set(this.cloneGroup(group));
    this.openModal('editGroup');
  }

  editTeacher(teacher: TeacherView): void {
    this.selectedTeacher.set(teacher);
    this.editTeacherDraft.set(this.cloneTeacher(teacher));
    this.openModal('editTeacher');
  }

  editStudent(student: StudentView): void {
    this.selectedStudent.set(student);
    this.editStudentDraft.set(this.cloneStudent(student));
    this.openModal('editStudent');
  }

  editSubject(subject: SubjectView): void {
    this.selectedSubject.set(subject);
    this.editSubjectDraft.set(this.cloneSubject(subject));
    this.openModal('editSubject');
  }

  closeModal(): void {
    this.activeModal.set(null);
    this.fieldErrors.set({});
    this.dateTimePickerOpen.set(null);
    this.customSelectOpen.set(null);
    this.editGroupDraft.set(null);
    this.editTeacherDraft.set(null);
    this.editStudentDraft.set(null);
    this.editSubjectDraft.set(null);
    this.editSlotDraft.set(null);
  }

  closeConfirmDialog(): void {
    this.confirmDialog.set(null);
  }

  confirmPendingAction(): void {
    const dialog = this.confirmDialog();

    if (!dialog) {
      return;
    }

    this.confirmDialog.set(null);
    dialog.action();
  }

  toggleSelection(collection: string[], id: string): void {
    const index = collection.indexOf(id);

    if (index >= 0) {
      collection.splice(index, 1);
      return;
    }

    collection.push(id);
  }

  fieldError(name: string): string {
    return this.fieldErrors()[name] ?? '';
  }

  go(view: WorkspaceView): void {
    if (view === 'admin' && !this.auth.hasRole('Admin')) {
      this.showToast('Недостаточно прав для раздела администрирования', 'warning');
      this.router.navigateByUrl('/overview');
      return;
    }

    this.router.navigateByUrl(view === 'overview' ? '/overview' : `/${view}`);
  }

  goAdmin(section: AdminView): void {
    if (!this.auth.hasRole('Admin')) {
      this.showToast('Недостаточно прав для раздела администрирования', 'warning');
      this.router.navigateByUrl('/overview');
      return;
    }

    this.router.navigateByUrl(`/admin/${section}`);
  }

  bookingsForSlot(slotId: string): BookingRecordView[] {
    return this.data().bookings.filter((booking) => booking.slot?.id === slotId);
  }

  slotBookingCount(slot: SubmissionSlotView): number {
    return Math.max(Number(slot.bookingCount) || 0, this.bookingsForSlot(slot.id).length);
  }

  isBookingSlotExpanded(slotId: string): boolean {
    return this.expandedBookingSlotIds().includes(slotId);
  }

  toggleBookingSlot(slot: SubmissionSlotView): void {
    const expanded = this.isBookingSlotExpanded(slot.id);
    this.expandedBookingSlotIds.update((ids) => (expanded ? ids.filter((id) => id !== slot.id) : [...ids, slot.id]));

    if (!expanded && !this.loadedBookingSlotIds().includes(slot.id)) {
      this.loadingBookingSlotIds.update((ids) => (ids.includes(slot.id) ? ids : [...ids, slot.id]));
      this.api
        .getBookingsBySlot(slot.id, true)
        .pipe(finalize(() => this.loadingBookingSlotIds.update((ids) => ids.filter((id) => id !== slot.id))))
        .subscribe((bookings) => {
          this.data.update((current) => ({
            ...current,
            bookings: [...current.bookings.filter((booking) => booking.slot?.id !== slot.id), ...bookings],
          }));
          this.loadedBookingSlotIds.update((ids) => (ids.includes(slot.id) ? ids : [...ids, slot.id]));
        });
    }
  }

  isBookingSlotLoading(slotId: string): boolean {
    return this.loadingBookingSlotIds().includes(slotId);
  }

  notificationSlot(): SubmissionSlotView | null {
    return this.data().slots.find((slot) => slot.id === this.selectedNotificationSlotId) ?? null;
  }

  currentStudent(): StudentView | null {
    const id = this.auth.user()?.id;
    return this.data().students.find((student) => student.id === id) ?? null;
  }

  currentTeacher(): TeacherView | null {
    const id = this.auth.user()?.id;
    return this.data().teachers.find((teacher) => teacher.id === id) ?? null;
  }

  sidebarName(): string {
    const profile = this.currentStudent() ?? this.currentTeacher();
    return profile ? this.fullName(profile) : (this.auth.user()?.userName ?? 'EFlow');
  }

  sidebarSubtitle(): string {
    const student = this.currentStudent();
    if (student) {
      return student.group?.name ?? 'Группа не указана';
    }

    return this.roleLabel(this.auth.primaryRole());
  }

  filteredGroups(): GroupView[] {
    const query = this.adminSearch().trim().toLowerCase();
    return this.data().groups.filter((group) => !query || group.name.toLowerCase().includes(query));
  }

  filteredTeachers(): TeacherView[] {
    const query = this.adminSearch().trim().toLowerCase();
    return this.data().teachers.filter((teacher) => !query || this.fullName(teacher).toLowerCase().includes(query));
  }

  filteredStudents(): StudentView[] {
    const query = this.adminSearch().trim().toLowerCase();
    return this.data().students.filter((student) =>
      !query ||
      this.fullName(student).toLowerCase().includes(query) ||
      student.group?.name.toLowerCase().includes(query),
    );
  }

  filteredSubjects(): SubjectView[] {
    const query = this.adminSearch().trim().toLowerCase();
    return this.data().subjects.filter((subject) =>
      !query ||
      subject.name.toLowerCase().includes(query) ||
      this.fullName(subject.teacher).toLowerCase().includes(query) ||
      subject.groups?.some((group) => group.name.toLowerCase().includes(query)),
    );
  }

  isBookedByCurrentUser(slotId: string): boolean {
    const userId = this.auth.user()?.id;
    return !!userId && this.data().bookings.some((booking) => booking.slot?.id === slotId && booking.student?.id === userId);
  }

  slotHasNoAdmissions(slot: SubmissionSlotView): boolean {
    return this.auth.hasRole('Teacher') && slot.teacher?.id === this.auth.user()?.id && !(slot.admittedStudents?.length ?? 0);
  }

  currentUserNotificationSettings(slot: SubmissionSlotView) {
    const userId = this.auth.user()?.id;
    return userId ? slot.notificationSettings?.find((settings) => settings.userId === userId) : undefined;
  }

  slotNeedsNotificationSetup(slot: SubmissionSlotView): boolean {
    if (!this.canConfigureSlotNotifications()) {
      return false;
    }

    if (this.currentUserNotificationSettings(slot)) {
      return false;
    }

    if (this.auth.hasRole('Student')) {
      return this.isBookedByCurrentUser(slot.id);
    }

    return this.auth.hasRole('Teacher') && slot.teacher?.id === this.auth.user()?.id;
  }

  slotAdmissionWarningHint(slot: SubmissionSlotView): string {
    return this.slotHasNoAdmissions(slot) ? 'Никому не выдан допуск к этому окну защиты!' : '';
  }

  slotNotificationWarningHint(slot: SubmissionSlotView): string {
    return this.slotNeedsNotificationSetup(slot) ? 'Уведомления не настроены!' : '';
  }

  canConfigureSlotNotifications(): boolean {
    return !this.auth.hasRole('Admin') && this.auth.hasRole('Student', 'Teacher');
  }

  slotMatchesFilters(slot: SubmissionSlotView, query: string, completion: SlotCompletionFilter, onlyAdmitted: boolean): boolean {
    return this.slotMatchesStateFilters(slot, completion, onlyAdmitted) && this.slotMatchesQuery(slot, query);
  }

  slotMatchesStateFilters(slot: SubmissionSlotView, completion: SlotCompletionFilter, onlyAdmitted: boolean): boolean {
    if (completion === 'active' && this.isSlotFinished(slot)) {
      return false;
    }

    if (completion === 'finished' && !this.isSlotFinished(slot)) {
      return false;
    }

    return !onlyAdmitted || !this.auth.hasRole('Student') || this.isAdmittedToCurrentStudent(slot);
  }

  slotMatchesQuery(slot: SubmissionSlotView, query: string): boolean {
    if (!query) {
      return true;
    }

    return [this.slotTitle(slot), this.fullName(slot.teacher), slot.location, slot.comment]
      .filter(Boolean)
      .some((value) => value!.toLowerCase().includes(query));
  }

  slotMatchesBookingQuery(slot: SubmissionSlotView, query: string): boolean {
    if (this.slotMatchesQuery(slot, query)) {
      return true;
    }

    return this.bookingsForSlot(slot.id).some((booking) =>
      [this.fullName(booking.student), booking.student?.group?.name]
        .filter(Boolean)
        .some((value) => value!.toLowerCase().includes(query)),
    );
  }

  isAdmittedToCurrentStudent(slot: SubmissionSlotView): boolean {
    const userId = this.auth.user()?.id;
    return !!userId && this.auth.hasRole('Student') && this.isAdmitted(slot, userId);
  }

  slotBookingDisabledReason(slot: SubmissionSlotView): string {
    if (this.isBookedByCurrentUser(slot.id)) {
      return 'Вы уже записаны на это окно защиты.';
    }

    const admissionHint = this.slotAdmissionHint(slot);
    if (admissionHint) {
      return admissionHint;
    }

    return '';
  }

  slotAdmissionHint(slot: SubmissionSlotView): string {
    return this.studentLacksAdmission(slot) ? this.noAdmissionHint : '';
  }

  showAdmissionTooltip(slot: SubmissionSlotView, event: MouseEvent): void {
    const text = this.slotAdmissionHint(slot);
    if (!text) {
      this.hideAdmissionTooltip();
      return;
    }

    this.admissionTooltip.set({ visible: true, text, x: event.clientX, y: event.clientY });
  }

  showSlotWarningTooltip(text: string, event: MouseEvent): void {
    if (!text) {
      this.hideAdmissionTooltip();
      return;
    }

    this.admissionTooltip.set({ visible: true, text, x: event.clientX, y: event.clientY });
  }

  moveAdmissionTooltip(event: MouseEvent): void {
    const tooltip = this.admissionTooltip();
    if (!tooltip.visible) {
      return;
    }

    this.admissionTooltip.set({ ...tooltip, x: event.clientX, y: event.clientY });
  }

  hideAdmissionTooltip(): void {
    this.admissionTooltip.set({ visible: false, text: '', x: 0, y: 0 });
  }

  studentLacksAdmission(slot: SubmissionSlotView): boolean {
    const user = this.auth.user();
    return !!user && this.auth.hasRole('Student') && !this.isAdmitted(slot, user.id);
  }

  studentCanBookSlot(slot: SubmissionSlotView): boolean {
    const user = this.auth.user();
    if (!user || !this.auth.hasRole('Student')) {
      return false;
    }

    return this.isAdmitted(slot, user.id);
  }

  isSlotFinished(slot: SubmissionSlotView): boolean {
    return new Date(slot.endTime).getTime() <= Date.now();
  }

  slotGroupIds(slot: SubmissionSlotView): string[] {
    if (slot.allowedGroups?.length) {
      return slot.allowedGroups.map((group) => group.id);
    }

    return slot.allowedGroups?.map((group) => group.id) ?? [];
  }

  slotEditableGroups(slot: SubmissionSlotView): GroupView[] {
    if (slot.subject?.groups?.length) {
      return slot.subject.groups;
    }

    return this.data().groups;
  }

  filteredSlotEditableGroups(slot: SubmissionSlotView): GroupView[] {
    const query = this.slotGroupSearch().trim().toLowerCase();
    const groups = this.slotEditableGroups(slot);

    if (!query) {
      return groups;
    }

    return groups.filter((group) => group.name.toLowerCase().includes(query));
  }

  toggleSlotAllowedGroup(slot: SubmissionSlotView, groupId: string): void {
    const selected = new Set(this.slotGroupIds(slot));

    if (selected.has(groupId)) {
      selected.delete(groupId);
    } else {
      selected.add(groupId);
    }

    slot.allowedGroups = this.slotEditableGroups(slot).filter((group) => selected.has(group.id));
  }

  isAdmitted(slot: SubmissionSlotView, studentId: string): boolean {
    return !!slot.admittedStudents?.some((student) => student.id === studentId);
  }

  admittedStudents(slot: SubmissionSlotView): StudentView[] {
    return (slot.admittedStudents ?? []).map((student) => this.data().students.find((item) => item.id === student.id) ?? student);
  }

  admissionStudents(slot: SubmissionSlotView): StudentView[] {
    const groupIds = new Set(this.slotEditableGroups(slot).map((group) => group.id));
    const query = this.admissionSearch().trim().toLowerCase();
    const students = this.data().students.filter((student) => {
      if (!query) {
        return true;
      }

      return [this.fullName(student), student.group?.name]
        .filter(Boolean)
        .some((value) => value!.toLowerCase().includes(query));
    });

    if (slot.allowAllGroups || !groupIds.size) {
      return students;
    }

    return students.filter((student) => {
      const groupId = student.group?.id;
      return !!groupId && groupIds.has(groupId);
    });
  }

  bookingPosition(booking: BookingRecordView): number {
    if (!booking.slot?.id || !booking.student?.id) {
      return 0;
    }

    return this.bookingsForSlot(booking.slot.id).findIndex((item) => item.id === booking.id || item.student?.id === booking.student?.id) + 1;
  }

  isCurrentUserBooking(booking: BookingRecordView): boolean {
    return booking.student?.id === this.auth.user()?.id;
  }

  initials(person?: { firstName?: string; lastName?: string } | null): string {
    return initials(person);
  }

  fullName(person?: { firstName?: string; lastName?: string; middleName?: string | null } | null): string {
    return fullName(person);
  }

  slotTitle(slot: SubmissionSlotView): string {
    return slot.subject?.name ?? 'Окно защиты';
  }

  remindTimeLabel(value: string): string {
    return remindTimeLabel(value);
  }

  bookingModeLabel(value: string): string {
    return (
      {
        All: 'Все уведомления',
        OnlyCancellation: 'Только отмены',
        OnlyNewBooking: 'Только новые записи',
        None: 'Не присылать',
      }[value] ?? value
    );
  }

  remindTimeValue(value: string): number {
    return (
      {
        TwoWeeks: 0,
        OneWeek: 1,
        TwoDays: 2,
        FourHours: 3,
      }[value] ?? 1
    );
  }

  bookingModeValue(value: string): number | null {
    return (
      {
        None: 0,
        All: 1,
        OnlyCancellation: 2,
        OnlyNewBooking: 3,
      }[value] ?? null
    );
  }

  private prefillNotificationForm(slot: SubmissionSlotView | null): void {
    const settings = slot ? this.currentUserNotificationSettings(slot) : undefined;

    this.notificationForm.remindTimes = settings?.submissionRemindTimes?.length
      ? settings.submissionRemindTimes.map((value) => this.reminderKeysByValue[value] ?? 'OneWeek')
      : ['OneWeek', 'TwoDays'];

    this.notificationForm.bookingMode =
      typeof settings?.bookingNotificationMode === 'number'
        ? (this.bookingModeKeysByValue[settings.bookingNotificationMode] ?? 'All')
        : 'All';
  }

  roleLabel(value: string): string {
    return roleLabel(value);
  }

  formatDateTime(value?: string | null): string {
    return formatDateTime(value);
  }

  utcOffsetLabel(): string {
    return utcOffsetLabel();
  }

  calendarTitle(): string {
    return calendarTitle(this.pickerMonth());
  }

  calendarDays(): Array<{ date: Date; day: number; muted: boolean; selected: boolean }> {
    const month = this.pickerMonth();
    const first = new Date(month.getFullYear(), month.getMonth(), 1);
    const startOffset = (first.getDay() + 6) % 7;
    const start = new Date(first);
    start.setDate(first.getDate() - startOffset);
    const target = this.dateTimePickerOpen();
    const selectedValue = target ? this.getDateTimeValue(target) : '';
    const selectedDate = selectedValue ? new Date(selectedValue) : null;

    return Array.from({ length: 42 }, (_, index) => {
      const date = new Date(start);
      date.setDate(start.getDate() + index);
      return {
        date,
        day: date.getDate(),
        muted: date.getMonth() !== month.getMonth(),
        selected: !!selectedDate && date.toDateString() === selectedDate.toDateString(),
      };
    });
  }

  shiftPickerMonth(delta: number): void {
    const next = new Date(this.pickerMonth());
    next.setMonth(next.getMonth() + delta);
    this.pickerMonth.set(next);
  }

  selectPickerDate(date: Date): void {
    const target = this.dateTimePickerOpen();
    if (!target) {
      return;
    }

    const current = this.getDateTimeValue(target);
    const currentDate = current ? new Date(current) : new Date();
    const next = new Date(date);
    next.setHours(currentDate.getHours(), currentDate.getMinutes(), 0, 0);
    this.setDateTimeValue(target, toDateTimeLocal(next));
  }

  pickerTime(): string {
    const target = this.dateTimePickerOpen();
    const value = target ? this.getDateTimeValue(target) : '';
    const date = value ? new Date(value) : new Date();
    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
  }

  setPickerTime(value: string): void {
    const target = this.dateTimePickerOpen();
    const match = /^([01]\d|2[0-3]):([0-5]\d)$/.exec(value);

    if (!target || !match) {
      return;
    }

    const current = this.getDateTimeValue(target);
    const date = current ? new Date(current) : new Date();
    const hours = Number(match[1]);
    const minutes = Number(match[2]);
    date.setHours(hours, minutes, 0, 0);
    this.setDateTimeValue(target, toDateTimeLocal(date));
  }

  closeErrorModal(): void {
    this.api.clearError();
  }

  studentGroupId(student: StudentView): string {
    return student.group?.id ?? '';
  }

  setStudentGroup(student: StudentView, groupId: string): void {
    const group = this.data().groups.find((item) => item.id === groupId);
    student.group = group;
  }

  selectEditStudentGroup(student: StudentView, group: GroupView): void {
    student.group = group;
    this.closeCustomSelect();
  }

  editStudentGroupLabel(student: StudentView): string {
    return student.group?.name ?? 'Выберите группу';
  }

  subjectTeacherId(subject: SubjectView): string {
    return subject.teacher?.id ?? '';
  }

  setSubjectTeacher(subject: SubjectView, teacherId: string): void {
    subject.teacher = this.data().teachers.find((teacher) => teacher.id === teacherId) ?? null;
  }

  selectEditSubjectTeacher(subject: SubjectView, teacher: TeacherView): void {
    subject.teacher = teacher;
    this.closeCustomSelect();
  }

  editSubjectTeacherLabel(subject: SubjectView): string {
    return subject.teacher ? this.fullName(subject.teacher) : 'Выберите преподавателя';
  }

  subjectGroupIds(subject: SubjectView): string[] {
    return subject.groups?.map((group) => group.id) ?? [];
  }

  toggleSubjectGroup(subject: SubjectView, groupId: string): void {
    const selected = subject.groups ? [...subject.groups] : [];
    const index = selected.findIndex((group) => group.id === groupId);

    if (index >= 0) {
      selected.splice(index, 1);
    } else {
      const group = this.data().groups.find((item) => item.id === groupId);
      if (group) {
        selected.push(group);
      }
    }

    subject.groups = selected;
  }

  private syncRoute(url: string): void {
    const [view, section] = url.split('?')[0].split('/').filter(Boolean);

    if (view === 'login') {
      if (this.auth.user()) {
        this.router.navigateByUrl('/overview');
      }
      this.activeView.set('overview');
      return;
    }

    if (!this.auth.hasToken() && !this.auth.user()) {
      this.router.navigateByUrl('/login');
      this.activeView.set('overview');
      return;
    }

    if (view === 'slots' || view === 'bookings' || view === 'overview') {
      this.activeView.set(view);
      return;
    }

    if (view === 'admin') {
      if (!this.auth.user() || this.auth.hasRole('Admin')) {
        this.activeView.set('admin');
        this.activeAdminView.set(section === 'users' || section === 'subjects' ? section : 'groups');
      } else {
        this.activeView.set('overview');
        this.router.navigateByUrl('/overview');
      }
      return;
    }

    this.activeView.set('overview');
  }

  private showToast(text: string, tone: ToastMessage['tone']): void {
    const toast = { id: ++this.toastId, text, tone };
    this.toasts.update((items) => [...items, toast]);

    window.setTimeout(() => {
      this.toasts.update((items) => items.filter((item) => item.id !== toast.id));
    }, 3200);
  }

  private confirmDestructiveAction(dialog: ConfirmDialog): void {
    this.dateTimePickerOpen.set(null);
    this.customSelectOpen.set(null);
    this.confirmDialog.set(dialog);
  }

  private setFieldErrors(errors: Record<string, string>): boolean {
    this.fieldErrors.set(errors);

    if (Object.keys(errors).length) {
      this.showToast('Проверьте обязательные поля', 'warning');
      return false;
    }

    return true;
  }

  private validateGroupForm(): boolean {
    return this.setFieldErrors({
      ...(this.groupForm.name.trim() ? {} : { groupName: 'Введите название группы.' }),
    });
  }

  private validatePersonForm(): boolean {
    const errors: Record<string, string> = {};

    if (!this.personForm.userName.trim()) {
      errors['personUserName'] = 'Введите логин.';
    }

    if (!this.personForm.email.trim()) {
      errors['personEmail'] = 'Введите email.';
    }

    if (!this.personForm.password.trim()) {
      errors['personPassword'] = 'Введите пароль.';
    }

    if (!this.personForm.lastName.trim()) {
      errors['personLastName'] = 'Введите фамилию.';
    }

    if (!this.personForm.firstName.trim()) {
      errors['personFirstName'] = 'Введите имя.';
    }

    if (!this.personForm.birthDate) {
      errors['personBirthDate'] = 'Укажите дату рождения.';
    }

    if (this.personForm.role === 'Student' && !this.personForm.groupId) {
      errors['personGroup'] = this.data().groups.length ? 'Выберите группу.' : 'Сначала создайте группу.';
    }

    return this.setFieldErrors(errors);
  }

  private validateSubjectForm(): boolean {
    const errors: Record<string, string> = {};

    if (!this.subjectForm.name.trim()) {
      errors['subjectName'] = 'Введите название дисциплины.';
    }

    if (!this.subjectForm.teacherId) {
      errors['subjectTeacher'] = this.data().teachers.length ? 'Выберите преподавателя.' : 'Сначала создайте преподавателя.';
    }

    return this.setFieldErrors(errors);
  }

  private validateSlotForm(): boolean {
    const errors: Record<string, string> = {};

    if (!this.slotForm.subjectId) {
      errors['slotSubject'] = 'Выберите дисциплину.';
    }

    if (this.auth.hasRole('Admin') && !this.slotForm.teacherId) {
      errors['slotTeacher'] = 'Выберите преподавателя.';
    }

    if (!this.slotForm.startTime) {
      errors['slotStart'] = 'Укажите начало.';
    }

    if (!this.slotForm.endTime) {
      errors['slotEnd'] = 'Укажите окончание.';
    }

    if (!this.slotForm.maxStudents || this.slotForm.maxStudents < 1) {
      errors['slotMax'] = 'Укажите количество мест больше нуля.';
    }

    return this.setFieldErrors(errors);
  }

  private ensureAdmin(): boolean {
    if (this.auth.hasRole('Admin')) {
      return true;
    }

    this.showToast('Недостаточно прав для операции администратора', 'warning');
    this.go('overview');
    return false;
  }

  private getDateTimeValue(target: string): string {
    if (target === 'createStart') {
      return this.slotForm.startTime;
    }

    if (target === 'createEnd') {
      return this.slotForm.endTime;
    }

    const [kind, id] = target.split(':');
    if (kind.startsWith('edit')) {
      const draft = this.editSlotDraft();
      if (draft?.id === id) {
        return kind.endsWith('Start') ? (draft.startTime ?? '') : (draft.endTime ?? '');
      }
    }

    const slot = this.data().slots.find((item) => item.id === id);
    return kind.endsWith('Start') ? (slot?.startTime ?? '') : (slot?.endTime ?? '');
  }

  private setDateTimeValue(target: string, value: string): void {
    if (target === 'createStart') {
      this.onSlotStartChanged(value);
      return;
    }

    if (target === 'createEnd') {
      this.slotForm.endTime = value;
      return;
    }

    const [kind, id] = target.split(':');
    if (kind.startsWith('edit')) {
      const draft = this.editSlotDraft();
      if (!draft || draft.id !== id) {
        return;
      }

      if (kind.endsWith('Start')) {
        draft.startTime = value;
      } else {
        draft.endTime = value;
      }
      return;
    }

    const slot = this.data().slots.find((item) => item.id === id);
    if (!slot) {
      return;
    }

    if (kind.endsWith('Start')) {
      slot.startTime = value;
    } else {
      slot.endTime = value;
    }
  }

  private cloneGroup(group: GroupView): GroupView {
    return { ...group };
  }

  private cloneTeacher(teacher: TeacherView): TeacherView {
    return { ...teacher };
  }

  private cloneStudent(student: StudentView): StudentView {
    return { ...student, group: student.group ?? null };
  }

  private cloneSubject(subject: SubjectView): SubjectView {
    return {
      ...subject,
      teacher: subject.teacher ?? null,
      groups: subject.groups ? [...subject.groups] : [],
    };
  }

  private cloneSlot(slot: SubmissionSlotView): SubmissionSlotView {
    return {
      ...slot,
      subject: slot.subject ?? null,
      teacher: slot.teacher ?? null,
      allowedGroups: slot.allowedGroups ? [...slot.allowedGroups] : [],
      admittedStudents: slot.admittedStudents ? [...slot.admittedStudents] : [],
    };
  }

  private resetBookingExpansion(): void {
    this.expandedBookingSlotIds.set([]);
    this.loadedBookingSlotIds.set([]);
  }

  private refreshSlot(slotId: string): void {
    this.api.getSlot(slotId).subscribe((slot) => {
      this.applyWorkspace({ slots: this.replaceSlot(this.data().slots, slot) });
      this.selectedSlotId.set(slot.id);
    });
  }

  private replaceSlot(slots: SubmissionSlotView[], slot: SubmissionSlotView): SubmissionSlotView[] {
    return slots.some((item) => item.id === slot.id) ? slots.map((item) => (item.id === slot.id ? slot : item)) : [slot, ...slots];
  }
}
