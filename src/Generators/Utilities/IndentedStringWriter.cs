using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Generators.Utilities;

internal sealed class IndentedStringWriter : IDisposable {
    public StringBuilder Builder;
    public int Indent;
    private bool tabsPending;
    public IndentedStringWriter() => Builder = new();
    public IndentedStringWriter(int initialCapacity) => Builder = new(initialCapacity);
    
    public IndentedStringWriter(StringBuilder builder) {
        Builder = builder;
    }
    
    private StringBuilder WritePendingTabs() {
        if (tabsPending) {
            Builder.Append(' ', Indent * 4);
            tabsPending = false;
        }
        return Builder;
    }

    public void Write([InterpolatedStringHandlerArgument("")] ref IndentedStringWriterInterpolationHandler handler) { }
    public void Write(byte value) => WritePendingTabs().Append(value);
    public void Write(sbyte value) => WritePendingTabs().Append(value);
    public void Write(short value) => WritePendingTabs().Append(value);
    public void Write(ushort value) => WritePendingTabs().Append(value);
    public void Write(int value) => WritePendingTabs().Append(value);
    public void Write(uint value) => WritePendingTabs().Append(value);
    public void Write(long value) => WritePendingTabs().Append(value);
    public void Write(ulong value) => WritePendingTabs().Append(value);
    public void Write(string value) => WritePendingTabs().Append(value);
    public void Write(char value) => WritePendingTabs().Append(value.ToString(CultureInfo.InvariantCulture));
    public void Write(float value) => WritePendingTabs().Append(value.ToString(CultureInfo.InvariantCulture));
    public void Write(double value) => WritePendingTabs().Append(value.ToString(CultureInfo.InvariantCulture));

    public void WriteLine() {
        Builder.AppendLine(); 
        tabsPending = true;
    }

    public void WriteLine(string text) {
        WritePendingTabs().Append(text).AppendLine(); 
        tabsPending = true;
    }

    public void WriteLine([InterpolatedStringHandlerArgument("")] ref IndentedStringWriterInterpolationHandler handler) {
        Builder.AppendLine(); 
        tabsPending = true;
    }

    /// <summary>Writes the specified text, writes a '{' character in a new line and increases indentation.</summary>
    public void BeginScope([InterpolatedStringHandlerArgument("")] ref IndentedStringWriterInterpolationHandler handler) {
        WriteLine(" {");
        Indent++;
    }
    public void EndScope() {
        Indent--;
        WriteLine("}");

    }
    public void EndScope([InterpolatedStringHandlerArgument("")] ref IndentedStringWriterInterpolationHandler handler) => EndScope();
    
    public void Dispose() { }
    
    public string ToStringAndClear() {
        string result = Builder.ToString();
        Builder.Clear();
        tabsPending = true;
        return result;
    }

    [InterpolatedStringHandler]
    internal readonly struct IndentedStringWriterInterpolationHandler {
        private readonly IndentedStringWriter writer;
        public IndentedStringWriterInterpolationHandler(int literalLength, int formattedCount, IndentedStringWriter writer) {
            this.writer = writer;
        }

        public void AppendLiteral(string literal) => writer.Write(literal);
        public void AppendFormatted(byte value) => writer.Write(value);
        public void AppendFormatted(sbyte value) => writer.Write(value);
        public void AppendFormatted(short value) => writer.Write(value);
        public void AppendFormatted(ushort value) => writer.Write(value);
        public void AppendFormatted(int value) => writer.Write(value);
        public void AppendFormatted(uint value) => writer.Write(value);
        public void AppendFormatted(long value) => writer.Write(value);
        public void AppendFormatted(ulong value) => writer.Write(value);
        public void AppendFormatted(char value) => writer.Write(value);
        public void AppendFormatted(string value) => writer.Write(value);
        public void AppendFormatted(float value) => writer.Write(value);
        public void AppendFormatted(double value) => writer.Write(value);
        public void AppendFormatted<T>(T value) {
            if (value is IFormattable t)
                writer.Write(t.ToString(null, CultureInfo.InvariantCulture));
            else
                writer.Write(value?.ToString() ?? "");
        }
    }
}