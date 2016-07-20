using System;
using System.Data.Odbc;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Collections.Generic;
/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
using System.Threading;

namespace Codingame
{
    public class CrossRoad
    {
        public Point Position { get; set; }
        public CrossRoadOption CameFrom { get; set; }
        public List<CrossRoadOption> Options { get; set; }

        public CrossRoad()
        {
            Options = new List<CrossRoadOption>();
        }
    }

    public class CrossRoadOption
    {
        public string Direction { get; set; }
        public bool Visited { get; set; }
    }

    public class Node
    {
        public List<string> StepsToNext { get; set; }

        public Node()
        {
            StepsToNext = new List<string>();
        }
    }

    public enum KirkStatus
    {
        Discovering = 0,
        GoingToControlRoom = 1,
        GoingToStartingPoint = 2,
    }

    public class TheLabyrinth
    {
        public const char Wall = '#';
        public const char FreeSpace = '.';
        public const char StartingPoistion = 'T';
        public const char ControlRoom = 'C';
        public const char NotScanned = '?';

        public const string Right = "RIGHT";
        public const string Left = "LEFT";
        public const string Up = "UP";
        public const string Down = "DOWN";

        public static List<CrossRoad> Decisions { get; set; }

        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int R = int.Parse(inputs[0]); // number of rows.
            int C = int.Parse(inputs[1]); // number of columns.
            int A = int.Parse(inputs[2]);
            // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

            string lastDirection = null;
            var kirkStatus = KirkStatus.Discovering;
            Decisions = new List<CrossRoad>();

            var testBoard = new List<string>
            {
                "???????????????????###########",
                "???????????????????###########",
                "?...#.######.#.????.#......T##",
                "?##.#.######.#.######.########",
                "##...........#.######.########",
                "###.#.######......###.##??????",
                "#...#.###....#.##.###.##??????",
                "#.############.##.....##??????",
                "#......##......#########??????",
                "###.###############.....??????",
                "###.#####......##?????????????",
                "?##...????????????????????????",
                "?#####????????????????????????",
                "?###C.????????????????????????",
                "??????????????????????????????",
            };


            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var kirkPosition = new Point(int.Parse(inputs[1]), int.Parse(inputs[0]));
                var mazeAsString = new List<string>();

                for (var i = 0; i < R; i++)
                {
                    var row = Console.ReadLine();
                    //Console.Error.WriteLine(row);
                    mazeAsString.Add(row);
                }

                //for (var i = 0; i < testBoard.Count; i++)
                //{
                //    var row = testBoard[i];
                //    mazeAsString.Add(row);
                //}

                var maze = new Maze();
                maze.CreateMaze(mazeAsString);

                var direction = string.Empty;

                switch (kirkStatus)
                {
                    case KirkStatus.Discovering:
                        if (maze.IsValid(kirkPosition))
                        {
                            kirkStatus = KirkStatus.GoingToControlRoom;
                            Console.Error.WriteLine("We can solve this maze");
                            Console.Error.WriteLine("Going to Control Room");
                            direction = GetNextStepToControlRoom(maze, kirkPosition);
                        }
                        else
                        {
                            Console.Error.WriteLine("We need to discover more about this maze");
                            direction = GetDirectionForDiscovering(lastDirection, kirkPosition, maze);
                            lastDirection = direction;
                        }
                        break;

                    case KirkStatus.GoingToControlRoom:
                        if (kirkPosition == maze.FindSquare(SquareContent.ControlRoom).Position)
                        {
                            kirkStatus = KirkStatus.GoingToStartingPoint;
                            Console.Error.WriteLine("Reached Control Room");
                            Console.Error.WriteLine("Going to Starting Point");
                            direction = GetNextStepToStartingPoint(maze, kirkPosition);
                        }
                        else
                        {
                            Console.Error.WriteLine("Going to Control Room");
                            direction = GetNextStepToControlRoom(maze, kirkPosition);
                        }

                        break;
                    case KirkStatus.GoingToStartingPoint:
                        Console.Error.WriteLine("Going to Starting Point");
                        direction = GetNextStepToStartingPoint(maze, kirkPosition);
                        break;
                }

                Console.WriteLine(direction);
            }
        }

        private static string GetNextStepToStartingPoint(Maze maze, Point kirkPosition)
        {
            var startingPoint = maze.FindSquare(kirkPosition);
            var destination = maze.FindSquare(SquareContent.StartingPoint);
            return maze.GetNextStepToDestination(startingPoint, destination);
        }

        private static string GetNextStepToControlRoom(Maze maze, Point kirkPosition)
        {
            var startingPoint = maze.FindSquare(kirkPosition);
            var destination = maze.FindSquare(SquareContent.ControlRoom);
            return maze.GetNextStepToDestination(startingPoint, destination);
        }

        private static string GetDirectionForDiscovering(string lastDirection, Point kirkPosition, Maze maze)
        {
            if (lastDirection == null)
            {
                Console.Error.WriteLine("First Step");
                var options = GetOptions(kirkPosition, maze);

                if (options.Count == 1)
                    return options.FirstOrDefault().Direction;

                var crossRoad = new CrossRoad()
                {
                    CameFrom = null,
                    Position = kirkPosition,
                };

                crossRoad.Options.AddRange(options);
                Decisions.Add(crossRoad);

                var selectedPath = crossRoad.Options.FirstOrDefault();
                selectedPath.Visited = true;
                return selectedPath.Direction;
            }

            var decision = GetPastDecision(kirkPosition);
            if (decision == null) // no
            {
                var options = GetOptions(kirkPosition, maze);

                if (options.Count == 1)
                    return options.FirstOrDefault().Direction;

                if (options.Count == 2)
                {
                    var direction = options.FirstOrDefault(x => x.Direction != InvertDirection(lastDirection)).Direction;
                    return direction;
                }

                Console.Error.WriteLine("never been here");

                var cameFrom = GetCameFrom(options, lastDirection);
                options.Remove(cameFrom);

                var crossRoad = new CrossRoad()
                {
                    CameFrom = cameFrom,
                    Position = kirkPosition,
                };
                crossRoad.Options.AddRange(options);
                Decisions.Add(crossRoad);
                var selectedPath = crossRoad.Options.FirstOrDefault();
                selectedPath.Visited = true;
                return selectedPath.Direction;
            }

            Console.Error.WriteLine("been here");

            var notVisitedPath = decision.Options.FirstOrDefault(x => !x.Visited);

            if (notVisitedPath == null)
            {
                Console.Error.WriteLine("need to go back");
                return decision.CameFrom.Direction;
            }

            Console.Error.WriteLine("going to new path");

            notVisitedPath.Visited = true;
            return notVisitedPath.Direction;
        }

        private static string InvertDirection(string direction)
        {
            switch (direction)
            {
                case Right:
                    return Left;

                case Left:
                    return Right;

                case Up:
                    return Down;

                case Down:
                    return Up;
            }

            return null;
        }

        private static CrossRoad GetPastDecision(Point kirkPosition)
        {
            return Decisions.FirstOrDefault(x => x.Position == kirkPosition);
        }

        private static CrossRoadOption GetCameFrom(IList<CrossRoadOption> options, string lastDirection)
        {
            return options.FirstOrDefault(x => x.Direction == InvertDirection(lastDirection));
        }

        private static IList<CrossRoadOption> GetOptions(Point kirkPosition, Maze maze)
        {
            var options = new List<CrossRoadOption>();

            if (maze.CanWalk(Right, kirkPosition))
                options.Add(new CrossRoadOption() { Direction = Right });

            if (maze.CanWalk(Down, kirkPosition))
                options.Add(new CrossRoadOption() { Direction = Down });

            if (maze.CanWalk(Left, kirkPosition))
                options.Add(new CrossRoadOption() { Direction = Left });

            if (maze.CanWalk(Up, kirkPosition))
                options.Add(new CrossRoadOption() { Direction = Up });

            return options;
        }
    }

    public class Maze
    {
        public bool IsValid(Point kirkPosition)
        {
            var controlRoom = FindSquare(SquareContent.ControlRoom);
            var startingPoint = FindSquare(SquareContent.StartingPoint);

            if (controlRoom == null || startingPoint == null)
                return false;

            var kirkSquare = FindSquare(kirkPosition);

            // Can reach Control Room
            if (!FindShortsPath(kirkSquare, controlRoom))
                return false;

            var knowingDistance = GetShortestDistance(controlRoom, startingPoint, false);
            var unknowingDistance = GetShortestDistance(controlRoom, startingPoint, true);

            return unknowingDistance == knowingDistance;
        }

        private int GetShortestDistance(Square initial, Square end, bool includeUnkown)
        {
            UpdateDistances(initial, includeUnkown);
            return end.DistanceSteps;
        }

        public Square[][] Board { get; set; }

        public void CreateMaze(IList<string> maze)
        {
            Board = new Square[maze.Count][];
            Square.MaxDistance = maze.Count * maze[0].Length;
            for (var i = 0; i < maze.Count; i++)
            {
                Board[i] = new Square[maze[i].Length];
                for (var j = 0; j < maze[i].Length; j++)
                {
                    Board[i][j] = new Square(maze[i][j], j, i);
                }
            }
        }

        private void UpdateDistances(Square startingPoint, bool includeUnkown)
        {
            ResetDistances();
            
            startingPoint.DistanceSteps = 0;

            while (true)
            {
                var madeProgress = false;

                // Look at each square on the board.
                foreach (var square in Board.SelectMany(x => x).Where(x => x.CanMove(includeUnkown)))
                {
                    // look through valid moves given the coordinates of that square.
                    foreach (var move in GetValidMoves(square, includeUnkown))
                    {
                        var newPass = square.DistanceSteps + 1;
                        if (move.DistanceSteps > newPass)
                        {
                            move.DistanceSteps = newPass;
                            madeProgress = true;
                        }
                    }
                }

                if (!madeProgress)
                    break;
            }
        }

        private void ResetDistances()
        {
            foreach (var square in Board.SelectMany(x => x))
            {
                square.DistanceSteps = Square.MaxDistance;
            }
        }

        public Square FindSquare(SquareContent content)
        {
            return Board.SelectMany(row => row).FirstOrDefault(cell => cell.Content == content);
        }

        private IEnumerable<Square> GetValidMoves(Square square, bool includeUnkown = false)
        {
            var moves = new List<Square>();

            // can go right
            if (square.Position.X < Board[0].Length - 1 && Board[square.Position.Y][square.Position.X + 1].CanMove(includeUnkown))
            {
                moves.Add(Board[square.Position.Y][square.Position.X + 1]);
            }

            // can go left
            if (square.Position.X > 0 && Board[square.Position.Y][square.Position.X - 1].CanMove(includeUnkown))
            {
                moves.Add(Board[square.Position.Y][square.Position.X - 1]);
            }

            // can go Up
            if (square.Position.Y > 0 && Board[square.Position.Y - 1][square.Position.X].CanMove(includeUnkown))
            {
                moves.Add(Board[square.Position.Y - 1][square.Position.X]);
            }

            // can go Down
            if (square.Position.Y < Board.Length - 1 && Board[square.Position.Y + 1][square.Position.X].CanMove(includeUnkown))
            {
                moves.Add(Board[square.Position.Y + 1][square.Position.X]);
            }

            return moves;
        }

        public string GetNextStepToDestination(Square start, Square end)
        {
            FindShortsPath(start, end);
            //DisplayDistances();
            //DisplayMaze();

            var currentSquare = start;
            // Right
            if (Board[currentSquare.Position.Y].Length - 1 > currentSquare.Position.X)
            {
                if (Board[currentSquare.Position.Y][currentSquare.Position.X + 1].IsPath)
                    return TheLabyrinth.Right;
            }

            // Left
            if (currentSquare.Position.X > 0)
            {
                if (Board[currentSquare.Position.Y][currentSquare.Position.X - 1].IsPath)
                    return TheLabyrinth.Left;
            }

            // Up
            if (currentSquare.Position.Y > 0)
            {
                if (Board[currentSquare.Position.Y - 1][currentSquare.Position.X].IsPath)
                    return TheLabyrinth.Up;
            }

            // Left
            if (Board.Length - 1 > currentSquare.Position.Y)
            {
                if (Board[currentSquare.Position.Y + 1][currentSquare.Position.X].IsPath)
                    return TheLabyrinth.Down;
            }

            return null;

        }

        public bool FindShortsPath(Square start, Square end)
        {
            var startingPoint = start;
            var endingPoint = end;

            UpdateDistances(startingPoint, false);
            endingPoint.IsPath = true;
            var currentPoint = endingPoint;

            while (true)
            {
                // Look through each direction and find the square
                // with the lowest number of steps marked.
                Square lowestSquare = null;
                var lowestDistance = Square.MaxDistance;

                foreach (var square in GetValidMoves(currentPoint))
                {
                    if (square.DistanceSteps < lowestDistance)
                    {
                        lowestDistance = square.DistanceSteps;
                        lowestSquare = square;
                    }
                }

                if (lowestSquare == null || lowestSquare.DistanceSteps == Square.MaxDistance)
                    break;

                currentPoint = lowestSquare;

                if (currentPoint.Position == startingPoint.Position)
                {
                    return true;
                }

                // Mark the square as part of the path if it is the lowest
                // number. Set the current position as the square with
                // that number of steps.
                lowestSquare.IsPath = true;
            }

            return false;
        }

        public bool CanWalk(string direction, Point currentPosition)
        {
            switch (direction)
            {
                case TheLabyrinth.Right:
                    currentPosition.Offset(1, 0);
                    return CanWalk(currentPosition);

                case TheLabyrinth.Left:
                    currentPosition.Offset(-1, 0);
                    return CanWalk(currentPosition);

                case TheLabyrinth.Up:
                    currentPosition.Offset(0, -1);
                    return CanWalk(currentPosition);

                case TheLabyrinth.Down:
                    currentPosition.Offset(0, 1);
                    return CanWalk(currentPosition);
            }

            return false;
        }

        public bool CanWalk(Point destination)
        {
            if (destination.Y < 0 ||
                destination.Y >= Board.Length ||
                destination.X < 0 ||
                destination.X >= Board[0].Length)
                return false;

            return Board[destination.Y][destination.X].CanMove(false, false);
        }

        public Square FindSquare(Point position)
        {
            if (position.Y < 0 ||
                position.Y >= Board.Length ||
                position.X < 0 ||
                position.X >= Board[0].Length)
                return null;

            return Board[position.Y][position.X];
        }

        private void DisplayDistances()
        {
            foreach (var row in Board)
            {
                var line = string.Empty;

                foreach (var cell in row)
                {
                    if (cell.DistanceSteps == Square.MaxDistance)
                    {
                        line += "**";
                    }
                    else
                    {
                        if (cell.DistanceSteps < 10)
                            line += "0";

                        line += cell.DistanceSteps;
                    }
                }
                Console.Error.WriteLine(line);
            }
        }

        private void DisplayMaze()
        {
            foreach (var row in Board)
            {
                var line = string.Empty;

                foreach (var cell in row)
                {
                    line += cell.ContentAsString;
                }
                Console.Error.WriteLine(line);
            }
        }
    }

    public enum SquareContent
    {
        Empty,
        ControlRoom,
        Kirk,
        Wall,
        StartingPoint,
        Unknown,
    }

    public class Square
    {
        public static int MaxDistance { get; set; }

        public SquareContent Content { get; set; }
        public int DistanceSteps { get; set; }
        public bool IsPath { get; set; }
        public Point Position { get; set; }
        public bool Visited { get; set; }

        public string ContentAsString
        {
            get
            {
                if (IsPath)
                    return "*";

                switch (Content)
                {
                    case SquareContent.Empty:
                        return ".";

                    case SquareContent.Wall:
                        return "#";

                    case SquareContent.StartingPoint:
                        return "T";

                    case SquareContent.Kirk:
                        return "K";

                    case SquareContent.ControlRoom:
                        return "C";

                    case SquareContent.Unknown:
                        return "?";
                }

                return null;
            }
        }

        public Square(char content, int x, int y)
        {
            switch (content)
            {
                case '.':
                    Content = SquareContent.Empty;
                    break;

                case '#':
                    Content = SquareContent.Wall;
                    break;

                case 'T':
                    Content = SquareContent.StartingPoint;
                    break;

                case 'K':
                    Content = SquareContent.Kirk;
                    break;

                case 'C':
                    Content = SquareContent.ControlRoom;
                    break;

                case '?':
                    Content = SquareContent.Unknown;
                    break;
            }

            DistanceSteps = MaxDistance;
            Position = new Point(x, y);
        }

        public bool CanMove(bool includeUnknown = false, bool includeControlRoom = true)
        {
            var canMove =
                Content == SquareContent.Empty ||
                Content == SquareContent.StartingPoint ||
                Content == SquareContent.Kirk;
            
            if (includeControlRoom)
                canMove = canMove || Content == SquareContent.ControlRoom;

            if (includeUnknown)
                canMove = canMove || Content == SquareContent.Unknown || canMove;

            return canMove;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Position.X, Position.Y, Content);
        }
    }
}