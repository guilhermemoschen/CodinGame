namespace Puzzles.Medium;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class DeathFirstSearchEpisode1
{
    public static void Main()
    {
        var inputs = Console.ReadLine()!.Split(' ');
        var totalNodes = int.Parse(inputs[0], CultureInfo.InvariantCulture);
        var graph = new List<Node>();
        for (var i = 0; i < totalNodes; i++)
        {
            graph.Add(new Node() { Index = i });
        }

        var totalLinks = int.Parse(inputs[1], CultureInfo.InvariantCulture);
        for (var i = 0; i < totalLinks; i++)
        {
            var link = Console.ReadLine()!.Split(' ');
            var nodeFrom = graph.First(n => n.Index == int.Parse(link[0], CultureInfo.InvariantCulture));
            var nodeTo = graph.First(n => n.Index == int.Parse(link[1], CultureInfo.InvariantCulture));
            nodeFrom.Neighbors.Add(nodeTo);
            nodeTo.Neighbors.Add(nodeFrom);
        }

        var totalExits = int.Parse(inputs[2], CultureInfo.InvariantCulture);
        for (var i = 0; i < totalExits; i++)
        {
            var exitIndex = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            var nodeWithExit = graph.First(n => n.Index == exitIndex);
            nodeWithExit.Exit = true;
        }

        Console.Error.WriteLine($"Initial Graph");
        graph.ForEach(n => Console.Error.WriteLine(n.ToString()));

        while (true)
        {
            var bobnetIndex = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            var bobnet = graph.First(n => n.Index == bobnetIndex);
            CalculateShortestPath(graph, bobnet);

            Console.Error.WriteLine($"Calculate CalculateShortestPath");
            graph.ForEach(n => Console.Error.WriteLine(n.ToString()));

            var closestExit = graph
                .Where(n => n.Exit)
                .OrderBy(n => n.Distance)
                .First();

            Console.Error.WriteLine($"Closest Node to exit {closestExit}");

            Node nodeToCut = closestExit;
            while (nodeToCut!.Parent != bobnet)
            {
                nodeToCut = nodeToCut.Parent!;
            }

            Console.Error.WriteLine($"Node to cut {nodeToCut}");

            Console.WriteLine($"{bobnet.Index} {nodeToCut.Index}");
        }
    }

    private static void CalculateShortestPath(List<Node> graph, Node root)
    {
        // clean
        foreach (var node in graph)
        {
            node.Parent = null;
            node.Distance = int.MaxValue;
        }

        var queue = new Queue<Node>();
        root.Distance = 0;
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            var current = queue.Dequeue();

            foreach (var node in current.Neighbors)
            {
                if (node.Distance != int.MaxValue)
                {
                    continue;
                }

                node.Distance = current.Distance + 1;
                node.Parent = current;
                if (!node.Exit)
                {
                    queue.Enqueue(node);
                }
            }
        }
    }

    public class Node
    {
        public int Index { get; init; } = -1;

        public List<Node> Neighbors { get; } = new List<Node>();

        public bool Exit { get; set; }

        public int Distance { get; set; }

        public Node? Parent { get; set; }

        public override string ToString()
        {
            return $"Node {Index} - Neighbors ({string.Join(',', Neighbors.Select(n => n.Index))}) - Exit {Exit} - Distance {Distance} - Parent {Parent?.Index}";
        }
    }
}
