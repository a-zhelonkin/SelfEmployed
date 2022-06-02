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
        private static readonly ISecretary Secretory = new FileSecretary();

        private static async Task Main()
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            foreach (var inn in File.ReadLines(InnsPath))
            {
                if (Secretory.Exists(inn))
                    continue;

                try
                {
                    var status = await Inspector.InspectAsync(inn, date);

                    await (status switch
                    {
                        InspectionStatus.SelfEmployed => Secretory.CommitSelfEmployedAsync(inn),
                        InspectionStatus.CommonPerson => Secretory.CommitCommonPersonAsync(inn),
                        InspectionStatus.PoorResponse => Secretory.CommitPoorResponseAsync(inn),
                        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
                    });
                }
                catch
                {
                    await Secretory.CommitPoorResponseAsync(inn);
                    await Task.Delay(60_000);
                }
            }
        }
    }
}