using System;
using System.IO;

namespace CodinGame;

public class TextReaderWithCallback(Func<string?> onReadLine) : TextReader
{
    public override string? ReadLine()
    {
        return onReadLine.Invoke();
    }
}