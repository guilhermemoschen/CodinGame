namespace CodinGame.Solo.Puzzles.Easy;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public static class MovesInMaze
{
    public static void Main(string[] args)
    {
        string[] inputs;

        if (args.Length == 0)
        {
            var lines = new List<string>();
            lines.Add(Console.ReadLine()!);
            var h = int.Parse(lines.First().Split(' ')[1], CultureInfo.InvariantCulture);

            for (int i = 0; i < h; i++)
            {
                lines.Add(Console.ReadLine()!);
            }
            inputs = lines.ToArray();
        }
        else
        {
            inputs = args;

        }

        var board = CreateBoard(inputs);
        var result = Bfs(board);

        for (int i = 0; i < result.Length; i++)
        {
            Console.WriteLine(new string(result[i]));
        }
    }

    private static char[][] CreateBoard(string[] inputs)
    {
        foreach (var line in inputs)
        {
            Console.Error.WriteLine(line);
        }

        var firstLine = inputs[0].Split(' ');
        var h = int.Parse(firstLine[1], CultureInfo.InvariantCulture);
        var board = new char[h][];

        for (int i = 0; i < h; i++)
        {
            board[i] = inputs[i + 1].ToCharArray();
        }

        return board;
    }

    private static char[][] Bfs(char[][] board)
    {
        var root = FindStartPoint(board);

        var queue = new Queue<Cell>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();

            if (board[next.Y][next.X] != '#' && board[next.Y][next.X] != '.' && board[next.Y][next.X] <= next.Points)
            {
                continue;
            }

            board[next.Y][next.X] = next.Points;
            foreach (var neighbor in FindNeighbors(next, board))
            {
                queue.Enqueue(neighbor);
            }
        }

        return board;
    }

    private static Cell FindStartPoint(char[][] board)
    {
        for (var i = 0; i < board.Length; i++)
        {
            for (var j = 0; j < board[i].Length; j++)
            {
                if (board[i][j] == 'S')
                {
                    return new Cell() { X = j, Y = i, Points = '0' };
                }
            }
        }

        throw new InvalidOperationException();
    }

    private static List<Cell> FindNeighbors(Cell cell, char[][] board)
    {
        return new[]
        {
            TraverseUp(cell, board),
            TraverseDown(cell, board),
            TraverseLeft(cell, board),
            TraverseRight(cell, board),
        }
        .Where(c => c is not null)
        .ToList()!;
    }

    private static Cell? TraverseUp(Cell cell, char[][] board)
    {
        var nextY = cell.Y == 0 ? board.Length - 1 : cell.Y - 1;
        if (board[nextY][cell.X] == '#')
        {
            return null;
        }

        var nextPoints = cell.Points == 57 ? (char)65 : (char)(cell.Points + 1);
        if (board[nextY][cell.X] > nextPoints)
        {
            board[nextY][cell.X] = nextPoints;
            return null;
        }

        if (board[nextY][cell.X] != '.')
        {
            return null;
        }

        return new Cell()
        {
            X = cell.X,
            Y = nextY,
            Points = nextPoints,
        };
    }

    private static Cell? TraverseDown(Cell cell, char[][] board)
    {
        var nextY = cell.Y == board.Length - 1 ? 0 : cell.Y + 1;
        if (board[nextY][cell.X] == '#')
        {
            return null;
        }

        var nextPoints = cell.Points == 57 ? (char)65 : (char)(cell.Points + 1);
        if (board[nextY][cell.X] > nextPoints)
        {
            board[nextY][cell.X] = nextPoints;
            return null;
        }

        if (board[nextY][cell.X] != '.')
        {
            return null;
        }

        return new Cell()
        {
            X = cell.X,
            Y = nextY,
            Points = nextPoints,
        };
    }

    private static Cell? TraverseLeft(Cell cell, char[][] board)
    {
        var nextX = cell.X == 0 ? board[0].Length - 1 : cell.X - 1;
        if (board[cell.Y][nextX] == '#')
        {
            return null;
        }

        var nextPoints = cell.Points == 57 ? (char)65 : (char)(cell.Points + 1);
        if (board[cell.Y][nextX] > nextPoints)
        {
            board[cell.Y][nextX] = nextPoints;
            return null;
        }

        if (board[cell.Y][nextX] != '.')
        {
            return null;
        }

        return new Cell()
        {
            X = nextX,
            Y = cell.Y,
            Points = nextPoints,
        };
    }

    private static Cell? TraverseRight(Cell cell, char[][] board)
    {
        var nextX = cell.X == board[0].Length - 1 ? 0 : cell.X + 1;
        if (board[cell.Y][nextX] == '#')
        {
            return null;
        }

        var nextPoints = cell.Points == 57 ? (char)65 : (char)(cell.Points + 1);
        if (board[cell.Y][nextX] > nextPoints)
        {
            board[cell.Y][nextX] = nextPoints;
            return null;
        }
        
        if (board[cell.Y][nextX] != '.')
        {
            return null;
        }

        return new Cell()
        {
            X = nextX,
            Y = cell.Y,
            Points = nextPoints,
        };
    }

    private class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Points { get; set; }
    }
}

public static class MovesInMazeTestCases
{
    public static readonly string[] Easy = new string[]
    {
        "10 5",
        "##########",
        "#S.......#",
        "##.#####.#",
        "##.#.....#",
        "##########",
    };

    public static readonly string[] ThroughBorders = new string[]
    {
        "10 5",
        "#.########",
        "#.##..####",
        "..##..#...",
        "####..#S##",
        "#....#####",
    };

    public static readonly string[] Space = new string[]
    {
        "15 10",
        "...............",
        "......#........",
        "...............",
        "...............",
        "...............",
        "............#..",
        "............#..",
        "...............",
        "S..............",
        ".#.............",
    };
}