using Generators.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Runtime.CompilerServices;

// Not the same as the default one from System
[InterpolatedStringHandler]
internal readonly ref struct DefaultInterpolatedStringHandler
{
    private readonly StringBuilder builder;

    public DefaultInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        builder = StringBuilderPool.Rent(literalLength + (formattedCount * 11));
    }

    public readonly void AppendFormatted<T>(T value)
    {
        builder.Append(value);
    }

    public readonly void AppendFormatted(int value)
    {
        builder.Append(value);
    }

    public readonly void AppendFormatted(string value)
    {
        builder.Append(value);
    }

    public readonly unsafe void AppendFormatted(ReadOnlyMemory<char> value)
    {
        if (MemoryMarshal.TryGetString(value, out string text, out int start, out int length))
        {
            builder.Append(text, start, length);
        }
        else if (MemoryMarshal.TryGetArray(value, out ArraySegment<char> segment))
        {
            builder.Append(segment.Array, segment.Offset, segment.Count);
        }
        else
        {
            fixed (char* ptr = value.Span)
            {
                builder.Append(ptr, value.Length);
            }
        }
    }

    public readonly void AppendLiteral(string str)
    {
        builder.Append(str);
    }

    public readonly string ToStringAndClear()
    {
        string result = builder.ToString();
        StringBuilderPool.Return(builder);
        return result;
    }
}