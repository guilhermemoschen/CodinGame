using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Temperature
{
    public const int MaxPositive = 5526;
    public const int MinNegative = -273;
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine()!); // the number of temperatures to analyse
        if (N == 0)
        {
            Console.WriteLine(0);
            return;
        }

        string TEMPS = Console.ReadLine()!; // the N temperatures expressed as integers ranging from -273 to 5526

        // To debug: Console.Error.WriteLine("Debug messages...");

        int positiveTemperature = 0;
        int negativeTemperature = 0;

        var temperatures = TEMPS.Split(' ');

        foreach (var temp in temperatures)
        {
            var temperature = Convert.ToInt32(temp);
            if (temperature < 0)
            {
                if (negativeTemperature == 0 || negativeTemperature < temperature)
                    negativeTemperature = temperature;
            }
            else if (temperature > 0)
            {
                if (positiveTemperature == 0 || positiveTemperature > temperature)
                    positiveTemperature = temperature;
            }
        }

        if (Math.Abs(negativeTemperature) == Math.Abs(positiveTemperature))
            Console.WriteLine(positiveTemperature);
        else if (negativeTemperature != 0 && positiveTemperature == 0)
            Console.WriteLine(negativeTemperature);
        else if (negativeTemperature == 0 && positiveTemperature != 0)
            Console.WriteLine(positiveTemperature);
        else if (Math.Abs(negativeTemperature) < Math.Abs(positiveTemperature))
            Console.WriteLine(negativeTemperature);
        else
            Console.WriteLine(positiveTemperature);
    }
}