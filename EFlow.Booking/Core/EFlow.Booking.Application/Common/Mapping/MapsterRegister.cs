using EFlow.Booking.Application.Admins;
using EFlow.Booking.Application.BookingRecords;
using EFlow.Booking.Application.BookingRecords.Commands.Update;
using EFlow.Booking.Application.Groups;
using EFlow.Booking.Application.Groups.Commands.Update;
using EFlow.Booking.Application.Students;
using EFlow.Booking.Application.Students.Commands.Update;
using EFlow.Booking.Application.Subjects;
using EFlow.Booking.Application.Subjects.Commands.Update;
using EFlow.Booking.Application.SubmissionSlots;
using EFlow.Booking.Application.SubmissionSlots.Commands.Update;
using EFlow.Booking.Application.Teachers;
using EFlow.Booking.Application.Teachers.Commands;
using EFlow.Common.Domain.Models;
using Mapster;

namespace EFlow.Booking.Application.Common.Mapping;

public class MapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        MapAdmin(config);
        MapTeacher(config);
        MapStudent(config);
        MapGroup(config);
        MapSubject(config);
        MapSubmissionSlot(config);
        MapBooking(config);
    }

    private void MapAdmin(TypeAdapterConfig config) =>
        config.NewConfig<Admin, AdminDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UserName, src => src.Identity!.UserName);

    private void MapTeacher(TypeAdapterConfig config)
    {
        config.NewConfig<Teacher, TeacherDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.MiddleName, src => src.MiddleName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<UpdateTeacherCommand, Teacher>()
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.MiddleName, src => src.MiddleName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
    }

    private void MapStudent(TypeAdapterConfig config)
    {
        config.NewConfig<Student, StudentDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.MiddleName, src => src.MiddleName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<UpdateStudentCommand, Student>()
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.MiddleName, src => src.MiddleName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
    }

    private void MapGroup(TypeAdapterConfig config)
    {
        config.NewConfig<Group, GroupDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<UpdateGroupCommand, Group>()
            .Map(dest => dest.Name, src => src.Name)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
    }

    private void MapSubject(TypeAdapterConfig config)
    {
        config.NewConfig<Subject, SubjectDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.TeacherId, src => src.TeacherId);

        config.NewConfig<UpdateSubjectCommand, Subject>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.TeacherId, src => src.TeacherId)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
    }

    private void MapSubmissionSlot(TypeAdapterConfig config)
    {
        config.NewConfig<SubmissionSlot, SubmissionSlotDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SubjectId, src => src.SubjectId)
            .Map(dest => dest.StartTime, src => src.StartTime)
            .Map(dest => dest.EndTime, src => src.EndTime)
            .Map(dest => dest.MaxStudents, src => src.MaxStudents)
            .Map(dest => dest.Location, src => src.Location);

        config.NewConfig<UpdateSubmissionSlotCommand, SubmissionSlot>()
            .Map(dest => dest.SubjectId, src => src.SubjectId)
            .Map(dest => dest.StartTime, src => src.StartTime)
            .Map(dest => dest.EndTime, src => src.EndTime)
            .Map(dest => dest.MaxStudents, src => src.MaxStudents)
            .Map(dest => dest.Location, src => src.Location)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
    }

    private void MapBooking(TypeAdapterConfig config)
    {
        config.NewConfig<BookingRecord, BookingRecordDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.StudentId, src => src.StudentId)
            .Map(dest => dest.SlotId, src => src.SlotId)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.Student, src => src.Student)
            .Map(dest => dest.SubmissionSlot, src => src.SubmissionSlot);

        config.NewConfig<UpdateBookingRecordCommand, BookingRecord>()
            .Map(dest => dest.StudentId, src => src.StudentId)
            .Map(dest => dest.SlotId, src => src.SlotId)
            .Ignore(dest => dest.Id)
            .IgnoreNullValues(true);
    }
}