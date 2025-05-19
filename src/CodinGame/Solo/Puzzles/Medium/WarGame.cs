namespace CodinGame.Solo.Puzzles.Medium;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class WarGame
{
    private enum WarResult
    {
        EquallyFirst,
        Player1Wins,
        Player2Wins,
    }

    private enum CardValue
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jocker,
        Queen,
        King,
        Ace,
    }

    public static void Main(string[] args)
    {
        var player1Cards = new List<string>();
        var player2Cards = new List<string>();

        if (args.Length == 0)
        {
            var n = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            Console.Error.WriteLine($"{n}");
            for (var i = 0; i < n; i++)
            {
                var card = Console.ReadLine()!;
                Console.Error.WriteLine($"{card}");
                player1Cards.Add(card);
            }

            var m = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            Console.Error.WriteLine($"{m}");
            for (int i = 0; i < m; i++)
            {
                var card = Console.ReadLine()!;
                Console.Error.WriteLine($"{card}");
                player2Cards.Add(card);
            }
        }
        else
        {
            Console.SetError(Console.Out);
            var player1CardCount = int.Parse(args[0], CultureInfo.InvariantCulture);
            player1Cards.AddRange(args.Skip(1).Take(player1CardCount));
            player2Cards.AddRange(args.Skip(player1CardCount + 2));
        }

        var deck1 = Deck.Create(player1Cards);
        var deck2 = Deck.Create(player2Cards);

        var result = ProcessWar(deck1, deck2);
        Console.WriteLine(result.Player == 0 ? "PAT" : $"{result.Player} {result.Round}");
    }

    private static (int Player, int Round) ProcessWar(Deck player1Deck, Deck player2Deck)
    {
        var gameRound = 0;

        while (true)
        {
            if (player1Deck.Pile.Count == 0)
            {
                return (2, gameRound);
            }

            if (player2Deck.Pile.Count == 0)
            {
                return (1, gameRound);
            }

            gameRound += 1;

            Console.Error.WriteLine($"Round {gameRound}");

            var player1Card = player1Deck.ShowNextCard();
            var player2Card = player2Deck.ShowNextCard();

            if (player1Card.CardValue > player2Card.CardValue)
            {
                Console.Error.WriteLine($"Player 1 won: {player1Card} > {player2Card}");
                player1Deck.EnqueueCards([player1Card, player2Card]);
            }
            else if (player2Card.CardValue > player1Card.CardValue)
            {
                Console.Error.WriteLine($"Player 2 won: {player1Card} < {player2Card}");
                player2Deck.EnqueueCards([player1Card, player2Card]);
            }
            else
            {
                Console.Error.WriteLine($"War {player1Card} == {player2Card}");
                var result = Step2War(
                    player1Card,
                    player2Card,
                    player1Deck,
                    player2Deck);

                if (result == WarResult.EquallyFirst)
                {
                    return (0, 0);
                }
            }

            Console.Error.WriteLine($"Deck 1 {player1Deck}");
            Console.Error.WriteLine($"Deck 2 {player2Deck}");
        }
    }

    private static WarResult Step2War(
        Card player1WarCard,
        Card player2WarCard,
        Deck player1Deck,
        Deck player2Deck)
    {
        var player1WarCards = new List<Card>()
        {
            player1WarCard,
        };
        var player2WarCards = new List<Card>()
        {
            player2WarCard,
        };

        while (true)
        {
            if (!player1Deck.Has4Cards || !player2Deck.Has4Cards)
            {
                return WarResult.EquallyFirst;
            }

            player1WarCards.AddRange(player1Deck.ShowNext3Cards());
            player2WarCards.AddRange(player2Deck.ShowNext3Cards());

            var player1Card = player1Deck.ShowNextCard();
            player1WarCards.Add(player1Card);
            var player2Card = player2Deck.ShowNextCard();
            player2WarCards.Add(player2Card);

            if (player1Card.CardValue > player2Card.CardValue)
            {
                Console.Error.WriteLine($"War - Player 1 won: {player1Card} > {player2Card}");
                player1Deck.EnqueueCards(player1WarCards);
                player1Deck.EnqueueCards(player2WarCards);
                return WarResult.Player1Wins;
            }

            if (player2Card.CardValue > player1Card.CardValue)
            {
                Console.Error.WriteLine($"War - Player 2 won: {player1Card} < {player2Card}");
                player2Deck.EnqueueCards(player1WarCards);
                player2Deck.EnqueueCards(player2WarCards);
                return WarResult.Player2Wins;
            }

            Console.Error.WriteLine($"Inner WAR: {player1Card} == {player2Card}");
        }
    }

    private class Deck
    {
        private Deck()
        {
            Pile = new Queue<Card>();
        }

        public Queue<Card> Pile { get; }

        public bool Has4Cards => Pile.Count >= 4;

        public static Deck Create(List<string> cards)
        {
            var deck = new Deck();
            foreach (var card in cards)
            {
                deck.Pile.Enqueue(Card.Create(card));
            }

            return deck;
        }

        public Card ShowNextCard()
        {
            var nextCard = Pile.Dequeue();
            return nextCard;
        }

        public List<Card> ShowNext3Cards()
        {
            return [Pile.Dequeue(), Pile.Dequeue(), Pile.Dequeue()];
        }

        public void EnqueueCards(IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                Pile.Enqueue(card);
            }
        }

        public override string ToString()
        {
            return $"{string.Join(", ", Pile.ToList())}";
        }
    }

    private class Card
    {
        public string Raw { get; private set; } = null!;

        public CardValue CardValue { get; init; }

        public char Suit { get; init; }

        public static Card Create(string cardAsString)
        {
            var cardValue = cardAsString[..^1] switch
            {
                "2" => CardValue.Two,
                "3" => CardValue.Three,
                "4" => CardValue.Four,
                "5" => CardValue.Five,
                "6" => CardValue.Six,
                "7" => CardValue.Seven,
                "8" => CardValue.Eight,
                "9" => CardValue.Nine,
                "10" => CardValue.Ten,
                "J" => CardValue.Jocker,
                "Q" => CardValue.Queen,
                "K" => CardValue.King,
                "A" => CardValue.Ace,
                _ => throw new InvalidOperationException(),
            };

            return new Card()
            {
                CardValue = cardValue,
                Suit = cardAsString[^1],
                Raw = cardAsString,
            };
        }

        public override string ToString()
        {
            return Raw;
        }
    }

    public static class TestCases
    {
        public static string[] ThreeCards =>
        [
            "3",
            "AD",
            "KC",
            "QC",
            "3",
            "KH",
            "QS",
            "JC",
        ];

        public static string[] Battle =>
        [
            "5",
            "8C",
            "KD",
            "AH",
            "QH",
            "2S",
            "5",
            "8D",
            "2D",
            "3H",
            "4D",
            "3S",
        ];

        public static string[] OneGameOneBattle =>
        [
            "26",
            "10H",
            "KD",
            "6C",
            "10S",
            "8S",
            "AD",
            "QS",
            "3D",
            "7H",
            "KH",
            "9D",
            "2D",
            "JC",
            "KS",
            "3S",
            "2S",
            "QC",
            "AC",
            "JH",
            "7D",
            "KC",
            "10D",
            "4C",
            "AS",
            "5D",
            "5S",
            "26",
            "2H",
            "9C",
            "8C",
            "4S",
            "5C",
            "AH",
            "JD",
            "QH",
            "7C",
            "5H",
            "4H",
            "6H",
            "6S",
            "QD",
            "9H",
            "10C",
            "4D",
            "JS",
            "6D",
            "3H",
            "8H",
            "3C",
            "7S",
            "9S",
            "8D",
            "2C",
        ];

        public static string[] LongGame =>
        [
            "26",
            "AH",
            "4H",
            "5D",
            "6D",
            "QC",
            "JS",
            "8S",
            "2D",
            "7D",
            "JD",
            "JC",
            "6C",
            "KS",
            "QS",
            "9D",
            "2C",
            "5S",
            "9S",
            "6S",
            "8H",
            "AD",
            "4D",
            "2H",
            "2S",
            "7S",
            "8C",
            "26",
            "10H",
            "4C",
            "6H",
            "3C",
            "KC",
            "JH",
            "10C",
            "AS",
            "5H",
            "KH",
            "10S",
            "9H",
            "9C",
            "8D",
            "5C",
            "AC",
            "3H",
            "4S",
            "KD",
            "7C",
            "3S",
            "QH",
            "10D",
            "3D",
            "7H",
            "QD",
        ];
    }
}