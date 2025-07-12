using EFlow.Application.Admins;
using EFlow.Application.Students;
using EFlow.Application.Students.Commands.Update;
using EFlow.Application.Teachers;
using EFlow.Application.Teachers.Commands;
using EFlow.Domain.Models;
using Mapster;

namespace EFlow.Application.Common.Mapping;

public class MapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        MapAdmin(config);
        MapTeacher(config);
        MapStudent(config);
    }

    private void MapAdmin(TypeAdapterConfig config) =>
        config.NewConfig<Admin, AdminDto>()
            .Map(dest => dest.IdentityId, src => src.IdentityId)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UserName, src => src.Identity!.UserName);

    private void MapTeacher(TypeAdapterConfig config)
    {
        config.NewConfig<Teacher, TeacherDto>()
            .Map(dest => dest.IdentityId, src => src.IdentityId)
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
            .Ignore(dest => dest.IdentityId)
            .IgnoreNullValues(true);
    }

    private void MapStudent(TypeAdapterConfig config)
    {
        config.NewConfig<Student, StudentDto>()
            .Map(dest => dest.IdentityId, src => src.IdentityId)
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
            .Ignore(dest => dest.IdentityId)
            .IgnoreNullValues(true);
    }
}