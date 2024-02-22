namespace CodinGame.Solo.Puzzles.Hard;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class ThereIsNoSpoonEpisode2
{
    public static bool LogEnabled { get; set; } = true;

    public static void Main(string[] args)
    {
        int boardWidth;
        int boardHeight;
        char[][] boardRaw;

        if (args.Length == 0)
        {
            boardWidth = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            boardWidth += boardWidth - 1;
            boardHeight = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            boardHeight += boardHeight - 1;
            boardRaw = new char[boardHeight][];

            for (var i = 0; i < boardHeight; i++)
            {
                var row = new string('.', boardWidth).ToCharArray();

                if (i % 2 == 0)
                {
                    var compactRow = Console.ReadLine()!.ToCharArray();
                    for (var j = 0; j < compactRow.Length; j++)
                    {
                        row[j * 2] = compactRow[j];
                    }
                }

                boardRaw[i] = row;
            }
        }
        else
        {
            boardWidth = int.Parse(args[0], CultureInfo.InvariantCulture);
            boardWidth += boardWidth - 1;
            boardHeight = int.Parse(args[1], CultureInfo.InvariantCulture);
            boardHeight += boardHeight - 1;
            boardRaw = new char[boardHeight][];

            for (var i = 0; i < boardHeight; i++)
            {
                var row = new string('.', boardWidth).ToCharArray();

                if (i % 2 == 0)
                {
                    var compactRow = args[2 + (i / 2)].ToCharArray();

                    for (var j = 0; j < compactRow.Length; j++)
                    {
                        row[j * 2] = compactRow[j];
                    }
                }

                boardRaw[i] = row;
            }
        }

        var board = Board.Create(boardWidth, boardHeight, boardRaw);
        board.PrintBoard();

        LogEnabled = false;
        var solver = new Solver()
        {
            Board = board,
        };

        solver.SolveByLogic();
        LogEnabled = true;
        board.PrintBoard();
        LogEnabled = false;
        solver.SolveByBacktrack();
        LogEnabled = true;
        board.PrintBoard();

        foreach (var link in board.GetLinksToCondingGame())
        {
            Console.WriteLine(link);
        }
    }

    private static void Log(string message)
    {
        if (LogEnabled)
        {
            Console.Error.WriteLine(message);
        }
    }

    private class Solver
    {
        public Board Board { get; init; } = null!;

        public void SolveByLogic()
        {
            var currentRawBoard = Array.Empty<char[]>();
            var nextRawBoard = Board.GetRaw();

            while (Board.IsRawDifferent(currentRawBoard, nextRawBoard))
            {
                currentRawBoard = nextRawBoard;

                foreach (var incompleteNode in Board.Nodes.Where(n => !n.Done))
                {
                    if (incompleteNode.Done)
                    {
                        continue;
                    }

                    var candidates = Board.DiscoverNeighbors(incompleteNode).ToList();
                    if (candidates.Count != 0)
                    {
                        Board.ProcessCandidates(incompleteNode, candidates);
                    }
                }

                nextRawBoard = Board.GetRaw();
            }
        }

        public void SolveByBacktrack(Link? candidate = null)
        {
            if (candidate is not null)
            {
                Board.AddLink(candidate);
                SolveByLogic();

                if (Board.Invalid())
                {
                    Board.Rollback(candidate);
                    return;
                }

                if (Board.Finished)
                {
                    return;
                }
            }

            var usedCandidates = new List<Link>();
            var next = GetNextCandidate(candidate, usedCandidates);
            while (next is not null)
            {
                SolveByBacktrack(next);

                usedCandidates.Add(next);
                next = GetNextCandidate(next, usedCandidates);
            }

            if (!Board.Finished)
            {
                Log($"Candidate {candidate} failed, rolling back");
                Board.Rollback(candidate!);
            }
        }

        private Link? GetNextCandidate(Link? currentCandidate, List<Link> rejected)
        {
            if (Board.Finished)
            {
                return null;
            }

            if (currentCandidate?.From.Done == false)
            {
                var neighbor = Board
                    .DiscoverNeighbors(currentCandidate.From)
                    .Find(n => !rejected.Exists(r => r.From == currentCandidate.From && r.To == n));
                if (neighbor is not null)
                {
                    return new Link() { From = currentCandidate.From, To = neighbor };
                }
            }

            if (currentCandidate?.To.Done == false)
            {
                var neighbor = Board
                    .DiscoverNeighbors(currentCandidate.To)
                    .Find(n => !rejected.Exists(r => r.From == currentCandidate.To && r.To == n));
                if (neighbor is not null)
                {
                    return new Link() { From = currentCandidate.To, To = neighbor };
                }
            }

            foreach (var nextNode in Board.Nodes
                .Where(n => !n.Done)
                .OrderBy(n => n.MissingLinks)
                .ThenBy(n => n.RequiredLinks))
            {
                var neighbor = Board
                    .DiscoverNeighbors(nextNode)
                    .Find(n => !rejected.Exists(r => r.From == nextNode && r.To == n));

                if (neighbor is not null)
                {
                    return new Link() { From = nextNode, To = neighbor };
                }
            }

            return null;
        }
    }

    private class Board
    {
        private char[][] _raw = null!;

        private Board()
        {
        }

        public int Width { get; init; }

        public int Height { get; init; }

        public List<Node> Nodes { get; private set; } = new List<Node>();

        public Stack<Link> Links { get; private set; } = new Stack<Link>();

        public bool Finished => Nodes.TrueForAll(n => n.Done) && Valid();

        public char[][] GetRaw()
        {
            var raw = new char[_raw.Length][];
            for (var i = 0; i < _raw.Length; i++)
            {
                raw[i] = (char[])_raw[i].Clone();
            }

            return raw;
        }

        public static Board Create(int width, int height, char[][] raw)
        {
            var board = new Board()
            {
                Width = width,
                Height = height,
                _raw = (char[][])raw.Clone(),
            };

            var nodeId = 0;

            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    if (raw[row][column] == '.')
                    {
                        continue;
                    }

                    var node = new Node()
                    {
                        Id = nodeId++,
                        Column = column,
                        Row = row,
                        RequiredLinks = int.Parse(raw[row][column].ToString(), CultureInfo.InvariantCulture),
                    };

                    board.Nodes.Add(node);
                }
            }

            return board;
        }

        public List<Node> DiscoverNeighbors(Node node)
        {
            var possibleNeighbors = new[]
                {
                    TraverseUp(node),
                    TraverseDown(node),
                    TraverseLeft(node),
                    TraverseRight(node),
                }
                .Where(n => n is not null && !n.Done && !WillInvalidate(node, n))
                .ToList();

            return possibleNeighbors!;
        }

        private Node? TraverseUp(Node node)
        {
            for (var row = node.Row - 1; row >= 0; row--)
            {
                if (_raw[row][node.Column] == '$')
                {
                    return null;
                }

                if (_raw[row][node.Column] != '.' && _raw[row][node.Column] != '|')
                {
                    var candidate = GetNode(row, node.Column);
                    if (candidate is not null && _raw[row + 1][node.Column] == '|' && candidate.MissingLinks == 0)
                    {
                        return null;
                    }

                    return candidate;
                }
            }

            return null;
        }

        private Node? TraverseDown(Node node)
        {
            for (var row = node.Row + 1; row < Height; row++)
            {
                if (_raw[row][node.Column] == '$')
                {
                    return null;
                }

                if (_raw[row][node.Column] != '.' && _raw[row][node.Column] != '|')
                {
                    var candidate = GetNode(row, node.Column);
                    if (candidate is not null && _raw[row - 1][node.Column] == '|' && candidate.MissingLinks == 0)
                    {
                        return null;
                    }

                    return candidate;
                }
            }

            return null;
        }

        private Node? TraverseLeft(Node node)
        {
            for (var column = node.Column - 1; column >= 0; column--)
            {
                if (_raw[node.Row][column] == '=')
                {
                    return null;
                }

                if (_raw[node.Row][column] != '.' && _raw[node.Row][column] != '-')
                {
                    var candidate = GetNode(node.Row, column);
                    if (candidate is not null && _raw[node.Row][column + 1] == '-' && candidate.MissingLinks == 0)
                    {
                        return null;
                    }

                    return candidate;
                }
            }

            return null;
        }

        private Node? TraverseRight(Node node)
        {
            for (var column = node.Column + 1; column < Width; column++)
            {
                if (_raw[node.Row][column] == '=')
                {
                    return null;
                }

                if (_raw[node.Row][column] != '.' && _raw[node.Row][column] != '-')
                {
                    var candidate = GetNode(node.Row, column);
                    if (candidate is not null && _raw[node.Row][column - 1] == '-' && candidate.MissingLinks == 0)
                    {
                        return null;
                    }

                    return candidate;
                }
            }

            return null;
        }

        private bool WillInvalidate(Node from, Node to)
        {
            var failed = false;
            AddLink(from, to);
            if (!Finished && HasAnyClosedConnections())
            {
                failed = true;
            }

            RollbackLastLink();

            return failed;
        }

        private Node? GetNode(int row, int column) => Nodes.Find(node => node.Column == column && node.Row == row);

        public void AddLink(Node candidate, Node nextCandidate)
        {
            var link = new Link() { From = candidate, To = nextCandidate };
            AddLink(link);
        }

        public void AddLink(Link link)
        {
            Links.Push(link);
            link.From.LinkedNodes.Add(link.To);
            link.To.LinkedNodes.Add(link.From);

            AddLinkToRaw(link);

            PrintBoard();
        }

        private void AddLinkToRaw(Link link)
        {
            if (link.From.Column < link.To.Column)
            {
                for (var column = link.From.Column + 1; column < link.To.Column; column++)
                {
                    _raw[link.From.Row][column] = _raw[link.From.Row][column] == '.'
                        ? '-'
                        : '=';
                }

                return;
            }

            if (link.From.Column > link.To.Column)
            {
                for (var column = link.From.Column - 1; column > link.To.Column; column--)
                {
                    _raw[link.From.Row][column] = _raw[link.From.Row][column] == '.'
                        ? '-'
                        : '=';
                }

                return;
            }

            if (link.From.Row < link.To.Row)
            {
                for (var row = link.From.Row + 1; row < link.To.Row; row++)
                {
                    _raw[row][link.From.Column] = _raw[row][link.From.Column] == '.'
                        ? '|'
                        : '$';
                }

                return;
            }

            if (link.From.Row > link.To.Row)
            {
                for (var row = link.From.Row - 1; row > link.To.Row; row--)
                {
                    _raw[row][link.From.Column] = _raw[row][link.From.Column] == '.'
                        ? '|'
                        : '$';
                }
            }
        }

        public void PrintBoard()
        {
            Log($"   - {string.Join(string.Empty, Enumerable.Range(0, _raw[0].Length))}");

            for (int y = 0; y < Height; y++)
            {
                Log($"{y:00} - {new string(_raw[y])}");
            }
        }

        public void ProcessCandidates(Node node, List<Node> candidates)
        {
            switch (node.MissingLinks)
            {
                case 1:
                    if (candidates.Count == 1)
                    {
                        AddLink(node, candidates[0]);
                    }

                    break
;
                case 2:
                    if (candidates.Count == 1)
                    {
                        AddLink(node, candidates[0]);
                        AddLink(node, candidates[0]);
                        break;
                    }

                    // Can only process 2 neighbors
                    if (candidates.Count != 2)
                    {
                        break;
                    }

                    if (candidates.TrueForAll(c => c.MissingLinks == 1))
                    {
                        AddLink(node, candidates[0]);
                        AddLink(node, candidates[1]);
                    }

                    // When on neighbor cannot receive 2 links, at least one must connected to the other neighbor
                    if (candidates.Exists(c => c.MissingLinks == 1) && candidates.Exists(c => c.MissingLinks > 1))
                    {
                        AddLink(node, candidates.Find(x => x.MissingLinks > 1)!);
                        break;
                    }

                    // When adding to links to a node will result in an closed island
                    if (candidates.Count(c => c.RequiredLinks == 2 && c.MissingLinks == 2) == 2 && Nodes.Count > 2)
                    {
                        AddLink(node, candidates[0]);
                        AddLink(node, candidates[1]);
                    }

                    break;

                default:
                    // MissingLinks == AvailableLinks
                    if (node.MissingLinks == candidates.Sum(x => x.MissingLinks > 1 ? 2 : 1))
                    {
                        foreach (var neighbor in candidates)
                        {
                            AddLink(node, neighbor);

                            var currentConnections = node.LinkedNodes.Count(n => n == neighbor);

                            if (currentConnections == 1 && neighbor.MissingLinks > 0)
                            {
                                AddLink(node, neighbor);
                            }
                        }

                        break;
                    }

                    // a 3 with two neighbors, a 5 with 3 neighbors and a 7 with four neighbors connect with one bridge
                    // a 4 with two neighbors, a 6 with 3 neighbors and a 8 with four neighbors connect with two bridges
                    if (Math.Ceiling(node.MissingLinks / 2M) == candidates.Count)
                    {
                        var add2Links = node.MissingLinks % 2 == 0;

                        foreach (var neighbor in candidates)
                        {
                            AddLink(node, neighbor);

                            if (add2Links)
                            {
                                AddLink(node, neighbor);
                            }
                        }
                    }

                    break;
            }
        }

        public List<string> GetLinksToCondingGame()
        {
            return Links
                .Reverse()
                .Select(link =>
                {
                    return $"{link.From.Column / 2} {link.From.Row / 2} {link.To.Column / 2} {link.To.Row / 2} 1";
                })
                .ToList();
        }

        private bool Valid()
        {
            var visited = new bool[Nodes.Count];
            var connectedNodes = CountConnections(Nodes[0], visited);
            return Nodes.Count == connectedNodes;
        }

        private static int CountConnections(Node node, bool[] visited)
        {
            if (visited[node.Id])
            {
                return 0;
            }

            visited[node.Id] = true;

            var localCount = 1;

            foreach (var linked in node.LinkedNodes)
            {
                localCount += CountConnections(linked, visited);
            }

            return localCount;
        }

        public bool Invalid()
        {
            if (Finished)
            {
                return false;
            }

            if (HasAnyClosedConnections())
            {
                return true;
            }

            return Nodes
                .Where(n => !n.Done)
                .Any(n => DiscoverNeighbors(n).Count == 0);
        }

        private bool HasAnyClosedConnections()
        {
            var visited = new bool[Nodes.Count];
            return Nodes
                .Where(n => n.Done && !visited[n.Id])
                .Any(n => IsFinishedIsland(n, visited));
        }

        public bool IsFinishedIsland(Node node, bool[] visited)
        {
            if (visited[node.Id])
            {
                return node.Done;
            }

            visited[node.Id] = true;

            var finished = node.Done;
            foreach (var linkedNode in node.LinkedNodes.Select(n => n.Id).Where(id => !visited[id]).Distinct())
            {
                finished = IsFinishedIsland(Nodes[linkedNode], visited) && finished;
            }

            return finished;
        }

        public void Rollback(Link lastLink)
        {
            if (!Links.Any(l => l.Equals(lastLink)))
            {
                return;
            }

            Link currentLink;
            do
            {
                currentLink = Links.Pop();
                currentLink.From.LinkedNodes.Remove(currentLink.To);
                currentLink.To.LinkedNodes.Remove(currentLink.From);
                RemoveLinkFromRaw(currentLink);
            }
            while (!lastLink.Equals(currentLink));

            Log($"Rollback {lastLink}");
            PrintBoard();
        }

        private void RemoveLinkFromRaw(Link link)
        {
            if (link.From.Column < link.To.Column)
            {
                for (var column = link.From.Column + 1; column < link.To.Column; column++)
                {
                    _raw[link.From.Row][column] = _raw[link.From.Row][column] == '='
                        ? '-'
                        : '.';
                }

                return;
            }

            if (link.From.Column > link.To.Column)
            {
                for (var column = link.From.Column - 1; column > link.To.Column; column--)
                {
                    _raw[link.From.Row][column] = _raw[link.From.Row][column] == '='
                        ? '-'
                        : '.';
                }

                return;
            }

            if (link.From.Row < link.To.Row)
            {
                for (var row = link.From.Row + 1; row < link.To.Row; row++)
                {
                    _raw[row][link.From.Column] = _raw[row][link.From.Column] == '$'
                        ? '|'
                        : '.';
                }

                return;
            }

            if (link.From.Row > link.To.Row)
            {
                for (var row = link.From.Row - 1; row > link.To.Row; row--)
                {
                    _raw[row][link.From.Column] = _raw[row][link.From.Column] == '$'
                        ? '|'
                        : '.';
                }
            }
        }

        public void RollbackLastLink()
        {
            Rollback(Links.First());
        }

        public static bool IsRawDifferent(char[][] rawA, char[][] rawB)
        {
            if (rawA.Length != rawB.Length)
            {
                return true;
            }

            var currentHash = rawA.Select(row => new string(row)).Aggregate((accumulator, row) => $"{accumulator}{row}");
            var nextHash = rawB.Select(row => new string(row)).Aggregate((accumulator, row) => $"{accumulator}{row}");
            return currentHash != nextHash;
        }
    }

    private class Node
    {
        public int Id { get; init; }

        public int Column { get; init; }

        public int Row { get; init; }

        public int RequiredLinks { get; init; }

        public int MissingLinks => RequiredLinks - LinkedNodes.Count;

        public List<Node> LinkedNodes { get; } = new List<Node>();

        public bool Done => RequiredLinks == LinkedNodes.Count;

        public override string ToString()
        {
            return $"({Row},{Column}) - {LinkedNodes.Count}/{RequiredLinks}";
        }
    }

    private class Link : IEquatable<Link>
    {
        public Node From { get; init; } = null!;

        public Node To { get; init; } = null!;

        public override string ToString()
        {
            return $"{From} -> {To}";
        }

        public bool Equals(Link? link)
        {
            if (link == null)
            {
                return false;
            }

            return (From.Id == link.From.Id && To.Id == link.To.Id) || (To.Id == link.From.Id && From.Id == link.To.Id);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Link);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return From.Id.GetHashCode() ^ To.Id.GetHashCode();
            }
        }
    }
}

public static class TestCases
{
    public static readonly string[] SimpleArgs = new[]
    {
        "3",
        "3",
        "1.3",
        "...",
        "123",
    };

    public static readonly string[] BasicArgs = new[]
    {
        "4",
        "3",
        "14.3",
        "....",
        ".4.4",
    };

    public static readonly string[] Intermediate2Args = new[]
    {
        "7",
        "5",
        "2..2.1.",
        ".3..5.3",
        ".2.1...",
        "2...2..",
        ".1....2",
    };

    public static readonly string[] Intermediate3Args = new[]
    {
        "4",
        "4",
        "25.1",
        "47.4",
        "..1.",
        "3344",
    };

    public static readonly string[] AdvancedArgs = new[]
    {
        "8",
        "8",
        "3.4.6.2.",
        ".1......",
        "..2.5..2",
        "1.......",
        "..1.....",
        ".3..52.3",
        ".2.17..4",
        ".4..51.2",
    };

    public static readonly string[] MultipleSolutions1Args = new[]
    {
        "2",
        "2",
        "33",
        "33",
    };

    public static readonly string[] CGArgs = new[]
    {
        "5",
        "14",
        "22221",
        "2....",
        "2....",
        "2....",
        "2....",
        "22321",
        ".....",
        ".....",
        "22321",
        "2....",
        "2....",
        "2.131",
        "2..2.",
        "2222.",
    };
}