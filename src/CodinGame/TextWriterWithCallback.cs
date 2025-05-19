using System;
using System.IO;
using System.Text;

namespace CodinGame;

public class TextWriterWithCallback(Action<string?> onWriteLine, Encoding encoding) : TextWriter
{
    public override void WriteLine(string? value)
    {
        onWriteLine(value);
    }

    public override Encoding Encoding => encoding;
}