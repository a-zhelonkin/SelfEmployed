using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SelfEmployed.Inspector.Api;

public sealed class ApiInspector : IInspector
{
    private const string ApiUrl = "https://statusnpd.nalog.ru:443/api/v1/tracker/taxpayer_status";

    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60),
    };

    public async Task<(string Inn, InspectionStatus Status)> InspectAsync(string inn, string date)
    {
        var response = await _httpClient.PostAsync(
            ApiUrl,
            new StringContent(
                $"{{\"inn\": \"{inn}\", \"requestDate\": \"{date}\"}}",
                Encoding.UTF8,
                "application/json"
            )
        );

        var content = await response.Content.ReadAsStringAsync();
        var status = content.Contains($"{inn} является")
            ? InspectionStatus.SelfEmployed
            : InspectionStatus.CommonPerson;

        return (inn, status);
    }
}