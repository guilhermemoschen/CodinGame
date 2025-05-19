using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CodinGame.Solo.Puzzles.Easy;

public static class SixDegreesOfKevinBacon
{
    public static void Main()
    {
        var allCasts = new List<string>();
        var actorName = Console.ReadLine()!;
        var totalCasts = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
        for (var i = 0; i < totalCasts; i++)
        {
            allCasts.Add(Console.ReadLine()!);
        }

        var allNodes = new List<Node>();
        foreach (var cast in allCasts)
        {
            var movieCast = cast.Split([": ", ", "], StringSplitOptions.RemoveEmptyEntries)[1..];
            foreach (var castActorName in movieCast)
            {
                var actorNode = GetOrAddNode(castActorName, allNodes);
                LinkMovieCast(actorNode, movieCast, allNodes);
            }
        }

        const string target = "Kevin Bacon";
        var root = GetOrAddNode(actorName, allNodes);
        var queue = new Queue<Node>();
        queue.Enqueue(root);

        var targetDepth = 0;

        while (queue.Count != 0)
        {
            var currentNode = queue.Dequeue();

            if (currentNode.Visited)
            {
                continue;
            }

            if (currentNode.ActorName == target)
            {
                targetDepth = currentNode.Depth;
                break;
            }

            currentNode.Visit();

            foreach (var nextNode in currentNode.Links.Where(linkedNode =>
                         !linkedNode.Visited &&
                         queue.All(enqueuedNode => enqueuedNode.ActorName != linkedNode.ActorName)))
            {
                nextNode.Depth = currentNode.Depth + 1;
                queue.Enqueue(nextNode);
            }
        }

        Console.WriteLine($"{targetDepth}");
    }

    private static Node GetOrAddNode(string actorName, List<Node> allNodes)
    {
        var actorNode = allNodes.Find(n => n.ActorName == actorName);
        if (actorNode is not null)
        {
            return actorNode;
        }

        actorNode = new Node(actorName);
        allNodes.Add(actorNode);

        return actorNode;
    }

    private static void LinkMovieCast(Node actorNode, IEnumerable<string> movieCast, List<Node> allNodes)
    {
        foreach (var otherActorName in movieCast.Where(cast => cast != actorNode.ActorName))
        {
            var otherActorNode = GetOrAddNode(otherActorName, allNodes);
            actorNode.AddLink(otherActorNode);
        }
    }

    private class Node(string actorName)
    {
        public string ActorName { get; } = actorName;

        public List<Node> Links { get; } = [];

        public int Depth { get; set; }

        public bool Visited { get; private set; }

        public void AddLink(Node otherActor)
        {
            if (Links.Exists(node => node.ActorName == otherActor.ActorName))
            {
                return;
            }

            Links.Add(otherActor);
        }

        public void Visit()
        {
            Visited = true;
        }

        public override string ToString()
        {
            return $"{ActorName}:{Depth}:{Visited}";
        }
    }
}