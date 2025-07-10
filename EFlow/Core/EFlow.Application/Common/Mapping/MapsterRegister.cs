using EFlow.Application.Teachers;
using EFlow.Application.Teachers.Commands.Update;
using EFlow.Domain.Models;
using Mapster;

namespace EFlow.Application.Common.Mapping;

public class MapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config) =>
        MapTeacher(config);

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
}