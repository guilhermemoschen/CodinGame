using System;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Don't let the machines win. You are humanity's last hope...
 **/

namespace ThereIsNoSpoonQ1
{
    public class Node
    {
        public Point Position { get; set; }
        public bool Visited { get; set; }
        public bool IsSpace { get; set; }
    }

    public class Q1
    {
        public static Node[][] Board { get; set; }


        static void Main(string[] args)
        {
            int width = int.Parse(Console.ReadLine()); // the number of cells on the X axis
            int height = int.Parse(Console.ReadLine()); // the number of cells on the Y axis

            Console.Error.WriteLine("width {0} height {1}", width, height);
            
            Board = new Node[height][];

            for (var i = 0; i < height; i++)
            {
                var line = Console.ReadLine(); // width characters, each either 0 or .

                Board[i] = new Node[width];

                for (var j = 0; j < width; j++)
                {
                    var node = new Node();
                    node.Visited = false;
                    node.Position = new Point(j, i);
                    node.IsSpace = line[j] == '.';

                    Board[i][j] = node;
                }

                Console.Error.WriteLine("line {0}", line);
            }

            var targetNode = GetNextNode();
            while ( targetNode != null)
            {
                targetNode.Visited = true;
                Console.WriteLine(GenerateOutput(targetNode));
                targetNode = GetNextNode();
            }
        }

        private static string GenerateOutput(Node targetNode)
        {
            var output = string.Empty;
            output += string.Format("{0} {1}", targetNode.Position.X, targetNode.Position.Y);


            Console.Error.WriteLine("targetNode {0}", targetNode.Position);
            Console.Error.WriteLine("Board[targetNode.Position.Y].Length {0}", Board[targetNode.Position.Y].Length);

            Node rightNode = null;

            for (var i = targetNode.Position.X + 1; i < Board[targetNode.Position.Y].Length; i++)
            {
                var currentNode = Board[targetNode.Position.Y][i];
                if (currentNode.Visited)
                    break;

                if (currentNode.IsSpace)
                    continue;

                rightNode = currentNode;
                break;

            }

            if (rightNode != null)
            {
                output += string.Format(" {0} {1}", rightNode.Position.X, rightNode.Position.Y);
            }
            else
            {
                output += " -1 -1";
            }

            Node bottonNode = null;

            for (var i = targetNode.Position.Y + 1; i < Board.Length; i++)
            {
                var currentNode = Board[i][targetNode.Position.X];
                if (currentNode.Visited)
                    break;

                if (currentNode.IsSpace)
                    continue;

                bottonNode = currentNode;
                break;
            }

            if (bottonNode != null)
            {
                output += string.Format(" {0} {1}", bottonNode.Position.X, bottonNode.Position.Y);
            }
            else
            {
                output += " -1 -1";
            }

            return output;
        }

        private static Node GetNextNode()
        {
            return Board.SelectMany(x => x).FirstOrDefault(x => !x.Visited && !x.IsSpace);
        }
    }
}