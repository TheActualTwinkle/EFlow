using System.Reactive.Linq;

namespace EFlow.Common.Extensions;

public static class ReactiveExtensions
{
    public static IObservable<T> DoAsync<T>(this IObservable<T> source, Func<T, Task> asyncAction) =>
        source.SelectMany(async item =>
        {
            await asyncAction(item);

            return item;
        });
}