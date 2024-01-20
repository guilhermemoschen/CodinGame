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
class MarsLanderLevel1
{
    static void Main(string[] args)
    {
        var marsSurface = new List<Point>();
        var flatSurface = new List<Point>();
        
        string[] inputs;
        var totalSurfacePoints = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.

        for (var i = 0; i < totalSurfacePoints; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var currentPoint = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));

            if (marsSurface.Count > 0 && currentPoint.Y == marsSurface.LastOrDefault().Y)
            {
                // flast Surface
                flatSurface.Add(marsSurface.LastOrDefault());
                flatSurface.Add(currentPoint);
            }

            marsSurface.Add(currentPoint);
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');

            var opportunityPosition = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
            var horizontalSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            var verticalSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            var remainingFuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            var currentRotation = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            var currentThrustPower = int.Parse(inputs[6]); // the thrust power (0 to 4).
            // a thrust power of 4 in an almost vertical position is needed to compensate for the gravity on Mars

            int targetRotation;
            int targetPower;

            if (ShouldGoLeft(opportunityPosition, flatSurface))
            {
                Console.Error.WriteLine("Should go left");
                targetRotation = -45;
            }
            else if (ShouldGoRight(opportunityPosition, flatSurface))
            {
                Console.Error.WriteLine("Should go right");
                targetRotation = 45;
            }
            else
            {
                Console.Error.WriteLine("in the flat ground");
                //horizontal speed must be limited ( ≤ 20m/s in absolute value)
                if (horizontalSpeed > 20 || horizontalSpeed < -20)
                {
                    Console.Error.WriteLine("Should decrease the horizontal speed");
                    if (horizontalSpeed > 0)
                        targetRotation = +20;
                    else
                        targetRotation = -20;
                }
                else
                {
                    targetRotation = 0;
                }
            }
            
            //vertical speed must be limited ( ≤ 40m/s in absolute value)
            if (verticalSpeed >= 0)
                targetPower = 1;
            else if (verticalSpeed < -40 + 5)
            {
                targetPower = 4;
            }
            else
            {
                targetPower = 3;
            }
            
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(string.Format("{0} {1}", targetRotation, targetPower)); // R P. R is the desired rotation angle. P is the desired thrust power.
        }
    }

    private static bool ShouldGoRight(Point opportunityPosition, List<Point> flatSurface)
    {
        return opportunityPosition.X < flatSurface.Min(x => x.X) + 5;
    }

    private static bool ShouldGoLeft(Point opportunityPosition, List<Point> flatSurface)
    {
        return opportunityPosition.X > flatSurface.Max(x => x.X) - 5;
    }
}