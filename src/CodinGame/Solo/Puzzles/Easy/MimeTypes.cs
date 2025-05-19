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
class MimeTypes
{

    static void Main(string[] args)
    {
        var dictionary = new Dictionary<string, string>();
        var files = new List<string>();

        int N = int.Parse(Console.ReadLine()!); // Number of elements which make up the association table.
        int Q = int.Parse(Console.ReadLine()!); // Number Q of file names to be analyzed.
        for (var i = 0; i < N; i++)
        {
            string[] inputs = Console.ReadLine()!.Split(' ');
            dictionary.Add("." + inputs[0].ToUpper(), inputs[1]);
            //string EXT = ; // file extension
            //string MT = ; // MIME type.
        }
        
        for (var i = 0; i < Q; i++)
        {
            files.Add(Console.ReadLine()!.ToUpper());
            //string FNAME = ; // One file name per line.
        }

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            if (dictionary.Keys.Contains(extension))
            {
                Console.WriteLine(dictionary[extension]);
            }
            else
            {
                Console.WriteLine("UNKNOWN");
            }

        }

        //Console.WriteLine("UNKNOWN"); // For each of the Q filenames, display on a line the corresponding MIME type. If there is no corresponding type, then display UNKNOWN.
    }
}