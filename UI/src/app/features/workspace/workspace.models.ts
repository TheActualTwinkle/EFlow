import type { components } from '../../api/contracts';

type Schemas = components['schemas'];
type BookingRecordView = Schemas['BookingRecordView'];
type GroupView = Schemas['GroupView'];
type StudentView = Schemas['StudentView'];
type SubjectView = Schemas['SubjectView'];
type SubmissionSlotView = Schemas['SubmissionSlotView'];
type TeacherView = Schemas['TeacherView'];

export type WorkspaceView = 'overview' | 'slots' | 'bookings' | 'admin';
export type AdminView = 'groups' | 'users' | 'subjects';
export type UserTable = 'teachers' | 'students';

export type ModalView =
  | 'createSlot'
  | 'editSlot'
  | 'bookSlot'
  | 'notifications'
  | 'createGroup'
  | 'createUser'
  | 'createSubject'
  | 'editGroup'
  | 'editTeacher'
  | 'editStudent'
  | 'editSubject'
  | 'admissions'
  | 'accountEmail'
  | 'accountPassword'
  | null;

export interface ToastMessage {
  id: number;
  text: string;
  tone: 'success' | 'warning' | 'error';
}

export interface WorkspaceData {
  groups: GroupView[];
  students: StudentView[];
  teachers: TeacherView[];
  subjects: SubjectView[];
  slots: SubmissionSlotView[];
  bookings: BookingRecordView[];
}

export const emptyWorkspaceData = (): WorkspaceData => ({
  groups: [],
  students: [],
  teachers: [],
  subjects: [],
  slots: [],
  bookings: [],
});

export const createPersonForm = () => ({
  role: 'Student',
  userName: '',
  password: '',
  email: '',
  firstName: '',
  middleName: '',
  lastName: '',
  birthDate: '',
  groupId: '',
});

export const createSlotForm = () => ({
  subjectId: '',
  teacherId: '',
  startTime: '',
  endTime: '',
  maxStudents: 12,
  allowAllGroups: true,
  allowedGroupIds: [] as string[],
  location: '',
  comment: '',
});

export const createAccountEmailForm = () => ({
  userId: '',
  email: '',
});

export const createAccountPasswordForm = () => ({
  userId: '',
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
  adminReset: false,
});
