export function initials(person?: { firstName?: string; lastName?: string } | null): string {
  return `${person?.lastName?.[0] ?? 'E'}${person?.firstName?.[0] ?? 'F'}`.toUpperCase();
}

export function fullName(person?: { firstName?: string; lastName?: string; middleName?: string | null } | null): string {
  if (!person) {
    return 'Не назначено';
  }

  return [person.lastName, person.firstName, person.middleName].filter(Boolean).join(' ');
}

export function remindTimeLabel(value: string): string {
  return (
    {
      TwoWeeks: 'За две недели',
      OneWeek: 'За неделю',
      TwoDays: 'За два дня',
      FourHours: 'За 4 часа',
    }[value] ?? value
  );
}

export function roleLabel(value: string): string {
  return (
    {
      Admin: 'Администратор',
      Teacher: 'Преподаватель',
      Student: 'Студент',
      Guest: 'Гость',
    }[value] ?? value
  );
}
