using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SelfEmployed.Inspector.Http;

public sealed class HttpInspector : IInspector
{
    private static readonly Uri WebUri = new("https://npd.nalog.ru/check-status/", UriKind.Absolute);
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;
    private static readonly MediaTypeHeaderValue StringMediaType = new("text/plain", DefaultEncoding.WebName);

    private static readonly MediaTypeHeaderValue FormDataMediaType = new("multipart/form-data")
    {
        Parameters =
        {
            new NameValueHeaderValue("boundary", "."),
        },
    };

    private static readonly HttpClient Client;

    static HttpInspector()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = false,
            UseProxy = false,
        };

        Client = new HttpClient(handler);
    }

    public async Task<(string Inn, InspectionStatus Status)> InspectAsync(string inn, string date)
    {
        try
        {
            using var eventContent = CreateFormDataString("__EVENTTARGET", string.Empty);
            using var sendContent = CreateFormDataString("ctl00$ctl00$btSend", "Найти");
            using var dateContent = CreateFormDataString("ctl00$ctl00$tbDate", date);
            using var innContent = CreateFormDataString("ctl00$ctl00$tbINN", inn);

            using var requestContent = new MultipartFormDataContent(".");
            requestContent.Headers.ContentType = FormDataMediaType;
            requestContent.Add(eventContent);
            requestContent.Add(sendContent);
            requestContent.Add(dateContent);
            requestContent.Add(innContent);

            using var request = new HttpRequestMessage(HttpMethod.Post, WebUri)
            {
                Content = requestContent,
            };
            using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent.Contains($"{inn} является"))
                return (inn, InspectionStatus.SelfEmployed);

            if (responseContent.Contains($"{inn} не является"))
                return (inn, InspectionStatus.CommonPerson);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Error while requesting web: {e.Message}");
        }

        return (inn, InspectionStatus.PoorResponse);
    }

    [Pure]
    [NotNull]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ByteArrayContent CreateFormDataString([NotNull] string name, [NotNull] string value)
    {
        var content = new ByteArrayContent(DefaultEncoding.GetBytes(value));
        content.Headers.ContentType = StringMediaType;
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = name,
        };

        return content;
    }
}