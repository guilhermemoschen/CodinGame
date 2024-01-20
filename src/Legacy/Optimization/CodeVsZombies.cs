using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codingame.CodeVSZombies.Version1
{
    public class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Start();
            //Console.WriteLine(spell);
        }
    }

    public class Game
    {
        public Board Board { get; set; }

        public Game()
        {
            Board = new Board();
        }

        public void Start()
        {
            // game loop
            while (true)
            {
                UpdateBoard();
                
                var nextDestination = CalculateNextDestination();
                //var nextDestination = "1000 2000";
                Console.WriteLine(nextDestination);
            }
        }

        private string CalculateNextDestination()
        {
            var target = Board.GetClosestHuman();


            return target.GetCoordenatesAsString();
        }

        private void UpdateBoard()
        {
            var ash = new Ash(Console.ReadLine());

            var humanCount = Convert.ToInt32(Console.ReadLine());
            var humans = new List<Character>();
            for (var i = 0; i < humanCount; i++)
            {
                humans.Add(new Character(Console.ReadLine()));
            }

            var zombieCount = Convert.ToInt32(Console.ReadLine());
            var zombies = new List<Zombie>();
            for (var i = 0; i < zombieCount; i++)
            {
                zombies.Add(new Zombie(Console.ReadLine()));
            }

            Board.Update(ash, humans, zombies);
        }
    }

    public class Board
    {
        public const int Height = 9000;
        public const int Width = 16000;

        public IList<Zombie> Zombies { get; protected set; }
        public IList<Character> Humans { get; protected set; }
        public Ash Ash { get; protected set; }

        public Board()
        {
            Zombies = new List<Zombie>();
            Humans = new List<Character>();
        }

        public void Update(Ash ash, IList<Character> humans, IList<Zombie> zombies)
        {
            Ash = ash;
            Humans = humans;
            Zombies = zombies;
        }

        public Character GetClosestHuman()
        {
            return Humans
                .Where(x => CanBeSaved(x))
                .OrderBy(x => x.GetDistance(Ash))
                .First();
        }

        private bool CanBeSaved(Character human)
        {
            var zombie = GetClosestZombie(human);
            if (zombie == null)
                return true;
            
            var turnsUntilSave = Math.Floor(human.GetDistance(Ash) / Ash.DisntacePerTurn);
            var turnsUntilDie = Math.Floor(human.GetDistance(zombie) / Zombie.DisntacePerTurn);
            
            return turnsUntilSave - 1 <= turnsUntilDie;
        }

        public Zombie GetClosestZombie(Character human)
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
        {
            var inputs = input.Split(' ');
            X = Convert.ToInt32(inputs[0]);
            Y = Convert.ToInt32(inputs[1]);
        }
    }

    public class Zombie : Character
    {
        public const int DisntacePerTurn = 400;

        public int NextX { get; set; }
        public int NextY { get; set; }

        public Zombie(string input) :
            base(input)
        {
            var inputs = input.Split(' ');
            NextX = Convert.ToInt32(inputs[3]);
            NextY = Convert.ToInt32(inputs[4]);
        }
    }

    public class Character
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Character() { }

        public Character(string input)
        {
            var inputs = input.Split(' ');
            Id = Convert.ToInt32(inputs[0]);
            X = Convert.ToInt32(inputs[1]);
            Y = Convert.ToInt32(inputs[2]);
        }

        public double GetDistance(Character character)
        {
            return Math.Sqrt(Math.Pow(character.X - X, 2) + Math.Pow(character.Y - Y, 2)) / 2;
        }

        public string GetCoordenatesAsString()
        {
            return string.Format("{0} {1}", X, Y);
        }
    }
}
