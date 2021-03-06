using System;
using System.Threading;

namespace CSPspEmu.Utils
{
    public static class SpanExt
    {
        public static unsafe Span<T> Reinterpret<T, R>(this Span<R> Span) where T : unmanaged where R : unmanaged
        {
            fixed (R* bp = &Span.GetPinnableReference()) {
                //return new Span<T>(bp, count * sizeof(T));
                return new Span<T>(bp, Span.Length / sizeof(T));
            }
        }

    }
}