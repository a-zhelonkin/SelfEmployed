using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SelfEmployed.App.Extensions;

namespace SelfEmployed.App.Inspectors
{
    public sealed class WebInspector : IInspector
    {
        private const string WebUrl = "https://npd.nalog.ru/check-status/";

        public async Task<(string Inn, InspectionStatus Status)> InspectAsync(string inn, string date)
        {
            try
            {
                var content = await HttpWebRequestAsync(WebUrl, new Dictionary<string, string>
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
            catch
            {
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
            [NotNull] string url,
            [NotNull] IReadOnlyDictionary<string, string> content
        )
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

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