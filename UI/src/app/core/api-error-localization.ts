const apiErrorMessages: Record<string, string> = {
  'BusinessRule.Violation': 'Нарушено бизнес-правило.',
  'BusinessRule.AllowedGroupIdsMustBeEmptyWhenAllowAllGroupsIsTrueRule': 'Очистите список групп, если окно защиты доступно всем группам.',
  'BusinessRule.AllowedGroupIdsMustNotBeEmptyWhenAllowAllGroupsIsFalseRule': 'Выберите хотя бы одну группу для окна защиты.',
  'BusinessRule.AllowedGroupIdsMustNotContainDuplicatesRule': 'Список групп не должен содержать дубликаты.',
  'BusinessRule.AllowedGroupIdsMustBeWithinSubjectGroupIds': 'Выбранные группы должны быть из числа групп, связанных с дисциплиной.',
  'BusinessRule.CreationTimeMustBeInPastRule': 'Дата создания должна быть в прошлом.',
  'BusinessRule.GroupIdsMustNotBeEmpty': 'Выберите хотя бы одну группу.',
  'BusinessRule.GroupIdsMustNotDuplicateRule': 'Список групп не должен содержать дубликаты.',
  'BusinessRule.GroupNameMustBeProperLengthRule': 'Название группы должно быть допустимой длины.',
  'BusinessRule.GroupNameMustUniqueRule': 'Группа с таким названием уже существует.',
  'BusinessRule.MaxStudentsMustBePositiveRule': 'Количество мест должно быть больше нуля.',
  'BusinessRule.StartTimeMustBeBeforeEndTimeRule': 'Время начала должно быть раньше времени окончания.',
  'BusinessRule.StudentBirthDateMustBeInPastRule': 'Дата рождения студента должна быть в прошлом.',
  'BusinessRule.StudentCannotBeMovedToSameGroupRule': 'Студент уже находится в этой группе.',
  'BusinessRule.StudentFirstNameMustBeProperLengthRule': 'Имя студента должно быть допустимой длины.',
  'BusinessRule.StudentGroupMustBeAllowedToSubmissionSlotRule': 'Группа студента не допущена к этому окну защиты.',
  'BusinessRule.StudentLastNameMustBeProperLengthRule': 'Фамилия студента должна быть допустимой длины.',
  'BusinessRule.StudentMiddleNameMustBeProperLengthRule': 'Отчество студента должно быть допустимой длины.',
  'BusinessRule.StudentMustHaveAdmissionToSubmissionSlotRule': 'У студента нет допуска к этому окну защиты.',
  'BusinessRule.StudentMustNotBeBookedToSubmissionSlotTwiceRule': 'Студент уже записан на это окно защиты.',
  'BusinessRule.SubjectNameMustBeProperLengthRule': 'Название дисциплины должно быть допустимой длины.',
  'BusinessRule.SubmissionSlotMustHaveAvailablePlacesRule': 'На этом окне защиты нет свободных мест.',
  'BusinessRule.TeacherBirthDateMustBeInPastRule': 'Дата рождения преподавателя должна быть в прошлом.',
  'BusinessRule.TeacherFirstNameMustBeProperLengthRule': 'Имя преподавателя должно быть допустимой длины.',
  'BusinessRule.TeacherLastNameMustBeProperLengthRule': 'Фамилия преподавателя должна быть допустимой длины.',
  'BusinessRule.TeacherMiddleNameMustBeProperLengthRule': 'Отчество преподавателя должно быть допустимой длины.',
  'BusinessRule.TeacherMustOwnSubjectRule': 'Преподаватель должен вести выбранную дисциплину.',
  'BusinessRule.UserMustNotBeInUsersWithoutNotificationsRule': 'Для пользователя уже отключены уведомления.',
};

const validationErrorMessages: Record<string, string> = {
  'Validation.BirthDate.NotEmptyValidator': 'Укажите дату рождения.',
  'Validation.Comment.MaximumLengthValidator': 'Комментарий не должен превышать 1023 символа.',
  'Validation.Email.EmailValidator': 'Введите корректный email.',
  'Validation.Email.MaximumLengthValidator': 'Email не должен превышать 256 символов.',
  'Validation.Email.NotEmptyValidator': 'Введите email.',
  'Validation.EndTime.GreaterThanValidator': 'Время окончания должно быть позже времени начала.',
  'Validation.EndTime.NotEmptyValidator': 'Укажите окончание.',
  'Validation.EndTime.PredicateValidator': 'Время окончания должно быть в UTC.',
  'Validation.FirstName.MaximumLengthValidator': 'Имя не должно превышать 31 символ.',
  'Validation.FirstName.NotEmptyValidator': 'Введите имя.',
  'Validation.GroupId.NotEmptyValidator': 'Выберите группу.',
  'Validation.GroupIds.NotEmptyValidator': 'Выберите хотя бы одну группу.',
  'Validation.LastName.MaximumLengthValidator': 'Фамилия не должна превышать 31 символ.',
  'Validation.LastName.NotEmptyValidator': 'Введите фамилию.',
  'Validation.Location.MaximumLengthValidator': 'Место не должно превышать 127 символов.',
  'Validation.MaxStudents.GreaterThanValidator': 'Укажите количество мест больше нуля.',
  'Validation.Name.MaximumLengthValidator': 'Название не должно превышать 127 символов.',
  'Validation.Name.NotEmptyValidator': 'Введите название.',
  'Validation.Password.MaximumLengthValidator': 'Пароль не должен превышать 63 символа.',
  'Validation.Password.MinimumLengthValidator': 'Пароль должен содержать минимум 6 символов.',
  'Validation.Password.NotEmptyValidator': 'Введите пароль.',
  'Validation.StartTime.LessThanValidator': 'Время начала должно быть раньше времени окончания.',
  'Validation.StartTime.NotEmptyValidator': 'Укажите начало.',
  'Validation.StartTime.PredicateValidator': 'Время начала должно быть в UTC.',
  'Validation.SubjectId.NotEmptyValidator': 'Выберите дисциплину.',
  'Validation.TeacherId.NotEmptyValidator': 'Выберите преподавателя.',
  'Validation.UserName.MaximumLengthValidator': 'Логин не должен превышать 31 символ.',
  'Validation.UserName.MinimumLengthValidator': 'Логин должен содержать минимум 3 символа.',
  'Validation.UserName.NotEmptyValidator': 'Введите логин.',
};

const genericValidationErrorMessages: Record<string, string> = {
  EmailValidator: 'Введите корректный email.',
  GreaterThanValidator: 'Значение должно быть больше допустимого минимума.',
  LessThanValidator: 'Значение должно быть меньше допустимого максимума.',
  MaximumLengthValidator: 'Значение слишком длинное.',
  MinimumLengthValidator: 'Значение слишком короткое.',
  NotEmptyValidator: 'Заполните обязательное поле.',
  PredicateValidator: 'Значение недопустимо.',
};

export function localizeApiErrorCode(code: string | undefined, fallbackCode?: string): string | null {
  if (code && apiErrorMessages[code]) {
    return apiErrorMessages[code];
  }

  return fallbackCode ? apiErrorMessages[fallbackCode] ?? null : null;
}

export function localizeValidationErrorCode(code: string | undefined): string | null {
  if (!code) {
    return null;
  }

  const exactMessage = validationErrorMessages[code];
  if (exactMessage) {
    return exactMessage;
  }

  const validator = code.split('.').at(-1);

  return validator ? genericValidationErrorMessages[validator] ?? null : null;
}
