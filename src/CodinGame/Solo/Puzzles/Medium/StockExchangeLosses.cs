namespace CodinGame.Solo.Puzzles.Medium;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class StockExchangeLosses
{
    public static void Main(string[] args)
    {
        int n;
        List<int> stockValues;
        if (args.Length == 0)
        {
            n = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            Console.Error.WriteLine($"{n}");
            stockValues = Console.ReadLine()!
                .Split(' ')
                .Select(v => int.Parse(v, CultureInfo.InvariantCulture))
                .ToList();
            Console.Error.WriteLine($"{string.Join(' ', stockValues)}");
        }
        else
        {
            n = int.Parse(args[0]);
            stockValues = args[1]
                .Split(' ')
                .Select(v => int.Parse(v, CultureInfo.InvariantCulture))
                .ToList();
        }

        var maximumLoss = 0;
        var topValue = stockValues[0];
        var previousValue = stockValues[0];

        for (var i = 1; i < stockValues.Count; i++)
        {
            // going up
            if (previousValue < stockValues[i] && topValue < stockValues[i])
            {
                topValue = stockValues[i];
            }
            else if (previousValue > stockValues[i])
            {
                var loss = stockValues[i] - topValue;

                if (loss < maximumLoss)
                {
                    maximumLoss = loss;
                }
            }

            previousValue = stockValues[i];
        }

        Console.WriteLine($"{maximumLoss}");
    }

    public static class TestCases
    {
        public static string[] Test01 =>
        [
            "6",
            "3 2 4 2 1 5",
        ];

        public static string[] Test02 =>
        [
            "6",
            "5 3 4 2 3 1",
        ];
    }
}