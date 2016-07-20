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
using System.Threading;

class HorseRacingDuals
{
    static void Main(string[] args)
    {
        var n = Convert.ToInt32(Console.ReadLine());
        var lowestOffset = int.MaxValue;
        var strengths = new List<int>();
        for (var i = 0; i < n; i++)
            strengths.Add(Convert.ToInt32(Console.ReadLine()));

        strengths.Sort();

        for (var i = 1; i < strengths.Count; i++)
        {
            var offset = strengths[i] - strengths[i - 1];
            if (offset < lowestOffset)
            {
                lowestOffset = offset;
            }
        }

        Console.WriteLine(lowestOffset);
    }
}