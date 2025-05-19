using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace CodinGame.Solo.Puzzles.Hard;

public static class TheFallEpisode2
{
    public static void Main(string[] args)
    {
        var inputs = Console.ReadLine()!.Split(' ');
        var width = int.Parse(inputs[0], CultureInfo.InvariantCulture);
        var height = int.Parse(inputs[1], CultureInfo.InvariantCulture);
        var lines = new List<string>();

        for (var i = 0; i < height; i++)
        {
            // represents a line in the grid and contains W integers. Each integer represents one room of a given type.
            lines.Add(Console.ReadLine()!);
        }

        var exit = new Point(height - 1, int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture));
        var tunnel = new Tunnel(width, height, lines, exit);

        while (true)
        {
            inputs = Console.ReadLine()!.Split(' ');
            var indyX = int.Parse(inputs[0], CultureInfo.InvariantCulture);
            var indyY = int.Parse(inputs[1], CultureInfo.InvariantCulture);
            var indyEntrancePoint = Enum.Parse<EntrancePoint>(inputs[2], true);

            var rockAmount = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            for (var i = 0; i < rockAmount; i++)
            {
                tunnel.AddRock(new Rock(Console.ReadLine()!));
            }

            var nextPosition = tunnel.GetNextPosition(indyX, indyY, indyEntrancePoint);

            Console.WriteLine($"{nextPosition.X} {nextPosition.Y}");
        }
    }

    private class Tunnel
    {
        private readonly Room[,] _rooms;
        private List<Rock> _rocks = null!;

        public Tunnel(int width, int height, List<string> lines, Point exit)
        {
            _rooms = new Room[width, height];

            for (var i = 0; i < height; i += 1)
            {
                var roomTypes = lines[i].Split(' ');
                for (var j = 0; j < width; j += 1)
                {
                    var locked = roomTypes[j][0] == '-';
                    var exitRoom = exit.X == j && exit.Y == i;
                    _rooms[j, i] = new Room(Enum.Parse<RoomType>($"Type{roomTypes[j].Replace("-", string.Empty)}", true), locked, exitRoom);
                }
            }
        }

        public Point GetNextPosition(int indyX, int indyY, EntrancePoint entrancePoint)
        {
            var exitPosition = _rooms[indyX, indyY].CalculateExitPoint(entrancePoint);
            return exitPosition switch
            {
                ExitPoint.Down => new Point(indyX, indyY + 1),
                ExitPoint.Left => new Point(indyX - 1, indyY),
                ExitPoint.Right => new Point(indyX + 1, indyY),
                _ => throw new InvalidOperationException(),
            };
        }

        public string GetNextCommand(int indyX, int indyY, EntrancePoint entrancePoint)
        {
            return "WAIT";
        }

        public void AddRock(Rock rock)
        {
            _rooms[rock.Position.X, rock.Position.Y].AddRock(rock);
        }
    }

    private class Room
    {
        private readonly RoomType _type;

        public Room(RoomType type, bool locked, bool exit)
        {
            _type = type;
            Locked = locked;
            Exit = exit;
        }

        public bool Locked { get; }

        public bool Exit { get; }

        public Rock? Rock { get; private set; }

        public ExitPoint CalculateExitPoint(EntrancePoint entrancePoint)
        {
            switch (_type)
            {
                case RoomType.Type0:
                    throw new InvalidOperationException("Room type 0 is not supported.");

                case RoomType.Type1:
                    return ExitPoint.Down;

                case RoomType.Type2:
                case RoomType.Type6:
                    if (entrancePoint == EntrancePoint.Top)
                    {
                        break;
                    }

                    return entrancePoint == EntrancePoint.Left
                        ? ExitPoint.Right
                        : ExitPoint.Left;

                case RoomType.Type3:
                    if (entrancePoint != EntrancePoint.Top)
                    {
                        break;
                    }

                    return ExitPoint.Down;

                case RoomType.Type4:
                    if (entrancePoint == EntrancePoint.Top)
                    {
                        return ExitPoint.Left;
                    }

                    if (entrancePoint == EntrancePoint.Right)
                    {
                        return ExitPoint.Down;
                    }

                    break;

                case RoomType.Type5:
                    if (entrancePoint == EntrancePoint.Top)
                    {
                        return ExitPoint.Right;
                    }

                    if (entrancePoint == EntrancePoint.Left)
                    {
                        return ExitPoint.Down;
                    }

                    break;

                case RoomType.Type7:
                    if (entrancePoint == EntrancePoint.Left)
                    {
                        break;
                    }

                    return ExitPoint.Down;

                case RoomType.Type8:
                    if (entrancePoint == EntrancePoint.Top)
                    {
                        break;
                    }

                    return ExitPoint.Down;

                case RoomType.Type9:
                    if (entrancePoint == EntrancePoint.Right)
                    {
                        break;
                    }

                    return ExitPoint.Down;

                case RoomType.Type10:
                    if (entrancePoint == EntrancePoint.Top)
                    {
                        return ExitPoint.Left;
                    }

                    break;

                case RoomType.Type11:
                    if (entrancePoint == EntrancePoint.Top)
                    {
                        return ExitPoint.Right;
                    }

                    break;

                case RoomType.Type12:
                    if (entrancePoint == EntrancePoint.Right)
                    {
                        return ExitPoint.Down;
                    }

                    break;

                case RoomType.Type13:
                    if (entrancePoint == EntrancePoint.Left)
                    {
                        return ExitPoint.Down;
                    }

                    break;
            }

            throw new ArgumentException($"Invalid entrance point for {_type}", nameof(entrancePoint));
        }

        public void AddRock(Rock rock)
        {
            Rock = rock;
        }
    }

    private enum RoomType
    {
        Type0,
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
        Type6,
        Type7,
        Type8,
        Type9,
        Type10,
        Type11,
        Type12,
        Type13,
    }

    private enum EntrancePoint
    {
        Top,
        Left,
        Right,
    }

    private enum ExitPoint
    {
        Down,
        Left,
        Right,
    }

    private class Rock
    {
        public Rock(string input)
        {
            var inputs = input.Split(' ');

            Position = new Point(int.Parse(inputs[0], CultureInfo.InvariantCulture), int.Parse(inputs[1], CultureInfo.InvariantCulture));
            EntrancePoint = Enum.Parse<EntrancePoint>(inputs[2], true);
        }

        public Point Position { get; }

        public EntrancePoint EntrancePoint { get; }
    }
}