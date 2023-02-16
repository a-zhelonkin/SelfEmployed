using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using SelfEmployed.Inspector.Tcp;

namespace SelfEmployed.Inspector.Tests.Tcp;

[UsedImplicitly]
[TestFixture(TestOf = typeof(TcpInspector))]
internal sealed class TcpInspectorTests : BenchmarkTestBase
{
    private const int Iterations = 100;

    private readonly IInspector _inspector = new TcpInspector();

    [Test]
    public async Task InspectAsync_SelfEmployed()
    {
        var result1 = await _inspector.InspectAsync("775126899903", "2023-02-12");
        var result2 = await _inspector.InspectAsync("772837764305", "2023-02-12");

        Assert.AreEqual(InspectionStatus.SelfEmployed, result1.Status);
        Assert.AreEqual(InspectionStatus.SelfEmployed, result2.Status);
    }

    [Test]
    public async Task InspectAsync_Benchmark()
    {
        var elapsed = await Profile(Iterations, async () =>
        {
            var result = await _inspector.InspectAsync("772303681564", "2023-02-12");
            Console.WriteLine(result);
        });

        Console.WriteLine($"Time Elapsed {elapsed} ms; Per operation {elapsed / Iterations}");
    }
}