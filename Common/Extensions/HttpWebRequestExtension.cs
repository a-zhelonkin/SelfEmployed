using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SelfEmployed.Common.Extensions;

public static class HttpWebRequestExtension
{
    public static async Task AddMultipartFormDataAsync(
        [NotNull] this HttpWebRequest thiz,
        [NotNull] IReadOnlyDictionary<string, string> content
    )
    {
        var boundary = "---------------------------" + DateTime.UtcNow.Ticks.ToString("x");
        var boundaryBytes = Encoding.ASCII.GetBytes($"\r\n--{boundary}\r\n");

        await using var requestStream = thiz.GetRequestStream();

        foreach (var (name, value) in content)
        {
            requestStream.Write(boundaryBytes);
            var data = $"Content-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}";
            var dataBytes = Encoding.UTF8.GetBytes(data);
            requestStream.Write(dataBytes);
        }

        var trailerBytes = Encoding.ASCII.GetBytes($"\r\n--{boundary}--\r\n");
        requestStream.Write(trailerBytes);

        thiz.ContentType = "multipart/form-data; boundary=" + boundary;
    }
}