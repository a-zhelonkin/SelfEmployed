using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SelfEmployed.App.Extensions;
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
            var inns = File.ReadLines(InnsPath)
                .Where(inn => !Secretary.Handled(inn));

            await InspectInnsAsync(inns, date);

            while (Secretary.PoorResponseInns.Any())
            {
                await Console.Out.WriteLineAsync("30 seconds pause for cooldown");
                await Task.Delay(30_000);

                await Console.Out.WriteLineAsync("Re-Inspecting poor response inns");
                await InspectInnsAsync(Secretary.PoorResponseInns, date);
            }

            await Console.Out.WriteLineAsync("Done. Press any key to close");
            Console.ReadKey();
        }

        private static async Task InspectInnsAsync(IEnumerable<string> inns, string date)
        {
            using var bus = new BlockingCollection<(string inn, InspectionStatus status)>();

            var inspectionTask = inns.ParallelForEachAsync(inn =>
            {
                return Inspector.InspectAsync(inn, date).ContinueWith(t =>
                {
                    bus.Add(t.Result);
                });
            }).ContinueWith(_ =>
            {
                bus.CompleteAdding();
            });

            var innIndex = 0;
            var stopwatch = Stopwatch.StartNew();
            foreach (var (inn, status) in bus.GetConsumingEnumerable())
            {
                await (status switch
                {
                    InspectionStatus.SelfEmployed => Secretary.CommitSelfEmployedAsync(inn),
                    InspectionStatus.CommonPerson => Secretary.CommitCommonPersonAsync(inn),
                    InspectionStatus.PoorResponse => Secretary.CommitPoorResponseAsync(inn),
                    _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
                });

                Console.Title = $"{++innIndex / stopwatch.Elapsed.TotalSeconds:F2} inns per seconds";
            }

            await inspectionTask;
        }
    }
}