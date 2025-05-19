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
public static class Defibrillators
{
    static void Main(string[] args)
    {
        var userLongitude = Convert.ToDouble(Console.ReadLine()!.Replace(",", "."));
        var userLatitude = Convert.ToDouble(Console.ReadLine()!.Replace(",", "."));
        var userChoice = string.Empty;
        var lowestDistance = -1.0;

        int N = int.Parse(Console.ReadLine()!);
        for (int i = 0; i < N; i++)
        {
            var info = Console.ReadLine()!.Split(';');
            var longitude = Convert.ToDouble(info[4].Replace(",", "."));
            var latitude = Convert.ToDouble(info[5].Replace(",", "."));
            var distance = CalculateDistance3(userLatitude, userLongitude, latitude, longitude);
            if (lowestDistance < 0 || distance < lowestDistance)
            {
                userChoice = info[1];
                lowestDistance = distance;
            }
        }

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        Console.WriteLine(userChoice);
    }

    public static double CalculateDistance(double latitudeA, double longitudeA, double latitudeB, double longitudeB)
    {
        var x = (longitudeB - longitudeA) * Math.Cos((latitudeA + latitudeB) / 2);
        var y = latitudeB - latitudeA;
        return Math.Sqrt(x * x + y * y) * 6371;
    }

    public static double CalculateDistance2(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Radius of the earth in km
        var dLat = deg2rad(lat2 - lat1);  // deg2rad below
        var dLon = deg2rad(lon2 - lon1);
        var a =
          Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
          Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
          Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
          ;
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c; // Distance in km
        return d;
    }

    public static double deg2rad(double deg)
    {
        return deg * (Math.PI / 180);
    }

    public static double CalculateDistance3(double latitudeA, double longitudeA, double latitudeB, double longitudeB)
    {
        var x = longitudeA - longitudeB;
        var y = latitudeA - latitudeB;
        return Math.Sqrt(x * x + y * y);
    }
}