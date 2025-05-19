using System;
using CodinGame.TestCases;

namespace CodinGame;

public class RunnerBuilder
{
    private readonly LocalRunner _runner;

    private RunnerBuilder(LocalRunner runner)
    {
        _runner = runner;
    }

    public static RunnerBuilder Create()
    {
        return new RunnerBuilder(new LocalRunner());
    }

    public RunnerBuilder WithTestCase(TestCase testCase)
    {
        _runner.SetTestCase(testCase);

        return this;
    }

    public LocalRunner Run(Action entryPoint)
    {
        entryPoint();

        return _runner;
    }
}