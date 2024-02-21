namespace CodinGame.Solo.Puzzles.Easy;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public static class AscIIArt
{
    private const string AllowedLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ?";

    [SuppressMessage("Minor Code Smell", "S1643:Strings should not be concatenated using '+' in a loop", Justification = "Small string")]
    public static void Main()
    {
        var letterWidth = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
        var letterHeight = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
        var targetText = Console.ReadLine()!.ToUpper(CultureInfo.InvariantCulture);
        var dictionary = new List<string>();

        for (var i = 0; i < letterHeight; i++)
        {
            dictionary.Add(Console.ReadLine()!);
        }

        for (var i = 0; i < letterHeight; i++)
        {
            var finalRowOutput = string.Empty;

            foreach (var letter in targetText)
            {
                if (AllowedLetters.Contains(letter))
                {
                    var letterIndex = AllowedLetters.IndexOf(letter);
                    finalRowOutput += dictionary[i].Substring(letterIndex * letterWidth, letterWidth);
                }
                else
                {
                    var letterIndex = AllowedLetters.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
                    finalRowOutput += dictionary[i].Substring(letterIndex * letterWidth, letterWidth);
                }
            }

            Console.WriteLine(finalRowOutput);
        }
    }
}