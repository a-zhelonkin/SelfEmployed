using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;

namespace SelfEmployed.Common.Extensions;

public static class EnumerableExtension
{
    private static readonly ExecutionDataflowBlockOptions DefaultOptions = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        SingleProducerConstrained = true,
    };

    public static Task ParallelForEachAsync<T>([NotNull] this IEnumerable<T> items, [NotNull] Func<T, Task> action)
    {
        var block = new ActionBlock<T>(action, DefaultOptions);

        foreach (var item in items)
            block.Post(item);

        block.Complete();
        return block.Completion;
    }
}