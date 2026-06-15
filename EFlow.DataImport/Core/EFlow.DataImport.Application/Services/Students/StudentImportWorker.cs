using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using EFlow.DataImport.Messaging.Common;
using EFlow.DataImport.Application.Models.Students;
using EFlow.DataImport.Messaging.Booking.Abstractions;
using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Application.Services.Students;

public sealed class StudentImportWorker(IBookingStudentImportClient bookingStudentImportClient)
    : IStudentImportWorker
{
    private const char Separator = ';';
    private const string BirthDateFormat = "dd.MM.yyyy";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<StudentImportProxyResult> ImportStudentsAsync(
        StudentImportProxyRequest request,
        CancellationToken cancellationToken)
    {
        var parseResult = await ParseStudentsAsync(request, cancellationToken);

        if (parseResult.InvalidMappingMessage is not null)
            return StudentImportProxyResult.FromStatus(HttpStatusCode.BadRequest, parseResult.InvalidMappingMessage);

        if (parseResult.Students.Count == 0)
            return CreateJsonResult(
                new StudentsImportResult
                {
                    TotalCount = parseResult.TotalCount,
                    ImportedCount = 0,
                    FailedCount = parseResult.Errors.Count,
                    Errors = parseResult.Errors
                });

        var bookingResult = await bookingStudentImportClient.ImportStudentsAsync(
            request.GroupId,
            parseResult.Students.Select(MapToBookingStudent).ToArray(),
            cancellationToken);

        if (!bookingResult.StatusCode.IsSuccess())
            return MapToProxyResult(bookingResult);

        var bookingImportResult = JsonSerializer.Deserialize<StudentsImportResult>(
            bookingResult.Body ?? string.Empty,
            JsonOptions);

        if (bookingImportResult is null)
            return MapToProxyResult(bookingResult);

        return CreateJsonResult(
            new StudentsImportResult
            {
                TotalCount = parseResult.TotalCount,
                ImportedCount = bookingImportResult.ImportedCount,
                FailedCount = parseResult.Errors.Count + bookingImportResult.FailedCount,
                Errors = parseResult.Errors.Concat(bookingImportResult.Errors).ToArray()
            });
    }

    private async Task<StudentParseResult> ParseStudentsAsync(
        StudentImportProxyRequest request,
        CancellationToken cancellationToken)
    {
        var fieldsResult = ParseFields(request.Fields);

        if (fieldsResult.Error is not null)
            return new StudentParseResult { InvalidMappingMessage = fieldsResult.Error };

        var fields = fieldsResult.Fields!;
        var students = new List<ImportedStudent>();
        var errors = new List<StudentImportRowError>();
        var totalCount = 0;

        using var reader = new StreamReader(
            request.FileStream,
            Encoding.UTF8,
            leaveOpen: true);

        if (request.HasHeaderRow)
            await reader.ReadLineAsync(cancellationToken);

        var rowNumber = request.HasHeaderRow ? 1 : 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            rowNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            totalCount++;

            var parseResult = TryParseStudent(line, fields, rowNumber);

            if (parseResult.Error is not null)
            {
                errors.Add(parseResult.Error);

                continue;
            }

            students.Add(parseResult.Student!);
        }

        return new StudentParseResult
        {
            TotalCount = totalCount,
            Students = students,
            Errors = errors
        };
    }

    private static (IReadOnlyList<StudentImportField>? Fields, string? Error) ParseFields(IReadOnlyList<string> fields)
    {
        var parsedFields = new List<StudentImportField>();

        foreach (var field in fields)
        {
            if (!Enum.TryParse<StudentImportField>(field, true, out var parsedField))
                return (null, $"Unknown column field '{field}'");

            parsedFields.Add(parsedField);
        }

        StudentImportField[] requiredFields =
        [
            StudentImportField.LastName,
            StudentImportField.FirstName,
            StudentImportField.Email,
            StudentImportField.UserName,
            StudentImportField.Password,
            StudentImportField.BirthDate
        ];

        if (requiredFields.Any(field => !parsedFields.Contains(field)))
            return (null, "Column fields must include last name, first name, email, username, password and birth date");

        var mappedFields = parsedFields.Where(field => field != StudentImportField.Ignore).ToArray();

        return mappedFields.Distinct().Count() == mappedFields.Length ?
            (parsedFields, null) :
            (null, "Column fields must not contain duplicate mapped fields");
    }

    private static (ImportedStudent? Student, StudentImportRowError? Error) TryParseStudent(
        string line,
        IReadOnlyList<StudentImportField> fields,
        int rowNumber)
    {
        var values = ParseCsvLine(line);

        if (values.Count < fields.Count)
            return (null, new StudentImportRowError
            {
                RowNumber = rowNumber,
                Message = "Row has fewer columns than the provided field mapping"
            });

        var valuesByField = new Dictionary<StudentImportField, string?>();

        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];

            if (field == StudentImportField.Ignore)
                continue;

            valuesByField[field] = Normalize(values[i]);
        }

        var birthDateValue = GetRequiredValue(valuesByField, StudentImportField.BirthDate);

        if (!DateOnly.TryParseExact(
                birthDateValue,
                BirthDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var birthDate))
            return (null, new StudentImportRowError
            {
                RowNumber = rowNumber,
                Message = $"Invalid birth date. Expected format: {BirthDateFormat}"
            });

        return (new ImportedStudent
        {
            RowNumber = rowNumber,
            LastName = GetRequiredValue(valuesByField, StudentImportField.LastName),
            FirstName = GetRequiredValue(valuesByField, StudentImportField.FirstName),
            MiddleName = GetOptionalValue(valuesByField, StudentImportField.MiddleName),
            Email = NormalizeAddress(GetRequiredValue(valuesByField, StudentImportField.Email)),
            UserName = NormalizeAddress(GetRequiredValue(valuesByField, StudentImportField.UserName)),
            Password = GetRequiredValue(valuesByField, StudentImportField.Password),
            BirthDate = birthDate
        }, null);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var currentChar = line[i];

            if (currentChar == '"')
            {
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;

                    continue;
                }

                insideQuotes = !insideQuotes;

                continue;
            }

            if (currentChar == Separator &&
                !insideQuotes)
            {
                values.Add(current.ToString());
                current.Clear();

                continue;
            }

            current.Append(currentChar);
        }

        values.Add(current.ToString());

        return values;
    }

    private static string GetRequiredValue(Dictionary<StudentImportField, string?> valuesByField, StudentImportField field) =>
        GetOptionalValue(valuesByField, field) ?? string.Empty;

    private static string? GetOptionalValue(Dictionary<StudentImportField, string?> valuesByField, StudentImportField field) =>
        valuesByField.TryGetValue(field, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeAddress(string value)
    {
        var normalized = value.Trim();

        if (normalized.Length >= 2 &&
            normalized[0] == '<' &&
            normalized[^1] == '>')
            normalized = normalized[1..^1].Trim();

        return normalized;
    }

    private static StudentImportProxyResult CreateJsonResult(StudentsImportResult result) =>
        new()
        {
            StatusCode = HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(result, JsonOptions),
            ContentType = "application/json"
        };

    private static BookingImportedStudent MapToBookingStudent(ImportedStudent student) =>
        new()
        {
            RowNumber = student.RowNumber,
            UserName = student.UserName,
            Password = student.Password,
            Email = student.Email,
            FirstName = student.FirstName,
            MiddleName = student.MiddleName,
            LastName = student.LastName,
            BirthDate = student.BirthDate
        };

    private static StudentImportProxyResult MapToProxyResult(BookingStudentImportResult result) =>
        new()
        {
            StatusCode = result.StatusCode,
            Body = result.Body,
            ContentType = result.ContentType
        };
}