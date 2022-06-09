using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using SelfEmployed.App.Extensions;

namespace SelfEmployed.App.Inspectors
{
    public sealed class WebInspector : IInspector
    {
        private static readonly Uri WebUri = new("https://npd.nalog.ru/check-status/", UriKind.Absolute);

        static WebInspector()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 256;
        }

        public async Task<(string Inn, InspectionStatus Status)> InspectAsync(string inn, string date)
        {
            try
            {
                var content = await HttpWebRequestAsync(WebUri, new Dictionary<string, string>
                {
                    {"__EVENTTARGET", ""},
                    {"__EVENTARGUMENT", ""},
                    {"__VIEWSTATEGENERATOR", "112E02C5"},
                    {"ctl00$ctl00$tbINN", inn},
                    {"ctl00$ctl00$tbDate", date},
                    {"ctl00$ctl00$btSend", "Найти"},
                });

                return (inn, DetectStatus(inn, content));
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"Error while requesting web: {e.Message}");
                return (inn, InspectionStatus.PoorResponse);
            }
        }

        private static InspectionStatus DetectStatus([NotNull] string inn, string content)
        {
            if (content is null)
                return InspectionStatus.PoorResponse;

            if (content.Contains($"{inn} является"))
                return InspectionStatus.SelfEmployed;

            if (content.Contains($"{inn} не является"))
                return InspectionStatus.CommonPerson;

            return InspectionStatus.PoorResponse;
        }

        private static async Task<string> HttpWebRequestAsync(
            [NotNull] Uri uri,
            [NotNull] IReadOnlyDictionary<string, string> content
        )
        {
            var request = WebRequest.CreateHttp(uri);
            request.Method = "POST";
            request.ContinueTimeout = 0;
            request.AllowWriteStreamBuffering = false;
            request.AuthenticationLevel = AuthenticationLevel.None;
            request.ImpersonationLevel = TokenImpersonationLevel.Anonymous;

            await request.AddMultipartFormDataAsync(content);

            using var response = await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();
            if (responseStream is null)
                return null;

            using var reader = new StreamReader(responseStream);
            return await reader.ReadToEndAsync();
        }
    }
}