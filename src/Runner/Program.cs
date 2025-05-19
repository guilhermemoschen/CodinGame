using CodinGame;
using CodinGame.Solo.Puzzles.Easy;

using var runner = RunnerBuilder.Create()
    .WithTestCase(SixDegreesOfKevinBaconTestCases.TwoDegreesOfKevinBaconReads)
    .Run(SixDegreesOfKevinBacon.Main);