using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace Codingame.TheParanoidAndroid
{
    class Player
    {
        static void Main(string[] args)
        {
            var game = new Game(Console.ReadLine());

            for (var i = 0; i < game.AmountOfElevators; i++)
            {
                game.AddElevatorPosition(Console.ReadLine());
            }

            Console.Error.WriteLine(game);

            while (true)
            {
                var leadClone = new ClonePosition(Console.ReadLine());
                Console.Error.WriteLine(leadClone);

                if (!game.IsCalculated)
                    game.CalculateBlockPositions(leadClone);

                Console.WriteLine(game.GetNextAction(leadClone));
            }
        }
    }

    public class Game
    {
        public int AmountOfFloors { get; set; }
        public Point ExitPosition { get; set; }
        public int AmountOfElevators { get; set; }
        public IList<Point> ElevatorsPositions { get; set; }
        public IList<Point> BlockPoisitions { get; set; }

        public bool IsCalculated
        {
            get { return BlockPoisitions.Any(); }
        }

        public Game(string input)
        {
            Console.Error.WriteLine(input);
            var inputs = input.Split(' ');
            AmountOfFloors = Convert.ToInt32(inputs[0]);
            ExitPosition = new Point(Convert.ToInt32(inputs[4]), Convert.ToInt32(inputs[3]));
            AmountOfElevators = Convert.ToInt32(inputs[7]);
            ElevatorsPositions = new List<Point>();

            BlockPoisitions = new List<Point>();
        }

        public void AddElevatorPosition(string input)
        {
            var inputs = input.Split(' ');
            ElevatorsPositions.Add(new Point(Convert.ToInt32(inputs[1]), Convert.ToInt32(inputs[0])));
            ElevatorsPositions = ElevatorsPositions.OrderBy(x => x.Y).ToList();
        }

        public override string ToString()
        {
            return string.Format(
                "Amount of Floors {0} Amount of Elevators {1} Exit Position {2}",
                AmountOfFloors,
                AmountOfElevators,
                ExitPosition);
        }

        public void CalculateBlockPositions(ClonePosition initialClonePosition)
        {
            var currentClonePosition = initialClonePosition;

            foreach (var elevatorPosition in ElevatorsPositions)
            {
                var elevatorDirection = Direction.None;
                if (currentClonePosition.Position.X < elevatorPosition.X)
                    elevatorDirection = Direction.Right;
                
                if (currentClonePosition.Position.X > elevatorPosition.X)
                    elevatorDirection = Direction.Left;

                var nextClonePosition = new ClonePosition()
                {
                    Position = new Point(elevatorPosition.X, elevatorPosition.Y + 1)
                };

                if (currentClonePosition.Direction != elevatorDirection)
                {
                    if (currentClonePosition.Direction == Direction.Right)
                    {
                        BlockPoisitions.Add(new Point(currentClonePosition.Position.X + 1, currentClonePosition.Position.Y));
                        nextClonePosition.Direction = Direction.Left;
                    }
                    else
                    {
                        BlockPoisitions.Add(new Point(currentClonePosition.Position.X - 1, currentClonePosition.Position.Y));
                        nextClonePosition.Direction = Direction.Right;
                    }
                }
                else
                {
                    nextClonePosition.Direction = currentClonePosition.Direction;
                }

                currentClonePosition = nextClonePosition;
            }

            var exitDirection = Direction.None;
            if (currentClonePosition.Position.X < ExitPosition.X)
                exitDirection = Direction.Right;

            if (currentClonePosition.Position.X > ExitPosition.X)
                exitDirection = Direction.Left;

            if (currentClonePosition.Direction != exitDirection)
            {
                if (currentClonePosition.Direction == Direction.Right)
                {
                    BlockPoisitions.Add(new Point(currentClonePosition.Position.X + 1, currentClonePosition.Position.Y));
                }
                else
                {
                    BlockPoisitions.Add(new Point(currentClonePosition.Position.X - 1, currentClonePosition.Position.Y));
                }
            }
        }

        public string GetNextAction(ClonePosition clonePosition)
        {
            return BlockPoisitions.Contains(clonePosition.Position) ? 
                "BLOCK" : 
                "WAIT";
        }
    }

    public class ClonePosition
    {
        public Point Position { get; set; }
        public Direction Direction { get; set; }

        public ClonePosition() { }

        public ClonePosition(string input)
        {
            var inputs = input.Split(' ');
            Position = new Point(Convert.ToInt32(inputs[1]), Convert.ToInt32(inputs[0]));

            switch (inputs[2])
            {
                case "LEFT":
                    Direction = Direction.Left;
                    break;

                case "RIGHT":
                    Direction = Direction.Right;
                    break;

                case "NONE":
                    Direction = Direction.None;
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "Clone is at {0} going to {1}", 
                Position,
                Direction);
        }
    }

    public enum Direction
    {
        None,
        Left,
        Right
    }
}
