using System;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class TheDescent
{
    static void Main(string[] args)
    {

        // game loop
        while (true)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            var shipPoint = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
            var mountainHights = new List<int>();

            for (int i = 0; i < 8; i++)
            {
                //int MH = int.Parse(Console.ReadLine()); // represents the height of one mountain, from 9 to 0. Mountain heights are provided from left to right.
                mountainHights.Add(int.Parse(Console.ReadLine()));
            }
            var heightestMountain = mountainHights.Max();
            var indexOfHeightestMountain = mountainHights.IndexOf(heightestMountain);


            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            if (shipPoint.X == indexOfHeightestMountain)
                Console.WriteLine("FIRE");
            else
                Console.WriteLine("HOLD");
        }
    }
}