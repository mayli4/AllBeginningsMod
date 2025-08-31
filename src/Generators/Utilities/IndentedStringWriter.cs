using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Generators.Utilities;

internal sealed class IndentedStringWriter : IDisposable
{
    [SuppressMessage("Style", "IDE1006:Naming rule violation",
        Justification = "'PascalCase' naming style required for preservation of original source.")]
    public const string DefaultIndentString = "    ";

    public readonly StringBuilder Builder;
    public readonly string IndentString;
    public int Indent;

    private bool tabsPending;

    public IndentedStringWriter()
    {
        Builder = StringBuilderPool.Rent(8192);
        Indent = 0;
        IndentString = DefaultIndentString;
        tabsPending = false;
    }

    public IndentedStringWriter(int capacity)
    {
        Builder = StringBuilderPool.Rent(capacity);
        Indent = 0;
        IndentString = DefaultIndentString;
        tabsPending = false;
    }

    public IndentedStringWriter(StringBuilder builder)
    {
        Builder = builder;
        Indent = 0;
        IndentString = DefaultIndentString;
        tabsPending = false;
    }

    private void WriteTabs()
    {
        if (tabsPending)
        {
            if (ReferenceEquals(IndentString, DefaultIndentString))
            {
                Builder.Append(' ', 4 * Indent);
            }
            else
            {
                for (int i = 0; i < Indent; i++)
                {
                    Builder.Append(IndentString);
                }
            }

            tabsPending = false;
        }
    }

    public override string ToString()
    {
        return Builder.ToString();
    }

    public IndentedStringWriter Clear()
    {
        Builder.Clear();
        return this;
    }

    /// <summary>Generates a string with the contents on the writer and clears the string builder for reusing it.</summary>
    /// <returns></returns>
    public string ToStringAndClear()
    {
        string result = Builder.ToString();
        Clear();
        return result;
    }

    #region Write methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(byte value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(sbyte value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(short value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(ushort value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(int value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(uint value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(long value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(ulong value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(float value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(double value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(char value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(string value)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe IndentedStringWriter Write(ReadOnlySpan<char> values)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        fixed (char* ptr = values)
        {
            Builder.Append(ptr, values.Length);
        }

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(char[] values)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(values);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(char[] values, int start, int count)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.Append(values, start, count);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter Write(
        [InterpolatedStringHandlerArgument("")] ref IndentedStringWriterInterpolatedStringHandler handler)
    {
        tabsPending = false;
        return this;
    }

    public IndentedStringWriter WriteLine()
    {
        Builder.AppendLine();
        tabsPending = true;
        return this;
    }

    public IndentedStringWriter WriteLine(string text)
    {
        if (tabsPending)
        {
            WriteTabs();
        }

        Builder.AppendLine(text);
        tabsPending = true;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IndentedStringWriter WriteLine(
        [InterpolatedStringHandlerArgument("")] ref IndentedStringWriterInterpolatedStringHandler handler)
    {
        WriteLine();
        return this;
    }


    /// <summary>
    /// Returns resources to pools. <br/>
    /// This instance MUST NOT be used for ANYTHING (not even ToString()) after being disposed.
    /// </summary>
    public void Dispose()
    {
        if (Builder != null)
        {
            StringBuilderPool.Return(Builder);
        }
    }

    #endregion // Write methods
}