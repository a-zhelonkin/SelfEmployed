using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SelfEmployed.App.Extensions
{
    public static class EnumerableExtension
    {
        public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
            };

            var block = new ActionBlock<T>(body, options);

            foreach (var item in source)
                block.Post(item);

            block.Complete();
            return block.Completion;
        }
    }
}