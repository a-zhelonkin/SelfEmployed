using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SelfEmployed.Inspector.Tests;

internal abstract class BenchmarkTestBase
{
    protected async Task<double> Profile(int iterations, Func<Task> func)
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        await func();

        var watch = new Stopwatch();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        watch.Start();
        for (var i = 0; i < iterations; i++) await func();
        watch.Stop();

        return watch.Elapsed.TotalMilliseconds;
    }
}