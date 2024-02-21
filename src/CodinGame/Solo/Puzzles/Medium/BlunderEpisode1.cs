namespace CodinGame.Solo.Puzzles.Medium;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

public static class BlunderEpisode1
{
    private static bool localDebug;

    private enum BlunderDirection
    {
        Invalid,
        South,
        East,
        North,
        West,
    }

    public static void Main(string[] args)
    {
        char[][] boardAsString;
        int rows;
        int columns;

        if (args.Length == 0)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            rows = int.Parse(inputs[0], CultureInfo.InvariantCulture);
            columns = int.Parse(inputs[1], CultureInfo.InvariantCulture);
            boardAsString = new char[rows][];
            for (int i = 0; i < rows; i++)
            {
                boardAsString[i] = Console.ReadLine()!.ToCharArray();
            }
        }
        else
        {
            localDebug = true;
            var inputs = args[0].Split(' ');
            rows = int.Parse(inputs[0], CultureInfo.InvariantCulture);
            columns = int.Parse(inputs[1], CultureInfo.InvariantCulture);
            boardAsString = new char[rows][];
            for (int i = 0; i < rows; i++)
            {
                boardAsString[i] = args[i + 1].ToCharArray();
            }
        }

        Log($"{rows} {columns}");
        foreach (var row in boardAsString)
        {
            Log(new string(row));
        }

        var board = new Board(rows, columns, boardAsString.ToArray());
        var solver = new Solver(board);
        solver.Solve();

        foreach (var move in solver.GetMoves())
        {
            Console.WriteLine(move);
        }
    }

    private static void Log(string message)
    {
        if (!localDebug)
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    private class Solver
    {
        private readonly Board _board;
        private readonly List<string> _movesSequence = new();
        private bool _loop;
        private bool _finish;
        private bool _breaker;
        private bool _inverter;
        private int _blunderColumn;
        private int _blunderRow;

        public Solver(Board board)
        {
            _board = board;
        }

        public void Solve()
        {
            BlunderDirection currentMove = BlunderDirection.South;

            var position = _board.GetPosition(Board.Blunder);
            _blunderColumn = position.Column;
            _blunderRow = position.Row;

            while (!_loop && !_finish)
            {
                if (localDebug)
                {
                    Console.Clear();
                }

                var currentCharacter = _board.BoardAsString[_blunderRow][_blunderColumn];
                currentMove = ProcessCurrentCharacter(currentMove, currentCharacter);
                var nextCharacter = GetNextCharacter(currentMove);

                while (!CanBlunderMove(nextCharacter))
                {
                    currentMove = ProcessNextMove(currentMove, nextCharacter);
                    nextCharacter = GetNextCharacter(currentMove);
                }

                MoveBlunder(currentMove);
            }
        }

        private void MoveBlunder(BlunderDirection direction)
        {
            _board.MoveBlunder(_blunderRow, _blunderColumn, direction, _breaker);
            Log($"Blunder moved to {direction}");
            switch (direction)
            {
                case BlunderDirection.South:
                    _blunderRow += 1;
                    break;

                case BlunderDirection.East:
                    _blunderColumn += 1;
                    break;

                case BlunderDirection.North:
                    _blunderRow -= 1;
                    break;

                case BlunderDirection.West:
                    _blunderColumn -= 1;
                    break;
            }

            _movesSequence.Add(direction.ToString().ToUpper(CultureInfo.InvariantCulture));

            if (_board.BoardAsString[_blunderRow][_blunderColumn] == Board.Suicide)
            {
                _finish = true;
            }

            if (InLoop())
            {
                _loop = true;
            }
        }

        private bool InLoop()
        {
            var moves = _movesSequence.ToArray();
            const int minimumSubGroupSize = 15;

            if (moves.Length <= minimumSubGroupSize * 2)
            {
                return false;
            }

            for (var i = 0; i < moves.Length; i += 1)
            {
                var currentSubGroupSize = minimumSubGroupSize;
                var subGroup = GetSubGroup(moves, i, currentSubGroupSize);
                while (subGroup.Length != 0)
                {
                    if (HasRepeated(moves, i, subGroup))
                    {
                        return true;
                    }

                    currentSubGroupSize += 1;
                    subGroup = GetSubGroup(moves, i, currentSubGroupSize);
                }
            }

            return false;
        }

        private static string[] GetSubGroup(string[] moves, int index, int currentSubGroupSize)
        {
            if (currentSubGroupSize * 2 > moves.Length - index)
            {
                return Array.Empty<string>();
            }

            return moves.Skip(index).Take(currentSubGroupSize).ToArray();
        }

        private static bool HasRepeated(string[] moves, int index, string[] subGroup)
        {
            var remainigMoves = moves.Skip(index).ToArray();

            if (subGroup.Length > remainigMoves.Length)
            {
                return false;
            }

            for (var i = subGroup.Length; i < remainigMoves.Length; i += subGroup.Length)
            {
                var nextMoves = remainigMoves.Skip(i).Take(subGroup.Length).ToArray();

                if (EqualMoves(nextMoves, subGroup))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EqualMoves(string[] movesA, string[] movesB)
        {
            if (movesA.Length != movesB.Length)
            {
                return false;
            }

            for (int i = 0; i < movesA.Length; i++)
            {
                if (movesA[i] != movesB[i])
                {
                    return false;
                }
            }

            return true;
        }

        private BlunderDirection ProcessCurrentCharacter(BlunderDirection currentDirection, char currentCharacter)
        {
            switch (currentCharacter)
            {
                case Board.ChangeDirectionToSouth:
                    return BlunderDirection.South;

                case Board.ChangeDirectionToEast:
                    return BlunderDirection.East;

                case Board.ChangeDirectionToNorth:
                    return BlunderDirection.North;

                case Board.ChangeDirectionToWest:
                    return BlunderDirection.West;

                case Board.Beer:
                    _breaker = !_breaker;
                    Log($"breaker {_breaker}");
                    return currentDirection;

                case Board.Inverter:
                    _inverter = !_inverter;
                    Log($"inverter {_inverter}");
                    return currentDirection;

                case Board.Teleporter:
                    var otherTeleport = _board.GetTeleportTo(_blunderRow, _blunderColumn);
                    Log($"Teleported to {otherTeleport}");
                    _blunderColumn = otherTeleport.Column;
                    _blunderRow = otherTeleport.Row;
                    return currentDirection;
            }

            return currentDirection;
        }

        private bool CanBlunderMove(char nextCharacter)
        {
            if (nextCharacter == Board.UnbreakableObstacle)
            {
                return false;
            }

            if (nextCharacter == Board.BreakableObstacle && !_breaker)
            {
                return false;
            }

            return true;
        }

        private char GetNextCharacter(BlunderDirection nextMove)
        {
            switch (nextMove)
            {
                case BlunderDirection.South:
                    return _board.BoardAsString[_blunderRow + 1][_blunderColumn];

                case BlunderDirection.East:
                    return _board.BoardAsString[_blunderRow][_blunderColumn + 1];

                case BlunderDirection.North:
                    return _board.BoardAsString[_blunderRow - 1][_blunderColumn];

                case BlunderDirection.West:
                    return _board.BoardAsString[_blunderRow][_blunderColumn - 1];
            }

            return '!';
        }

        private BlunderDirection ProcessNextMove(BlunderDirection direction, char nextCharacter)
        {
            switch (nextCharacter)
            {
                case Board.BreakableObstacle:
                case Board.UnbreakableObstacle:
                    return ProcessObstacle(direction, nextCharacter);

                case Board.ChangeDirectionToSouth:
                    return BlunderDirection.South;

                case Board.ChangeDirectionToEast:
                    return BlunderDirection.East;

                case Board.ChangeDirectionToNorth:
                    return BlunderDirection.North;

                case Board.ChangeDirectionToWest:
                    return BlunderDirection.West;
            }

            throw new ArgumentException("Invalid Character", nameof(nextCharacter));
        }

        private BlunderDirection ProcessObstacle(BlunderDirection currentDirection, char obstable)
        {
            if (_breaker && obstable == Board.BreakableObstacle)
            {
                return currentDirection;
            }

            List<BlunderDirection> directions;
            if (_inverter)
            {
                directions = new List<BlunderDirection>()
                {
                    BlunderDirection.West,
                    BlunderDirection.North,
                    BlunderDirection.East,
                    BlunderDirection.South,
                };
            }
            else
            {
                directions = new List<BlunderDirection>()
                {
                    BlunderDirection.South,
                    BlunderDirection.East,
                    BlunderDirection.North,
                    BlunderDirection.West,
                };
            }

            foreach (var direction in directions)
            {
                var nextCharacter = GetNextCharacter(direction);
                if (CanBlunderMove(nextCharacter))
                {
                    return direction;
                }
            }

            return BlunderDirection.Invalid;
        }

        public string[] GetMoves()
        {
            if (_loop)
            {
                return new[] { "LOOP" };
            }

            return _movesSequence.ToArray();
        }
    }

    private class Board
    {
        public const char BreakableObstacle = 'X';
        public const char UnbreakableObstacle = '#';
        public const char ChangeDirectionToSouth = 'S';
        public const char ChangeDirectionToEast = 'E';
        public const char ChangeDirectionToNorth = 'N';
        public const char ChangeDirectionToWest = 'W';
        public const char Beer = 'B';
        public const char Inverter = 'I';
        public const char Teleporter = 'T';

        public static readonly char Space = ' ';
        public static readonly char Blunder = '@';
        public static readonly char Suicide = '$';

        public Board(int rows, int columns, char[][] boardAsString)
        {
            Rows = rows;
            Columns = columns;
            BoardAsString = boardAsString;
        }

        public int Rows { get; }

        public int Columns { get; }

        public char[][] BoardAsString { get; }

        public (int Row, int Column) GetPosition(char charecter)
        {
            for (var row = 0; row < Rows; row += 1)
            {
                for (var column = 0; column < Columns; column += 1)
                {
                    if (BoardAsString[row][column] == charecter)
                    {
                        return (row, column);
                    }
                }
            }

            return (-1, -1);
        }

        public void MoveBlunder(int row, int column, BlunderDirection nextMove, bool destroyCharacter)
        {
            var nextY = row;
            var nextX = column;

            switch (nextMove)
            {
                case BlunderDirection.South:
                    nextY += 1;
                    break;

                case BlunderDirection.East:
                    nextX += 1;
                    break;

                case BlunderDirection.North:
                    nextY -= 1;
                    break;

                case BlunderDirection.West:
                    nextX -= 1;
                    break;
            }

            if (BoardAsString[row][column] == Blunder)
            {
                BoardAsString[row][column] = Space;
            }

            if (BoardAsString[nextY][nextX] == Space || destroyCharacter && BoardAsString[nextY][nextX] == BreakableObstacle)
            {
                BoardAsString[nextY][nextX] = Blunder;
            }

            Print();
        }

        public void Print()
        {
            for (var row = 0; row < Rows; row += 1)
            {
                Log(new string(BoardAsString[row]));
            }
        }

        public (int Row, int Column) GetTeleportTo(int fromRow, int fromColumn)
        {
            for (var row = 0; row < Rows; row += 1)
            {
                for (var column = 0; column < Columns; column += 1)
                {
                    if (row == fromRow && column == fromColumn)
                    {
                        continue;
                    }

                    if (BoardAsString[row][column] == Teleporter)
                    {
                        return (row, column);
                    }
                }
            }

            return (-1, -1);
        }
    }

    public static class TestCases
    {
        public static readonly string[] Sample = new string[]
        {
            "10 10",
            "##########",
            "#        #",
            "#  S   W #",
            "#        #",
            "#  $     #",
            "#        #",
            "#@       #",
            "#        #",
            "#E     N #",
            "##########",
        };

        public static readonly string[] SimpleMoves = new string[]
        {
            "5 5",
            "#####",
            "#@  #",
            "#   #",
            "#  $#",
            "#####",
        };

        public static readonly string[] Obstacles = new string[]
        {
            "8 8",
            "########",
            "# @    #",
            "#     X#",
            "# XXX  #",
            "#   XX #",
            "#   XX #",
            "#     $#",
            "########",
        };

        public static readonly string[] BreakerMode = new string[]
        {
            "10 10",
            "##########",
            "# @      #",
            "# B      #",
            "#XXX     #",
            "# B      #",
            "#    BXX$#",
            "#XXXXXXXX#",
            "#        #",
            "#        #",
            "##########",
        };

        public static readonly string[] Inverter = new string[]
        {
            "10 10",
            "##########",
            "#    I   #",
            "#        #",
            "#       $#",
            "#       @#",
            "#        #",
            "#       I#",
            "#        #",
            "#        #",
            "##########",
        };

        public static readonly string[] Teleport = new string[]
        {
            "10 10",
            "##########",
            "#    T   #",
            "#        #",
            "#        #",
            "#        #",
            "#@       #",
            "#        #",
            "#        #",
            "#    T  $#",
            "##########",
        };

        public static readonly string[] BrokenWall = new string[]
        {
            "10 10",
            "##########",
            "#        #",
            "#  @     #",
            "#  B     #",
            "#  S   W #",
            "# XXX    #",
            "#  B   N #",
            "# XXXXXXX#",
            "#       $#",
            "##########",
        };

        public static readonly string[] Loop = new string[]
        {
            "15 15",
            "###############",
            "#      IXXXXX #",
            "#  @          #",
            "#E S          #",
            "#             #",
            "#  I          #",
            "#  B          #",
            "#  B   S     W#",
            "#  B   T      #",
            "#             #",
            "#         T   #",
            "#         B   #",
            "#N          W$#",
            "#        XXXX #",
            "###############",
        };

        public static readonly string[] MultipleLoops = new string[]
        {
            "30 15",
            "###############",
            "#  #@#I  T$#  #",
            "#  #    IB #  #",
            "#  #     W #  #",
            "#  #      ##  #",
            "#  #B XBN# #  #",
            "#  ##      #  #",
            "#  #       #  #",
            "#  #     W #  #",
            "#  #      ##  #",
            "#  #B XBN# #  #",
            "#  ##      #  #",
            "#  #       #  #",
            "#  #     W #  #",
            "#  #      ##  #",
            "#  #B XBN# #  #",
            "#  ##      #  #",
            "#  #       #  #",
            "#  #       #  #",
            "#  #      ##  #",
            "#  #  XBIT #  #",
            "#  #########  #",
            "#             #",
            "# ##### ##### #",
            "# #     #     #",
            "# #     #  ## #",
            "# #     #   # #",
            "# ##### ##### #",
            "#             #",
            "###############",
        };
    }
}
