namespace CodinGame.Solo.Puzzles.Easy;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

public static class RetroTypewriterArt
{
    [SuppressMessage("Minor Code Smell", "S1643:Strings should not be concatenated using '+' in a loop", Justification = "Simple case")]
    public static void Main(string[] args)
    {
        var abbreviations = new Dictionary<string, string>()
        {
            ["sp"] = " ",
            ["bS"] = @"\",
            ["sQ"] = "'",
            ["nl"] = Environment.NewLine,
        };

        var recipe = Console.ReadLine()!;

        var output = string.Empty;

        foreach (var chunks in recipe.Split(' '))
        {
            var abbreviation = abbreviations.Keys.SingleOrDefault(k => chunks.EndsWith(k, StringComparison.InvariantCultureIgnoreCase));
            string character;
            var count = 0;
            if (abbreviation is null)
            {
                character = chunks.Last().ToString();
                count = Convert.ToInt32(chunks.Substring(0, chunks.Length - 1), CultureInfo.InvariantCulture);
            }
            else
            {
                character = abbreviations[abbreviation];
                count = character == Environment.NewLine
                    ? 1
                    : Convert.ToInt32(chunks.Substring(0, chunks.Length - 2), CultureInfo.InvariantCulture);
            }

            output += string.Concat(Enumerable.Repeat(character, count));
        }

        Console.WriteLine(output);
    }
}
