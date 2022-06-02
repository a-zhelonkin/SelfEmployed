using System;
using System.IO;
using System.Threading.Tasks;
using SelfEmployed.App.Inspectors;
using SelfEmployed.App.Secretaries;

namespace SelfEmployed.App
{
    internal static class Program
    {
        private const string InnsPath = @"inns.txt";

        private static readonly IInspector Inspector = new WebInspector();
        private static readonly ISecretary Secretary = new FileSecretary();

        private static async Task Main()
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            foreach (var inn in File.ReadLines(InnsPath))
            {
                if (Secretary.Exists(inn))
                    continue;

                try
                {
                    var status = await Inspector.InspectAsync(inn, date);

                    await (status switch
                    {
                        InspectionStatus.SelfEmployed => Secretary.CommitSelfEmployedAsync(inn),
                        InspectionStatus.CommonPerson => Secretary.CommitCommonPersonAsync(inn),
                        InspectionStatus.PoorResponse => Secretary.CommitPoorResponseAsync(inn),
                        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
                    });
                }
                catch
                {
                    await Secretary.CommitPoorResponseAsync(inn);
                    await Task.Delay(60_000);
                }
            }
        }
    }
}