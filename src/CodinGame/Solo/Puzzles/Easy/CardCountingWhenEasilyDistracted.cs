namespace CodinGame.Solo.Puzzles.Easy;

using System;
using System.Globalization;
using System.Linq;

public static class CardCountingWhenEasilyDistracted
{
    public static void Main(string[] args)
    {
        string[] inputs;

        if (args.Length == 0)
        {
            inputs = new string[2];
            inputs[0] = Console.ReadLine()!;
            inputs[1] = Console.ReadLine()!;
        }
        else
        {
            inputs = args;
        }

        foreach (var input in inputs)
        {
            Console.Error.WriteLine(input);
        }

        var streamOfConsciousness = inputs[0];
        var bustThreshold = int.Parse(inputs[1], CultureInfo.InvariantCulture);

        if (bustThreshold <= 1)
        {
            Console.WriteLine($"0%");
            return;
        }

        var validCards = "A23456789TJQK".ToCharArray();
        char[] validCardsWithThreshold;
        if (bustThreshold > 10)
        {
            validCardsWithThreshold = (char[])validCards.Clone();
        }
        else
        {
            validCardsWithThreshold = new char[bustThreshold - 1];
            validCardsWithThreshold[0] = 'A';
            for (int i = 2; i < bustThreshold; i++)
            {
                validCardsWithThreshold[i - 1] += i.ToString(CultureInfo.InvariantCulture)[0];
            }
        }

        var observcedCards = streamOfConsciousness
            .Split('.')
            .Where(observation => observation.All(c => validCards.Contains(c)))
            .SelectMany(observation => observation.ToArray())
            .ToList();

        var observcedCardsLessThan = observcedCards
            .Where(observation => validCardsWithThreshold.Contains(observation))
            .ToList();

        var remainingDeck = 52 - observcedCards.Count;
        var remainingValidCards = ((bustThreshold - 1) * 4) - observcedCardsLessThan.Count;

        var percentageChance = (int)Math.Round((decimal)remainingValidCards * 100 / remainingDeck);

        Console.WriteLine($"{percentageChance}%");
    }
}

public static class CardCountingWhenEasilyDistractedTestCases
{
    public static readonly string[] Test1 = new string[]
    {
        "222.333.444.some distraction.555.5.678.678.678.678.another distraction.9999.TTTT.JJJJ.QQQQ.KKKK.AAAA",
        "4",
    };

    public static readonly string[] Test6 = new string[]
    {
        "sound of surveillance camera moving.pushy cocktail waitress.972KQ.TATTOO.QANON.pushy cocktail waitress.pushy cocktail waitress.TAT.937A2247.MINIskirts!.I'm so smart.hungry.mob boss.9T8.68",
        "3",
    };
}