export function formatDateTime(value?: string | null): string {
  if (!value) {
    return 'Дата не указана';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).format(new Date(value));
}

export function utcOffsetLabel(): string {
  const offset = -new Date().getTimezoneOffset();
  const sign = offset >= 0 ? '+' : '-';
  const absolute = Math.abs(offset);
  const hours = Math.floor(absolute / 60).toString().padStart(2, '0');
  const minutes = (absolute % 60).toString().padStart(2, '0');
  return `UTC${sign}${hours}:${minutes}`;
}

export function calendarTitle(date: Date): string {
  return new Intl.DateTimeFormat('ru-RU', { month: 'long', year: 'numeric' }).format(date);
}

export function addMinutesToDateTimeLocal(value: string, minutes: number): string {
  const date = new Date(value);
  date.setMinutes(date.getMinutes() + minutes);
  return toDateTimeLocal(date);
}

export function toDateTimeLocal(date: Date): string {
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60_000);
  return local.toISOString().slice(0, 16);
}

export function toApiDateTime(value: string): string {
  return new Date(value).toISOString();
}

export function minutesPerStudentLabel(startTime: string, endTime: string, maxStudents: number): string {
  if (!startTime || !endTime || !maxStudents) {
    return 'Для расчета времени на студента укажите время и количество мест';
  }

  const minutes = (new Date(endTime).getTime() - new Date(startTime).getTime()) / 60_000;
  if (!Number.isFinite(minutes) || minutes <= 0) {
    return 'Проверьте начало и окончание';
  }

  return `~${Math.ceil(minutes / maxStudents)} минут на студента`;
}
