namespace CodinGame.Solo.Puzzles.Medium;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public static class FinishTheEightQueens
{
    public static void Main(string[] args)
    {
        var raw = new char[8, 8];

        if (args.Length == 0)
        {
            for (var i = 0; i < 8; i++)
            {
                var row = Console.ReadLine()!.ToCharArray();
                for (var j = 0; j < 8; j++)
                {
                    raw[i, j] = row[j];
                }
            }
        }
        else
        {
            Console.SetError(Console.Out);

            for (var i = 0; i < 8; i++)
            {
                var row = args[i].ToCharArray();
                for (var j = 0; j < 8; j++)
                {
                    raw[i, j] = row[j];
                }
            }
        }

        var board = new Board(raw);
        foreach (var position in board.AvailablePositions)
        {
            Solve(board, position);
            if (board.Completed)
            {
                break;
            }
        }

        board.Print();
    }

    private static void Solve(Board board, Point position, int depth = 0)
    {
        Console.Error.WriteLine($"{depth} - Current: {position}");
        board.AddQueen(position);
        board.ShowCurrentBoard();

        if (board.Completed)
        {
            return;
        }

        var nextPositions = board.AvailablePositions;
        if (nextPositions.Count > 0)
        {
            Console.Error.WriteLine(
                $"{depth} - Next available points: {string.Join(", ", nextPositions.Select(p => p.ToString()))}");
            foreach (var nextPosition in board.AvailablePositions)
            {
                Solve(board, nextPosition, depth + 1);
                if (board.Completed)
                {
                    return;
                }
            }
        }

        Console.Error.WriteLine("Dead end");
        board.RemoveQueen(position);
    }

    private class Board
    {
        private readonly char[,] _raw = new char[8, 8];
        private readonly List<Queen> _queens;
        private List<Point> _availablePositions = [];

        public Board(char[,] raw)
        {
            _queens = GetInitialQueens(raw);
            Update();
        }

        public IReadOnlyList<Point> AvailablePositions => _availablePositions.AsReadOnly();

        public bool Completed => _queens.Count == 8;

        private void Update()
        {
            _availablePositions = [];
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    if (_queens.Exists(q => q.Row == i && q.Column == j))
                    {
                        _raw[i, j] = 'Q';
                    }
                    else
                    {
                        var threaten = _queens.Exists(q => q.Threats(i, j));
                        if (!threaten)
                        {
                            _availablePositions.Add(new Point(i, j));
                        }

                        _raw[i, j] = threaten ? 'X' : '.';
                    }
                }
            }
        }

        private static List<Queen> GetInitialQueens(char[,] raw)
        {
            var queens = new List<Queen>();
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    if (raw[i, j] == 'Q')
                    {
                        queens.Add(new Queen(i, j));
                    }
                }
            }

            return queens;
        }

        public void ShowCurrentBoard()
        {
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    Console.Error.Write(_raw[i, j]);
                }

                Console.Error.WriteLine();
            }

            Console.Error.WriteLine();
        }

        public void Print()
        {
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    Console.Write(_raw[i, j] == 'Q' ? 'Q' : '.');
                }

                Console.WriteLine();
            }
        }

        public void AddQueen(Point position)
        {
            _queens.Add(new Queen(position.X, position.Y));
            Update();
        }

        public void RemoveQueen(Point position)
        {
            var queen = _queens.Single(q => q.Row == position.X && q.Column == position.Y);
            _queens.Remove(queen);
            Update();
        }
    }

    private class Queen
    {
        public Queen(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }

        public int Column { get; }

        public bool Threats(int row, int column)
        {
            if (row == Row || column == Column)
            {
                return true;
            }

            const int maximumDistance = 7;

            for (var i = 1; i <= maximumDistance; i++)
            {
                if (Row + i == row && (Column + i == column || Column - i == column))
                {
                    return true;
                }

                if (Row - i == row && (Column + i == column || Column - i == column))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class TestCases
    {
        public static string[] Test1 =>
        [
            "Q.......",
            "........",
            "...Q....",
            "........",
            ".......Q",
            ".Q......",
            "........",
            "........",
        ];

        public static string[] Test2 =>
        [
            "........",
            "..Q.....",
            "....Q...",
            "......Q.",
            "........",
            "........",
            "........",
            "........",
        ];
    }
}