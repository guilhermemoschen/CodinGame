namespace CodinGame.Solo.Puzzles.Medium;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static CodinGame.Solo.Puzzles.Medium.MayanCalculation;

public static class MayanCalculation
{
    public static void Main()
    {
        var characterSize = Console.ReadLine()!.Split(' ');
        Console.Error.WriteLine($"characterSize {string.Join(' ', characterSize)}");
        int characterWidth = int.Parse(characterSize[0], CultureInfo.InvariantCulture);
        int characterHeight = int.Parse(characterSize[1], CultureInfo.InvariantCulture);

        var alphabetAsChar = new char[characterHeight, characterWidth * 20];
        for (var i = 0; i < characterHeight; i++)
        {
            var line = Console.ReadLine()!.ToArray();
            for (var j = 0; j < line.Length; j++)
            {
                alphabetAsChar[i, j] = line[j];
            }
        }

        var numerals = Numerals.Create(characterHeight, characterWidth, alphabetAsChar);

        var number1 = ReadNumber(characterWidth, characterHeight, numerals);
        Console.Error.WriteLine($"Number1 {number1}");

        var number2 = ReadNumber(characterWidth, characterHeight, numerals);
        Console.Error.WriteLine($"Number2 {number2}");

        var operation = Console.ReadLine()!;
        var resultAsDecimal = Calculate(number1.DecimalRepresentation, number2.DecimalRepresentation, operation);
        Console.Error.WriteLine($"operation {number1.DecimalRepresentation} {operation} {number2.DecimalRepresentation} = {resultAsDecimal}");

        var result = numerals.ConvertToNumbers(resultAsDecimal);
        Console.Error.WriteLine($"result {result}");

        foreach (var number in result.Digits)
        {
            Console.Write(number.ConverToString());
        }
    }

    private static Number ReadNumber(int characterWidth, int characterHeight, Numerals numerals)
    {
        var number = new Number();
        int number1Digits = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture) / characterHeight;
        for (var digit = 0; digit < number1Digits; digit++)
        {
            var numberAsChar = new char[characterHeight, characterWidth];
            for (var i = 0; i < characterHeight; i++)
            {
                var line = Console.ReadLine()!.ToArray();
                for (var j = 0; j < line.Length; j++)
                {
                    numberAsChar[i, j] = line[j];
                }
            }

            number.Digits.Add(numerals.Digits.Find(n => n.Equal(numberAsChar))!);
        }

        return number;
    }

    private static decimal Calculate(decimal number1, decimal number2, string operation)
    {
        return operation switch
        {
            "+" => number1 + number2,
            "-" => number1 - number2,
            "*" => number1 * number2,
            "/" => number1 / number2,
            _ => 0,
        };
    }

    internal class Numerals
    {
        private Numerals()
        {
        }

        public List<Digit> Digits { get; } = new List<Digit>();

        public static Numerals Create(int characterHeight, int characterWidth, char[,] alphabetAsChar)
        {
            var alphabet = new Numerals();

            for (var i = 0; i < 20; i++)
            {
                var character = new char[characterHeight, characterWidth];
                for (var rows = 0; rows < characterHeight; rows++)
                {
                    for (var columns = 0; columns < characterWidth; columns++)
                    {
                        character[rows, columns] = alphabetAsChar[rows, columns + (i * characterWidth)];
                    }
                }

                var number = new Digit()
                {
                    DecimalRepresentation = i,
                    Character = character,
                    CharacterHeight = characterHeight,
                    CharacterWidth = characterWidth,
                };

                alphabet.Digits.Add(number);
            }

            return alphabet;
        }

        public Number ConvertToNumbers(decimal numberInDecimal)
        {
            var number = new Number();

            var maxPower = 0;

            decimal powerCurrent = numberInDecimal;

            while (powerCurrent >= 20)
            {
                maxPower += 1;
                powerCurrent = powerCurrent / 20;
            }

            var current = numberInDecimal;
            for (var p = maxPower; p >= 0; p--)
            {
                var decimalMumber = Math.Floor(current / (decimal)Math.Pow(20, p));
                number.Digits.Add(Digits.Find(n => n.DecimalRepresentation == decimalMumber)!);
                current -= decimalMumber * (int)Math.Pow(20, p);
            }

            return number;
        }
    }

    internal class Number
    {
        public decimal DecimalRepresentation
        {
            get
            {
                var result = 0;
                for (var i = 0; i < Digits.Count; i++)
                {
                    result += Digits[^(i + 1)].DecimalRepresentation * (int)Math.Pow(20, i);
                }

                return result;
            }
        }

        public List<Digit> Digits { get; } = new List<Digit>();

        public override string ToString()
        {
            return $"{DecimalRepresentation} ({string.Join(',', Digits.Select(d => d.DecimalRepresentation))})";
        }
    }

    internal class Digit
    {
        public int DecimalRepresentation { get; init; }

        public char[,] Character { get; init; } = null!;

        public int CharacterWidth { get; init; }

        public int CharacterHeight { get; init; }

        public bool Equal(char[,] other)
        {
            if (Character.Length != other.Length)
            {
                return false;
            }

            for (var i = 0; i < CharacterHeight; i++)
            {
                for (var j = 0; j < CharacterWidth; j++)
                {
                    if (Character[i, j] != other[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public string ConverToString()
        {
            var output = string.Empty;
            for (var i = 0; i < CharacterHeight; i++)
            {
                for (var j = 0; j < CharacterWidth; j++)
                {
                    output += Character[i, j];
                }

                output += Environment.NewLine;
            }

            return output;
        }

        public override string ToString()
        {
            return $"{DecimalRepresentation}{Environment.NewLine}{ConverToString()}";
        }
    }
}
