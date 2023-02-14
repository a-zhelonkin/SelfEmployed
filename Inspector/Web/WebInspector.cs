using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using SelfEmployed.Common.Extensions;

namespace SelfEmployed.Inspector.Web;

public sealed class WebInspector : IInspector
{
    private const string Boundary = ".";
    private const string ContentType = $"multipart/form-data;boundary={Boundary}";
    private static readonly Uri WebUri = new("https://npd.nalog.ru/check-status/", UriKind.Absolute);
    private static readonly byte[] BoundaryBytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}\r\n");
    private static readonly byte[] TrailerBytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}--\r\n");
    private static readonly byte[] DefaultData;

    static WebInspector()
    {
        ServicePointManager.UseNagleAlgorithm = false;
        ServicePointManager.Expect100Continue = false;
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;

        using var stream = new MemoryStream();
        stream.Write(BoundaryBytes);
        stream.WriteUtf8("Content-Disposition:form-data;name=__EVENTTARGET\r\n\r\n");
        stream.Write(BoundaryBytes);
        stream.WriteUtf8("Content-Disposition:form-data;name=ctl00$ctl00$btSend\r\n\r\nНайти");
        DefaultData = stream.ToArray();
    }

    public async Task<(string Inn, InspectionStatus Status)> InspectAsync(string inn, string date)
    {
        try
        {
            var request = WebRequest.CreateHttp(WebUri);
            request.Method = "POST";
            request.ContinueTimeout = 0;
            request.ContentType = ContentType;
            request.AllowWriteStreamBuffering = false;
            request.AuthenticationLevel = AuthenticationLevel.None;
            request.ImpersonationLevel = TokenImpersonationLevel.Anonymous;

            await using var requestStream = await request.GetRequestStreamAsync();
            await requestStream.WriteAsync(DefaultData);
            await requestStream.WriteAsync(BoundaryBytes);
            await requestStream.WriteUtf8Async($"Content-Disposition:form-data;name=ctl00$ctl00$tbINN\r\n\r\n{inn}");
            await requestStream.WriteAsync(BoundaryBytes);
            await requestStream.WriteUtf8Async($"Content-Disposition:form-data;name=ctl00$ctl00$tbDate\r\n\r\n{date}");
            await requestStream.WriteAsync(TrailerBytes);

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();
            if (responseStream is null)
                return (inn, InspectionStatus.PoorResponse);

            using var reader = new StreamReader(responseStream);
            var content = await reader.ReadToEndAsync();

            if (content.Contains($"{inn} является"))
                return (inn, InspectionStatus.SelfEmployed);

            if (content.Contains($"{inn} не является"))
                return (inn, InspectionStatus.CommonPerson);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Error while requesting web: {e.Message}");
        }

        return (inn, InspectionStatus.PoorResponse);
    }
}