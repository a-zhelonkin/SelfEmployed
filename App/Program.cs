using System;
using System.Collections.Concurrent;
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

        private static async Task Main()
        {
            var inspector = new WebInspector();
            var secretary = new FileSecretary();

            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var inns = File.ReadLines(InnsPath)
                .Where(inn => !secretary.Exists(inn));

            using var bus = new BlockingCollection<(string inn, InspectionStatus status)>();

            var inspectionTask = inns.ParallelForEachAsync(inn =>
            {
                return inspector.InspectAsync(inn, date).ContinueWith(t =>
                {
                    bus.Add(t.Result);
                });
            }).ContinueWith(_ =>
            {
                bus.CompleteAdding();
            });

            foreach (var (inn, status) in bus.GetConsumingEnumerable())
            {
                await (status switch
                {
                    InspectionStatus.SelfEmployed => secretary.CommitSelfEmployedAsync(inn),
                    InspectionStatus.CommonPerson => secretary.CommitCommonPersonAsync(inn),
                    InspectionStatus.PoorResponse => secretary.CommitPoorResponseAsync(inn),
                    _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
                });
            }

            await inspectionTask;
        }
    }
}