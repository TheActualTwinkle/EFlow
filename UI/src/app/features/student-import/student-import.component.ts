import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import type { components } from '../../api/contracts';
import { ApiService } from '../../core/api.service';
import { AppCheckboxComponent } from '../../shared/ui/app-checkbox/app-checkbox.component';
import { AppIconComponent } from '../../shared/ui/app-icon/app-icon.component';
import { AppSelectComponent } from '../../shared/ui/app-select/app-select.component';
import { AppSelectOption } from '../../shared/ui/app-select/app-select-option';
import { AppStepProgressComponent } from '../../shared/ui/app-step-progress/app-step-progress.component';
import { HoverHintDirective } from '../../shared/ui/hover-hint/hover-hint.directive';
import {
  defaultStudentImportFields,
  StudentImportField,
  studentImportFieldOptions,
  StudentsImportResult,
} from './student-import.models';

type GroupView = components['schemas']['GroupView'];

@Component({
  selector: 'app-student-import',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AppCheckboxComponent,
    AppIconComponent,
    AppSelectComponent,
    AppStepProgressComponent,
    HoverHintDirective,
  ],
  templateUrl: './student-import.component.html',
  styleUrl: './student-import.component.less',
})
export class StudentImportComponent {
  @Input({ required: true }) groups: GroupView[] = [];
  @Output() imported = new EventEmitter<StudentsImportResult>();

  readonly fieldOptions = studentImportFieldOptions;
  readonly requiredFields: StudentImportField[] = ['LastName', 'FirstName', 'Email', 'UserName', 'Password', 'BirthDate'];
  readonly importSteps = ['Файл и группа', 'Сопоставление колонок'];

  selectedGroupId = '';
  file: File | null = null;
  fileName = '';
  hasHeaderRow = false;
  fields: StudentImportField[] = [...defaultStudentImportFields];
  previewRows: string[][] = [];
  totalRows = 0;
  result: StudentsImportResult | null = null;
  error = '';
  importing = false;
  importModalOpen = false;
  currentStep: 1 | 2 = 1;
  groupSearch = '';

  constructor(
    private readonly api: ApiService,
    private readonly changeDetector: ChangeDetectorRef,
  ) {}

  get columnIndexes(): number[] {
    const count = Math.max(this.fields.length, ...this.previewRows.map((row) => row.length), 0);

    return Array.from({ length: count }, (_, index) => index);
  }

  get canImport(): boolean {
    return !!this.file &&
      !!this.selectedGroupId &&
      this.fields.length > 0 &&
      this.missingRequiredFields().length === 0 &&
      this.duplicateFields().length === 0 &&
      !this.importing;
  }

  get selectedGroupName(): string {
    return this.groups.find((group) => group.id === this.selectedGroupId)?.name ?? 'Выберите группу';
  }

  get groupOptions(): AppSelectOption[] {
    return this.groups.map((group) => ({ value: group.id, label: group.name }));
  }

  get filteredGroupOptions(): AppSelectOption[] {
    const term = this.groupSearch.trim().toLowerCase();
    const groups = term
      ? this.groups.filter((group) => group.name.toLowerCase().includes(term))
      : this.groups;

    const selectedGroup = this.groups.find((group) => group.id === this.selectedGroupId);
    const visibleGroups = selectedGroup && !groups.some((group) => group.id === selectedGroup.id)
      ? [selectedGroup, ...groups]
      : groups;

    return visibleGroups.map((group) => ({ value: group.id, label: group.name }));
  }

  get canGoToMapping(): boolean {
    return !!this.selectedGroupId && !!this.file && this.previewRows.length > 0;
  }

  get importRowCount(): number {
    return Math.max(this.totalRows - (this.hasHeaderRow ? 1 : 0), 0);
  }

  openImportModal(): void {
    this.importModalOpen = true;
    this.currentStep = 1;
    this.error = '';
  }

  closeImportModal(): void {
    if (this.importing) {
      return;
    }

    this.importModalOpen = false;
  }

  goToMapping(): void {
    if (!this.canGoToMapping) {
      return;
    }

    this.currentStep = 2;
  }

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.file = file;
    this.fileName = file?.name ?? '';
    this.result = null;
    this.error = '';

    if (!file) {
      this.previewRows = [];
      this.fields = [...defaultStudentImportFields];
      this.totalRows = 0;

      return;
    }

    const text = await file.text();
    const lines = text
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter(Boolean);

    this.totalRows = lines.length;
    this.previewRows = lines.slice(0, 3).map((line) => this.parseCsvLine(line));
    this.syncFieldCount();
    this.changeDetector.markForCheck();
  }

  updateField(index: number, value: string): void {
    if (!this.fieldOptions.some((option) => option.value === value)) {
      return;
    }

    const field = value as StudentImportField;

    this.fields[index] = field;
    this.result = null;
    this.error = '';
  }

  importStudents(): void {
    if (!this.file || !this.selectedGroupId || !this.canImport) {
      return;
    }

    this.importing = true;
    this.result = null;
    this.error = '';

    this.api.importStudentsCsv(this.selectedGroupId, this.file, this.fields, this.hasHeaderRow)
      .pipe(finalize(() => this.importing = false))
      .subscribe({
        next: (result) => {
          this.result = result;
          this.importModalOpen = false;
          this.imported.emit(result);
        },
        error: (error) => {
          this.error = typeof error?.error === 'string' && error.error.trim() ? error.error : 'Не удалось импортировать студентов.';
        },
      });
  }

  missingRequiredFields(): StudentImportField[] {
    return this.requiredFields.filter((field) => !this.fields.includes(field));
  }

  duplicateFields(): StudentImportField[] {
    const mappedFields = this.fields.filter((field) => field !== 'Ignore');

    return mappedFields.filter((field, index) => mappedFields.indexOf(field) !== index);
  }

  fieldLabel(field: StudentImportField): string {
    return this.fieldOptions.find((option) => option.value === field)?.label ?? field;
  }

  cell(row: string[], index: number): string {
    return row[index] ?? '';
  }

  private syncFieldCount(): void {
    const columnCount = this.columnIndexes.length;

    this.fields = Array.from(
      { length: columnCount },
      (_, index) => this.fields[index] ?? defaultStudentImportFields[index] ?? 'Ignore',
    );
  }

  private parseCsvLine(line: string): string[] {
    const values: string[] = [];
    let current = '';
    let insideQuotes = false;

    for (let index = 0; index < line.length; index++) {
      const char = line[index];

      if (char === '"') {
        if (insideQuotes && line[index + 1] === '"') {
          current += '"';
          index++;
          continue;
        }

        insideQuotes = !insideQuotes;
        continue;
      }

      if (char === ';' && !insideQuotes) {
        values.push(current.trim());
        current = '';
        continue;
      }

      current += char;
    }

    values.push(current.trim());

    return values;
  }
}
