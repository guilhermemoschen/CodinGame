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
class ChuckNorris
{
    static void Main(string[] args)
    {
        var message = Console.ReadLine();

        var binarryMessage = string.Empty;

        foreach (var c in message.ToCharArray())
        {
            var b = Convert.ToString(c, 2);
            if (b.Length != 7)
            {
                b = new string('0', 7 - b.Length) + b;
            }
            
            binarryMessage += b;
        }

        var chuckNorrisMessage = string.Empty;
        for (var i = 0; i < binarryMessage.Length; )
        {
            if (chuckNorrisMessage != string.Empty)
                chuckNorrisMessage += " ";

            var lastIndex = 0;

            if (binarryMessage[i] == '1')
            {
                lastIndex = binarryMessage.IndexOf("0", i);
                chuckNorrisMessage += "0 ";
            }
            else
            {
                lastIndex = binarryMessage.IndexOf("1", i);
                chuckNorrisMessage += "00 ";
            }

            var amountOf0 = 0;
            if (lastIndex == -1)
            {
                amountOf0 = binarryMessage.Length - i;
            }
            else
            {
                amountOf0 = lastIndex - i;
            }

            chuckNorrisMessage += new string('0', amountOf0);
            i += amountOf0;
        }

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        Console.WriteLine(chuckNorrisMessage);
    }
}