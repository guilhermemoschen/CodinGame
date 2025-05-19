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
class HeatDetector
{
    static Rectangle buildingArea;

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine()!.Split(' ');
        int W = int.Parse(inputs[0]); // width of the building.
        int H = int.Parse(inputs[1]); // height of the building.
        
        buildingArea = new Rectangle(0, 0, W, H);
        Console.Error.WriteLine("buildingArea {0}", buildingArea);
        var possibleBombPoistion = new Rectangle(0, 0, W, H);
        int N = int.Parse(Console.ReadLine()!); // maximum number of turns before game over.
        inputs = Console.ReadLine()!.Split(' ');
        var batmanPosition = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));

        // game loop
        while (true)
        {
            var bombDirection = Console.ReadLine()!; // the direction of the bombs from batman's current location (U, UR, R, DR, D, DL, L or UL)

            possibleBombPoistion = GetNewPossibleBombPosition(bombDirection, possibleBombPoistion, batmanPosition);
            Console.Error.WriteLine("possibleBombPoistion {0}", possibleBombPoistion);

            var jumpSize = CaculateJumpSize(bombDirection, possibleBombPoistion);
            Console.Error.WriteLine("Jump Size X {0} Y {1}", jumpSize.X, jumpSize.Y);

            batmanPosition = GetNewBatmanPosition(bombDirection, batmanPosition, jumpSize);

            Console.WriteLine("{0} {1}", batmanPosition.X, batmanPosition.Y); // the location of the next window Batman should jump to.
        }
    }

    private static Point GetNewBatmanPosition(string bombDirection, Point batmanPosition, Point jumpSize)
    {
        switch (bombDirection)
        {
            case "U":
                batmanPosition.Y -= jumpSize.Y;
                if (batmanPosition.Y < 0)
                    batmanPosition.Y = 0;
                break;
            case "UR":
                batmanPosition.X += jumpSize.X;
                if (batmanPosition.X > buildingArea.Width - 1)
                    batmanPosition.X = buildingArea.Width - 1;

                batmanPosition.Y -= jumpSize.Y;
                if (batmanPosition.Y < 0)
                    batmanPosition.Y = 0;
                break;
            case "R":
                batmanPosition.X += jumpSize.X;
                if (batmanPosition.X > buildingArea.Width - 1)
                    batmanPosition.X = buildingArea.Width - 1;
                break;
            case "DR":
                batmanPosition.X += jumpSize.X;
                if (batmanPosition.X > buildingArea.Width - 1)
                    batmanPosition.X = buildingArea.Width - 1;

                batmanPosition.Y += jumpSize.Y;
                if (batmanPosition.Y > buildingArea.Height - 1)
                    batmanPosition.Y = buildingArea.Height - 1;
                break;
            case "D":
                batmanPosition.Y += jumpSize.Y;
                if (batmanPosition.Y > buildingArea.Height - 1)
                    batmanPosition.Y = buildingArea.Height - 1;
                break;
            case "DL":
                batmanPosition.X -= jumpSize.X;
                if (batmanPosition.X < 0)
                    batmanPosition.X = 0;

                batmanPosition.Y += jumpSize.Y;
                if (batmanPosition.Y > buildingArea.Height - 1)
                    batmanPosition.Y = buildingArea.Height - 1;
                break;
            case "L":
                batmanPosition.X -= jumpSize.X;
                if (batmanPosition.X < 0)
                    batmanPosition.X = 0;
                break;
            case "UL":
                batmanPosition.X -= jumpSize.X;
                if (batmanPosition.X < 0)
                    batmanPosition.X = 0;

                batmanPosition.Y -= jumpSize.Y;
                if (batmanPosition.Y < 0)
                    batmanPosition.Y = 0;
                break;
        }

        return batmanPosition;
    }

    private static Rectangle GetNewPossibleBombPosition(string bombDirection, Rectangle possibleBombPoistion, Point batmanPosition)
    {
        switch (bombDirection)
        {
            case "U":
                possibleBombPoistion.X = batmanPosition.X;
                possibleBombPoistion.Width = 1;

                possibleBombPoistion.Height = batmanPosition.Y - 1 - possibleBombPoistion.Y;
                break;

            case "UR":
                possibleBombPoistion.Width -= batmanPosition.X + 1 - possibleBombPoistion.X;
                possibleBombPoistion.X = batmanPosition.X + 1;

                possibleBombPoistion.Height = batmanPosition.Y - 1 - possibleBombPoistion.Y;
                break;

            case "R":
                possibleBombPoistion.Y = batmanPosition.Y;
                possibleBombPoistion.Height = 1;

                possibleBombPoistion.Width -= (batmanPosition.X + 1 - possibleBombPoistion.X);
                possibleBombPoistion.X = batmanPosition.X + 1;
                break;

            case "DR":
                possibleBombPoistion.Height -= (batmanPosition.Y + 1 - possibleBombPoistion.Y);
                possibleBombPoistion.Y = batmanPosition.Y + 1;

                possibleBombPoistion.Width -= (batmanPosition.X + 1 - possibleBombPoistion.X);
                possibleBombPoistion.X = batmanPosition.X + 1;
                break;

            case "D":
                possibleBombPoistion.X = batmanPosition.X;
                possibleBombPoistion.Width = 1;

                possibleBombPoistion.Height -= (batmanPosition.Y + 1 - possibleBombPoistion.Y);
                possibleBombPoistion.Y = batmanPosition.Y + 1;
                break;

            case "DL":
                possibleBombPoistion.Height -= (batmanPosition.Y + 1 - possibleBombPoistion.Y);
                possibleBombPoistion.Y = batmanPosition.Y + 1;

                possibleBombPoistion.Width = batmanPosition.X - 1 - possibleBombPoistion.X;
                break;

            case "L":
                possibleBombPoistion.Y = batmanPosition.Y;
                possibleBombPoistion.Height = 1;

                possibleBombPoistion.Width = batmanPosition.X - 1 - possibleBombPoistion.X;
                break;

            case "UL":
                possibleBombPoistion.Height = batmanPosition.Y - 1 - possibleBombPoistion.Y;

                possibleBombPoistion.Width = batmanPosition.X - 1 - possibleBombPoistion.X;
                break;
        }

        if (possibleBombPoistion.Height == 0)
            possibleBombPoistion.Height = 1;

        if (possibleBombPoistion.Width == 0)
            possibleBombPoistion.Width = 1;

        return possibleBombPoistion;
    }

    private static Point CaculateJumpSize(string bombDirection, Rectangle possibleBombPoistion)
    {
        var offsetX = 0;
        var offsetY = 0;

        switch (bombDirection)
        {
            case "U":
                offsetX = 0;
                offsetY = possibleBombPoistion.Height + 1;
                break;
            case "UR":
                offsetX = possibleBombPoistion.Width + 1;
                offsetY = possibleBombPoistion.Height + 1;
                break;
            case "R":
                offsetX = possibleBombPoistion.Width + 1;
                offsetY = 0;
                break;
            case "DR":
                offsetX = possibleBombPoistion.Width + 1;
                offsetY = possibleBombPoistion.Height + 1;
                break;
            case "D":
                offsetX = 0;
                offsetY = possibleBombPoistion.Height + 1;
                break;
            case "DL":
                offsetX = possibleBombPoistion.Width + 1;
                offsetY = possibleBombPoistion.Height + 1;
                break;
            case "L":
                offsetX = possibleBombPoistion.Width + 1;
                offsetY = 0;
                break;
            case "UL":
                offsetX = possibleBombPoistion.Width + 1;
                offsetY = possibleBombPoistion.Height + 1;
                break;
        }

        if (offsetX % 2 == 0)
            offsetX /= 2;
        else
            offsetX = (offsetX / 2) + 1;

        if (offsetY % 2 == 0)
            offsetY /= 2;
        else
            offsetY = (offsetY / 2) + 1;
        
        return new Point(offsetX, offsetY);
    }
}
