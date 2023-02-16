using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SelfEmployed.Common.Extensions;

public static class StreamExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBytes([NotNull] this Stream thiz, [NotNull] byte[] buffer)
    {
        thiz.Write(buffer, 0, buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteBytesAsync([NotNull] this Stream thiz, [NotNull] byte[] buffer)
    {
        return thiz.WriteAsync(buffer, 0, buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUtf8([NotNull] this Stream thiz, [NotNull] string value)
    {
        thiz.WriteBytes(Encoding.UTF8.GetBytes(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteUtf8Async([NotNull] this Stream thiz, [NotNull] string value)
    {
        return thiz.WriteBytesAsync(Encoding.UTF8.GetBytes(value));
    }
}