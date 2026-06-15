export type StudentImportField =
  | 'Ignore'
  | 'LastName'
  | 'FirstName'
  | 'MiddleName'
  | 'Email'
  | 'UserName'
  | 'Password'
  | 'BirthDate';

export interface StudentsImportResult {
  totalCount: number;
  importedCount: number;
  failedCount: number;
  errors: StudentImportRowError[];
}

export interface StudentImportRowError {
  rowNumber: number;
  message: string;
}

export const studentImportFieldOptions: Array<{ value: StudentImportField; label: string }> = [
  { value: 'LastName', label: 'Фамилия' },
  { value: 'FirstName', label: 'Имя' },
  { value: 'MiddleName', label: 'Отчество' },
  { value: 'Email', label: 'Email' },
  { value: 'UserName', label: 'Логин' },
  { value: 'Password', label: 'Пароль' },
  { value: 'BirthDate', label: 'Дата рождения' },
  { value: 'Ignore', label: 'Игнорировать' },
];

export const defaultStudentImportFields: StudentImportField[] = [
  'LastName',
  'FirstName',
  'MiddleName',
  'Email',
  'UserName',
  'Password',
  'BirthDate',
];
