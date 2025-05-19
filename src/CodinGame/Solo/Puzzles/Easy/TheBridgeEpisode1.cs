using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class TheBridgeEpisode1
{
    static void Main(string[] args)
    {
        var spaceBeforeGap = int.Parse(Console.ReadLine()!); // the length of the road before the gap.
        var gapSize = int.Parse(Console.ReadLine()!); // the length of the gap.
        var landingSpace = int.Parse(Console.ReadLine()!); // the length of the landing platform.

        // game loop
        while (true)
        {
            int currentSpeed = int.Parse(Console.ReadLine()!); // the motorbike's speed.
            int currentPossition = int.Parse(Console.ReadLine()!); // the position on the road of the motorbike.

            Console.Error.WriteLine("Current Position {0}", currentPossition);
            Console.Error.WriteLine("gapSize {0}", gapSize);
            Console.Error.WriteLine("spaceBeforeGap {0}", spaceBeforeGap);
            Console.Error.WriteLine("SpaceUntilStop {0}", SpaceUntilStop(currentSpeed));

            string decision;

            // A single line containing one of 4 keywords: SPEED, SLOW, JUMP, WAIT.
            if (CanIncreaseSpped(currentSpeed, currentPossition, spaceBeforeGap, gapSize))
                decision = "SPEED";
            else if(currentPossition == (spaceBeforeGap - 1))
                decision = "JUMP";
            else if (ShouldStop(currentPossition, spaceBeforeGap, landingSpace, currentSpeed, gapSize))
                decision = "SLOW";
            else
                decision = "WAIT";

            Console.WriteLine(decision);
        }
    }

    private static bool ShouldStop(int currentPossition, int spaceBeforeGap, int landingSpace, int currentSpeed, int gapSize)
    {
        if (currentPossition < spaceBeforeGap)
        {
            if (currentSpeed > gapSize + 1)
                return true;
            else
                return false;
        }

        if (landingSpace - (currentPossition + currentSpeed) <= SpaceUntilStop(currentSpeed))
            return true;
        
        return false;
    }

    private static bool CanIncreaseSpped(int currentSpeed, int currentPossition, int spaceBeforeGap, int gapSize)
    {
        if (currentPossition > spaceBeforeGap)
            return false;

        if (currentSpeed >= gapSize + 1)
            return false;

        if (currentSpeed < gapSize)
            return true;

        return ((spaceBeforeGap - 1) - currentPossition) % (currentSpeed + 1) == 0;
    }

    private static int SpaceUntilStop(int currentSpeed)
    {
        var total = 0;
        while (currentSpeed > 0)
        {
            total += currentSpeed;
            currentSpeed--;
        }
        return total;
    }
}