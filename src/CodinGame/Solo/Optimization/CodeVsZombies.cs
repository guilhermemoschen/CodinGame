namespace CodinGame.Solo.Optimization;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class CodeVsZombies
{
    public static void Main(string[] args)
    {
        var game = new Game();
        game.Start();
    }

    public class Game
    {
        public Game()
        {
            Board = new Board();
        }

        public Board Board { get; }

        public void Start()
        {
            while (true)
            {
                UpdateBoard();
                PrintData();
                var nextDestination = CalculateNextDestination();
                Console.WriteLine(nextDestination);
            }
        }

        private void PrintData()
        {
            Console.Error.WriteLine("Statistics");
            foreach (var zombie in Board.Zombies)
            {
                Console.Error.WriteLine($"{zombie}");
                Console.Error.WriteLine($"Ash can kill in {zombie.GetDistance(Board.Ash) / Ash.DisntacePerTurn} turns");
                var closestHuman = Board.GetClosestHuman(zombie);
                closestHuman.WillDieIn = zombie.GetDistance(closestHuman) / Zombie.DisntacePerTurn;
                Console.Error.WriteLine($"Closest human is {closestHuman}");
            }
        }

        private string CalculateNextDestination()
        {
            var target = Board.Humans.OrderBy(h => h.WillDieIn).First();
            return target.GetCoordenatesAsString();
        }

        private void UpdateBoard()
        {
            var ash = new Ash(Console.ReadLine()!);

            var humanCount = Convert.ToInt32(Console.ReadLine(), CultureInfo.InvariantCulture);
            var humans = new List<Human>();
            for (var i = 0; i < humanCount; i++)
            {
                humans.Add(new Human(Console.ReadLine()!));
            }

            var zombieCount = Convert.ToInt32(Console.ReadLine(), CultureInfo.InvariantCulture);
            var zombies = new List<Zombie>();
            for (var i = 0; i < zombieCount; i++)
            {
                zombies.Add(new Zombie(Console.ReadLine()!));
            }

            Board.Update(ash, humans, zombies);
        }
    }

    public class Board
    {
        public const int Height = 9000;
        public const int Width = 16000;

        public Board()
        {
            Zombies = new List<Zombie>();
            Humans = new List<Human>();
        }

        public IList<Zombie> Zombies { get; protected set; }

        public IList<Human> Humans { get; protected set; }

        public Ash Ash { get; protected set; } = null!;

        public void Update(Ash ash, IList<Human> humans, IList<Zombie> zombies)
        {
            Ash = ash;
            Humans = humans;
            Zombies = zombies;
        }

        public Human GetClosestHuman(Character character)
        {
            return Humans
                .OrderBy(x => x.GetDistance(character))
                .First();
        }

        public Zombie? GetClosestZombie(Character human)
        {
            return Zombies
                .OrderBy(x => x.GetDistance(human))
                .FirstOrDefault();
        }
    }

    public class Ash : Character
    {
        public const int DisntacePerTurn = 1000;

        public Ash(string input)
            : base()
        {
            var inputs = input.Split(' ');
            X = Convert.ToInt32(inputs[0], CultureInfo.InvariantCulture);
            Y = Convert.ToInt32(inputs[1], CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return $"Ash ({X}, {Y})";
        }
    }

    public class Zombie : Character
    {
        public const int DisntacePerTurn = 400;

        public Zombie(string input)
            : base(input)
        {
            var inputs = input.Split(' ');
            FutureX = Convert.ToInt32(inputs[3], CultureInfo.InvariantCulture);
            FutureY = Convert.ToInt32(inputs[4], CultureInfo.InvariantCulture);
        }

        public int FutureX { get; set; }

        public int FutureY { get; set; }

        public override string ToString()
        {
            return $"Zombie {Id} ({X}, {Y})";
        }
    }

    public class Human : Character
    {
        public Human(string input)
            : base(input)
        {
        }

        public int WillDieIn { get; set; } = int.MaxValue;

        public override string ToString()
        {
            return $"Human {Id} ({X}, {Y}) will die in {WillDieIn}";
        }
    }

    public abstract class Character
    {
        protected Character()
        {
        }

        protected Character(string input)
        {
            var inputs = input.Split(' ');
            Id = Convert.ToInt32(inputs[0], CultureInfo.InvariantCulture);
            X = Convert.ToInt32(inputs[1], CultureInfo.InvariantCulture);
            Y = Convert.ToInt32(inputs[2], CultureInfo.InvariantCulture);
        }

        public int Id { get; }

        public int X { get; init; }

        public int Y { get; init; }

        public int GetDistance(Character character)
        {
            if (X == character.X)
            {
                return Math.Abs(Y - character.Y);
            }

            if (Y == character.Y)
            {
                return Math.Abs(X - character.X);
            }

            var a = Math.Abs(X - character.X);
            var b = Math.Abs(Y - character.Y);

            return (int)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        public string GetCoordenatesAsString()
        {
            return $"{X} {Y}";
        }
    }
}
