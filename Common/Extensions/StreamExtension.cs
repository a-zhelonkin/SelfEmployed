using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SelfEmployed.Common.Extensions;

public static class StreamExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUtf8([NotNull] this Stream thiz, [NotNull] string value)
    {
        thiz.Write(Encoding.UTF8.GetBytes(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask WriteUtf8Async([NotNull] this Stream thiz, [NotNull] string value)
    {
        return thiz.WriteAsync(Encoding.UTF8.GetBytes(value));
    }
}