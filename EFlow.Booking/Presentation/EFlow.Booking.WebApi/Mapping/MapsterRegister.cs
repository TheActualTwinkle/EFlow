using EFlow.Booking.Application.Admins;
using EFlow.Booking.Application.BookingRecords;
using EFlow.Booking.Application.Groups;
using EFlow.Booking.Application.Students;
using EFlow.Booking.Application.Subjects;
using EFlow.Booking.Application.SubmissionSlots;
using EFlow.Booking.Application.Teachers;
using EFlow.Booking.Domain.Admins;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Domain.Subjects;
using Mapster;

namespace EFlow.Booking.WebApi.Mapping;

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
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UserName, src => src.Id.Value.ToString());

    private void MapTeacher(TypeAdapterConfig config) =>
        config.NewConfig<Teacher, TeacherDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.MiddleName, src => src.MiddleName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

    private void MapStudent(TypeAdapterConfig config) =>
        config.NewConfig<Student, StudentDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.GroupId, src => src.GroupId.Value)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.MiddleName, src => src.MiddleName)
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

    private void MapGroup(TypeAdapterConfig config) =>
        config.NewConfig<Group, GroupDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name);

    private void MapSubject(TypeAdapterConfig config) =>
        config.NewConfig<Subject, SubjectDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.TeacherId, src => src.TeacherId.Value)
            .Map(dest => dest.GroupIds, src => src.GroupIds.Select(id => id.Value));

    private void MapSubmissionSlot(TypeAdapterConfig config) =>
        config.NewConfig<SubmissionSlot, SubmissionSlotDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.SubjectId, src => src.SubjectId.Value)
            .Map(dest => dest.TeacherId, src => src.TeacherId.Value)
            .Map(dest => dest.StartTime, src => src.StartTime)
            .Map(dest => dest.EndTime, src => src.EndTime)
            .Map(dest => dest.MaxStudents, src => src.MaxStudents)
            .Map(dest => dest.Location, src => src.Location)
            .Map(dest => dest.Comment, src => src.Comment);

    private void MapBooking(TypeAdapterConfig config) =>
        config.NewConfig<BookingRecord, BookingRecordDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.StudentId, src => src.StudentId.Value)
            .Map(dest => dest.SlotId, src => src.SlotId.Value)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
}
