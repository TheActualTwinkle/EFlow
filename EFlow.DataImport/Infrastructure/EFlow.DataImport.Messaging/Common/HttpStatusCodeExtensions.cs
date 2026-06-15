using System.Net;

namespace EFlow.DataImport.Messaging.Common;

public static class HttpStatusCodeExtensions
{
    public static bool IsSuccess(this HttpStatusCode statusCode) =>
        (int)statusCode is >= 200 and <= 299;
}
