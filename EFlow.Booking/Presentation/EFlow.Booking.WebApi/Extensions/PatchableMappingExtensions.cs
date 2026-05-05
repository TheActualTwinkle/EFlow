using FluentPatcher;

namespace EFlow.Booking.WebApi.Extensions;

public static class PatchableMappingExtensions
{
    extension<TSource>(Patchable<TSource> patchable)
    {
        public Patchable<TTarget> Map<TTarget>(Func<TSource, TTarget> mapper) =>
            patchable.HasValue ?
                Patchable.Set(mapper(patchable.Value!)) :
                Patchable.NotSet<TTarget>();
    }
}
