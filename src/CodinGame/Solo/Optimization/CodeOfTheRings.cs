using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codingame.CodeOfTheRings.Version4
{
    public class Player
    {
        static void Main(string[] args)
        {
            string magicPhrase;
            if (args.Length == 0)
            {
                magicPhrase = Console.ReadLine()!;
            }
            else
            {
                magicPhrase = Game.Tenletterwordx8;
            }

            var game = new Game(magicPhrase);
            var spell = game.CreateMaigicSpell();
            Console.WriteLine(spell);
        }
    }

    public class Game
    {
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        public const int TotalZones = 30;
        public string Zones { get; set; }
        public int ZonesIndex { get; set; }
        public string MagicPhrase { get; set; }
        public int MagicPhraseIndex { get; set; }

        public char CurrentZoneChar
        {
            get { return Zones[ZonesIndex]; }
        }

        public char CurrentMagicPhraseChar
        {
            get { return MagicPhrase[MagicPhraseIndex]; }
        }

        public bool IsFinished
        {
            get { return MagicPhraseIndex >= MagicPhrase.Length; }
        }

        public const string AZ = "AZ";
        public const string ShortSpell = "UMNE TALMAR RAHTAINE NIXENEN UMIR";
        public const string FarAwayLetters = "GUZ MUG ZOG GUMMOG ZUMGUM ZUM MOZMOZ MOG ZOGMOG GUZMUGGUM";
        public const string OneLetterx15 = "OOOOOOOOOOOOOOO";
        public const string OneLetterx31 = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        public const string OneLetterx53OneLetterx38 = "SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";
        public const string Twoletterwordx20 = "NONONONONONONONONONONONONONONONONONONONO";
        public const string Tenletterwordx8 = "GAAVOOOLLUGAAVOOOLLUGAAVOOOLLUGAAVOOOLLUGAAVOOOLLUGAAVOOOLLUGAAVOOOLLUGAAVOOOLLU";
 
        public Game(string magicPhrase)
        {
            MagicPhrase = magicPhrase;
            Zones = string.Empty;
            for (var i = 0; i < TotalZones; i++)
                Zones += " ";

            ZonesIndex = 0;
            MagicPhraseIndex = 0;

            //CreateDictionary();
        }

        private void CreateDictionary()
        {
            throw new NotImplementedException();
        }

        public string CreateMaigicSpell()
        {
            var instructions = string.Empty;
            while (!IsFinished)
                instructions += CalculateNextInstruction();

            return instructions;
        }

        private void ReplaceZone(char value)
        {
            Zones = string.Format("{0}{1}{2}", Zones.Substring(0, ZonesIndex), value, Zones.Substring(ZonesIndex + 1));
        }

        private string CalculateNextInstruction()
        {
            var targetString = GetNextString();
            var count = GetRepeatCount(targetString.Length);

            var steps = string.Empty;

            if (CurrentZoneChar.ToString() == targetString)
            {
                MagicPhraseIndex++;
                return ".";
            }
                

            var shortestDistance = Alphabet.Length;
            var bestIndex = 99;
            for (var i = Zones.Length / 2; i > -Zones.Length / 2; i--)
            {
                var index = ZonesIndex + i;
                if (index < 0)
                    index += TotalZones;
                else if (index >= TotalZones)
                {
                    index -= TotalZones;
                }

                var newPath = CalculateInstructionLength(Zones[index], CurrentMagicPhraseChar);
                var newDistance = Math.Abs(i);
                if (newPath != null)
                    newDistance += newPath.Distance;
                if (newDistance < shortestDistance)
                {
                    bestIndex = i;
                    shortestDistance = newDistance;
                }
            }

            if (bestIndex < 0)
            {
                steps += new string('<', Math.Abs(bestIndex));
            }
            else if (bestIndex > 0)
            {
                steps += new string('>', Math.Abs(bestIndex));
            }

            UpdateZoneIndex(bestIndex);

            for (var i = 0; i < targetString.Length; i++)
            {
                var path = CalculateInstructionLength(CurrentZoneChar, targetString[i]);
                if (path != null)
                {
                    if (path.Direction == Direction.Forward)
                    {
                        steps += new string('+', path.Distance);
                    }
                    else if (path.Direction == Direction.Backward)
                    {
                        steps += new string('-', path.Distance);
                    }

                    ReplaceZone(targetString[i]);
                }

                if (i < targetString.Length - 1)
                {
                    steps += ">";
                    UpdateZoneIndex(1);
                }
            }

            if (count > 1 && ShouldUseLoop(targetString.Length, count))
            {
                steps += CalculateStepsForLoop(targetString.Length, count);
            }
            else
            {
                steps += new string('.', count);
            }

            MagicPhraseIndex += targetString.Length * count;

            return steps;
        }

        private string GetNextString()
        {
            for (var i = 1; i <= 11  && i < MagicPhrase.Length / 2; i++)
            {
                var repeatCount = GetRepeatCount(i);

                if (ShouldUseLoop(i, repeatCount))
                    return MagicPhrase.Substring(MagicPhraseIndex, i);
            }

            return CurrentMagicPhraseChar.ToString();
        }

        private int GetRepeatCount(int length)
        {
            var repeatCount = 1;
            if (MagicPhraseIndex + length >= MagicPhrase.Length)
                return repeatCount;

            var targetString = MagicPhrase.Substring(MagicPhraseIndex, length);

            for (var j = MagicPhraseIndex + length; j < MagicPhrase.Length && j + length < MagicPhrase.Length; j += length)
            {
                var nextString = MagicPhrase.Substring(j, length);
                if (nextString != targetString)
                    break;

                repeatCount++;
            }

            return repeatCount;
        }

        private string CalculateStepsForLoop(int length, int count)
        {
            var steps = ">";

            while (count != 0)
            {
                if (count < 20 && length == 1)
                {
                    steps += "<";
                    steps += new string('.', count);
                    count = 0;
                }
                else
                {
                    if (count > Alphabet.Length / 2)
                    {
                        if (count > Alphabet.Length - 1)
                        {
                            steps += new string('-', 1);
                            count -= Alphabet.Length - 1;
                        }
                        else
                        {
                            steps += new string('-', Alphabet.Length - count);
                            count = 0;
                        }
                    }
                    else
                    {
                        steps += new string('+', count);
                        count = 0;
                    }

                    steps += "[";

                    steps += new string('<', length);

                    for (int i = 0; i < length; i++)
                    {
                        steps += ".>";
                    }
                    
                    steps += "-]";

                    if (count == 0)
                    {
                        UpdateZoneIndex(1);
                        ReplaceZone(' ');
                    }
                }
            }

            return steps;
        }

        private bool ShouldUseLoop(int count)
        {
            var nextIndex = ZonesIndex + 1;
            if (nextIndex >= TotalZones)
                nextIndex -= TotalZones;

            var previewsIndex = ZonesIndex - 1;
            if (previewsIndex < 0)
                previewsIndex += TotalZones;

            return Zones[previewsIndex] == ' ' && count >= 20;
        }

        private bool ShouldUseLoop(int length, int count)
        {
            switch (length)
            {
                case 1:
                    return count >= 20;

                case 2:
                    return count > 10;

                case 3:
                    return count > 5;

                case 4:
                    return count > 3;

                case 5:
                    return count > 2;

                default:
                    return count > 1;
            }
        }

        private void UpdateZoneIndex(int offset)
        {
            ZonesIndex += offset;
            if (ZonesIndex < 0)
                ZonesIndex += TotalZones;
            else if (ZonesIndex >= TotalZones)
            {
                ZonesIndex -= TotalZones;
            }
        }

        private Path? CalculateInstructionLength(char currentChar, char targetChar)
        {
            var currentIndex = Alphabet.IndexOf(currentChar);

            var targetIndex = Alphabet.IndexOf(targetChar);

            if (currentIndex == targetIndex)
                return null;

            var distance = targetIndex - currentIndex;

            var path = new Path();

            if (Math.Abs(distance) < Alphabet.Length / 2)
            {
                if (distance > 0)
                    path.Direction = Direction.Forward;
                else
                {
                    path.Direction = Direction.Backward;
                    distance = Math.Abs(distance);
                }
            }
            else
            {
                if (distance > 0)
                {
                    path.Direction = Direction.Backward;
                    distance = Alphabet.Length - distance;
                }
                else
                {
                    path.Direction = Direction.Forward;
                    distance = Alphabet.Length + distance;
                }
            }


            path.Distance = distance;

            return path;
        }
    }

    public enum Direction
    {
        Forward,
        Backward
    }

    public class Path
    {
        public int Distance { get; set; }
        public Direction Direction { get; set; }
    }
}