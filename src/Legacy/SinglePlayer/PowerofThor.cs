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
class PowerofThor
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        var lightPoint = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
        var thorPoint = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));

        lightPoint.Y *= -1; // converting to defaul coordenation settings
        thorPoint.Y *= -1; // converting to defaul coordenation settings

        // game loop
        while (true)
        {
            int E = int.Parse(Console.ReadLine()); // The level of Thor's remaining energy, representing the number of moves he can still make.
            var angule = GetAngule(thorPoint, lightPoint);
            var direction = GetDirection(angule);
            Console.WriteLine(direction); // A single line providing the move to be made: N NE E SE S SW W or NW
            UpdateThorLocation(direction, ref thorPoint);
        }
    }

    static void UpdateThorLocation(string direction, ref Point thorPoint)
    {
        switch (direction)
        {
            case "E":
                thorPoint. X++;
                break;
            case "NE":
                thorPoint.X++;
                thorPoint.Y++;
                break;
            case "N":
                thorPoint.Y--;
                break;
            case "NW":
                thorPoint.X--;
                thorPoint.Y++;
                break;
            case "W":
                thorPoint.X--;
                break;
            case "SW":
                thorPoint.X--;
                thorPoint.Y--;
                break;
            case "S":
                thorPoint.Y--;
                break;
            case "SE":
                thorPoint.X++;
                thorPoint.Y--;
                break;
        }
    }

    static string GetDirection(double angule)
    {
        var a = Convert.ToInt32(angule);
        if (a < 0)
            a += 360;

        const int offset = 90 / 4;

        var currentDirection = 0;
        if ((a >= 0 && a <= offset) || (a <= 360 && a >= 360 - offset))
            return "E";

        currentDirection = 45;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "NE";

        currentDirection = 90;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "N";

        currentDirection = 135;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "NW";

        currentDirection = 180;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "W";

        currentDirection = 225;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "SW";

        currentDirection = 270;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "S";

        currentDirection = 315;
        if (a >= currentDirection - offset && a <= currentDirection + offset)
            return "SE";

        return null;
    }

    static double GetAngule(Point thorPoint, Point lightPoint)
    {
        return (Math.Atan2(lightPoint.Y - thorPoint.Y, lightPoint.X - thorPoint.X) * 180) / Math.PI;
    }
}