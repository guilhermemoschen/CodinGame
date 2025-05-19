using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CodinGame.TestCases;

namespace CodinGame;

public sealed class LocalRunner : IDisposable
{
    private Queue<string> _reads = null!;
    private Queue<string> _writes = null!;
    private readonly TextReaderWithCallback _readerWithCallback;
    private readonly TextWriterWithCallback _writerWithCallback;
    private readonly TextWriter _consoleOut;
    private readonly TextReader _consoleIn;

    internal LocalRunner()
    {
        _readerWithCallback = new TextReaderWithCallback(ReadLine);
        _writerWithCallback = new TextWriterWithCallback(WriteLine, Console.Out.Encoding);

        _consoleOut = Console.Out;
        _consoleIn = Console.In;

        Console.SetOut(_writerWithCallback);
        Console.SetIn(_readerWithCallback);
    }

    internal void SetTestCase(TestCase testCase)
    {
        _reads = new Queue<string>(testCase.Reads);
        _writes = new Queue<string>(testCase.Writes);
    }

    private string? ReadLine()
    {
        if (_reads.Count == 0)
        {
            Debug.WriteLine("Unexpected read line");
            return null;
        }

        return _reads.Dequeue();
    }

    private void WriteLine(string? value)
    {
        _consoleOut.WriteLine(value);
        if (_writes.Count == 0)
        {
            Debug.WriteLine("Unexpected write line");
        }
        else
        {
            var expectedValue = _writes.Dequeue();
            Debug.Assert(expectedValue == value, $"Unexpected value to write a string. Received: '{value}' expected: '{expectedValue}'");
        }
    }

    public void Dispose()
    {
        _readerWithCallback.Dispose();
        _writerWithCallback.Dispose();
    }


    public static LocalRunner Create()
    {
        return new LocalRunner();
    }
}