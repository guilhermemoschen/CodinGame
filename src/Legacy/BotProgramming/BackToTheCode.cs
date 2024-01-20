using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;

namespace Codingame.BackToTheCode.Version4
{
    public class Program
    {
        static void Main(string[] args)
        {
            var opponentsInput = new string[Convert.ToInt32(Console.ReadLine())];
            var game = new Game();

            while (true)
            {
                game.UpdateGameRound(Console.ReadLine());
                game.UpdatePlayer(Console.ReadLine());

                for (var i = 0; i < opponentsInput.Length; i++)
                    opponentsInput[i] = Console.ReadLine();

                game.UpdateOpponents(opponentsInput);

                var rows = new Collection<string>();
                for (var i = 0; i < 20; i++)
                    rows.Add(Console.ReadLine());

                game.UpdateBoard(rows);

                var action = game.GetNextAction();
                if (action != null)
                {
                    Console.WriteLine(action);
                }
                else
                {
                    Console.Error.WriteLine("Couldn't define the next action");
                    break;
                }
            }
        }
    }

    public class Game
    {
        public const int TargetSquareLength = 7;
        public const int TargetSquareOffset = 5;
        public const int MaxWidth = 10;
        public const int MinWidth = 5;
        public const int MaxHeight = 8;
        public const int MinHeight = 4;
        public const int ExpansionOffset = 1;

        protected Rectangle TargetRectangle;
        protected Board Board;
        protected int GameRound;
        protected Player Player;
        protected IList<Opponent> Opponents;
        protected GameStatus Status;
        protected Strategy CurrentStrategy;

        public Game()
        {
            CurrentStrategy = new OrganicRectangle();
            Status = GameStatus.FirstStrategy;
        }

        public void UpdateGameRound(string gameRound)
        {
            GameRound = Convert.ToInt32(gameRound);
            CurrentStrategy.UpdatedGamneRound(GameRound);
        }

        public void UpdatePlayer(string playerData)
        {
            if (Player == null)
                Player = new Player(playerData);
            else
                Player.Update(playerData);

            CurrentStrategy.Player = Player;
        }

        public void UpdateOpponents(string[] opponentsData)
        {
            for (var i = 0; i < opponentsData.Length; i++)
            {
                CurrentStrategy.UpdateOpponent(i + 1, opponentsData[i]);
            }
        }

        public void UpdateBoard(IList<string> rows)
        {
            Board = new Board(rows);
            CurrentStrategy.Board = Board;
        }

        public string GetNextAction()
        {
            //Console.Error.WriteLine(Status);
            var nextAction = CurrentStrategy.GetNextAction();

            if (nextAction != null)
                return nextAction;
            else
                UpdateStatus();

            return GetNextAction();
        }

        public void UpdateStatus()
        {
            Console.Error.WriteLine("UpdateStatus");
        }
    }

    public enum GameStatus
    {
        CreatingRectangle,
        FinishingRectangle,
        FirstStrategy,
        SearchingForRectangles,
        GoingToPoints,
    }

    public abstract class Strategy
    {
        public Board Board { get; set; }
        public Player Player { get; set; }
        public IList<Opponent> Opponents { get; protected set; }

        protected Dictionary<int, bool> OpponentsBackInTimeUsage; 

        public int GameRound { get; protected set; }
        protected int LastGameRound;

        protected List<Point> NextPoints;

        protected Strategy()
        {
            NextPoints = new List<Point>();
            Opponents = new List<Opponent>();
            OpponentsBackInTimeUsage = new Dictionary<int, bool>();
            LastGameRound = 0;
            GameRound = 0;
        }

        protected Strategy(Board board, Player player, IList<Opponent> opponents)
            : this()
        {
            Board = board;
            Player = player;
            Opponents = opponents;
        }

        protected string GetNextExistentAction()
        {
            if (!NextPoints.Any())
                return null;

            var nextPoint = NextPoints.First();
            if (nextPoint != Player.Position)
            {
                return nextPoint.ToAction();
            }

            if (Opponents.Any(x => x.Position == Player.Position))
            {
                var nextNextFreePosition = GetNextNextFreePosition();
                if (nextNextFreePosition == Board.EmptyPoint)
                    return nextPoint.ToAction();

                NextPoints.Insert(0, nextNextFreePosition);
                return nextNextFreePosition.ToAction();
            }

            NextPoints.Remove(nextPoint);

            if (NextPoints.Any())
            {
                nextPoint = NextPoints.First();
                return nextPoint.ToAction();
            }

            return null;
        }

        protected abstract Point GetNextNextFreePosition();

        public string GetNextAction()
        {
            return GetNextExistentAction() ?? DefineNextAction();
        }

        protected virtual string DefineNextAction()
        {
            return null;
        }

        public IEnumerable<Point> GetBestPathForRectangle(Rectangle rectangle)
        {
            var path = new List<Point>();
            var lastPoint = new Point(Player.Position.X, Player.Position.Y);

            Point initialReference;

            if (Player.Position.X > rectangle.X && Player.Position.X < rectangle.X + (rectangle.Width - 1))
            {
                initialReference = new Point(Player.Position.X, Player.Position.Y);

                if (rectangle.X == Board.Width - 1)
                {
                    initialReference.X = Board.Width - 1;
                }
                else
                {
                    initialReference.X = rectangle.X;
                }

                path.Add(initialReference);
            }
            else
            {
                initialReference = Player.Position;
            }

            if (Player.Position.Y > rectangle.Y) // Going up
            {
                path.Add(new Point(initialReference.X, rectangle.Y));
            }
            else // Going down
            {
                path.Add(new Point(initialReference.X, rectangle.Y + (rectangle.Height - 1)));
            }

            if (Player.Position.X > rectangle.X) // Going left
            {
                if (Player.Position.Y > rectangle.Y) // already up
                {
                    path.Add(new Point(initialReference.X - (rectangle.Width - 1), rectangle.Y));
                }
                else // already down
                {
                    path.Add(new Point(initialReference.X - (rectangle.Width - 1), rectangle.Y + (rectangle.Height - 1)));
                }

                lastPoint.X--;
            }
            else // Going right
            {
                if (Player.Position.Y > rectangle.Y) // already up
                {
                    path.Add(new Point(initialReference.X + (rectangle.Width - 1), rectangle.Y));
                }
                else // already down
                {
                    path.Add(new Point(initialReference.X + (rectangle.Width - 1), rectangle.Y + (rectangle.Height - 1)));
                }
                lastPoint.X++;
            }

            if (Player.Position.Y > rectangle.Y) // Started as down
            {
                if (Player.Position.X > rectangle.X) // Started as right
                {
                    path.Add(new Point(initialReference.X - (rectangle.Width - 1), rectangle.Y + (rectangle.Height - 1)));
                }
                else // Started as left
                {
                    path.Add(new Point(initialReference.X + (rectangle.Width - 1), rectangle.Y + (rectangle.Height - 1)));
                }
            }
            else // Started as up
            {
                if (Player.Position.X > rectangle.X) // Started as right
                {
                    path.Add(new Point(initialReference.X - (rectangle.Width - 1), rectangle.Y));
                }
                else // Started as left
                {
                    path.Add(new Point(initialReference.X + (rectangle.Width - 1), rectangle.Y));
                }
            }

            if (!rectangle.IsCorner(Player.Position))
            {
                var extraPoint = new Point(initialReference.X, rectangle.Y);
                if (Player.Position.Y > rectangle.Y) // Started as down
                    extraPoint.Y += rectangle.Height - 1;

                path.Add(extraPoint);
                lastPoint = GetNearPointToTargetPoint(extraPoint);
            }

            path.Add(lastPoint);

            return path;
        }

        protected Point GetNearPointToTargetPoint(Point targetPoint)
        {
            var nextPoint = new Point(Player.Position.X, Player.Position.Y);

            if (Player.Position.X == targetPoint.X) // vertical
            {
                if (Player.Position.Y > targetPoint.Y)
                    nextPoint.Y--;
                else
                    nextPoint.Y++;
            }
            else // Horizontal
            {
                if (Player.Position.X > targetPoint.X)
                    nextPoint.X--;
                else
                    nextPoint.X++;
            }

            return nextPoint;
        }

        public Point GetClosestNeutralPositionForRectangle(Rectangle rectangle)
        {
            var nodes = Board.GetEdgeAndNeutralNodes(rectangle);
            nodes = nodes
                .Where(x => x.Position != Player.Position)
                .OrderBy(x => x.Position.GetDistance(Player.Position));

            return nodes.Any() ?
                nodes.First().Position :
                Board.EmptyPoint;
        }

        public void UpdateOpponent(int number, string input)
        {
            var usedBackInTime = false;
            var opponent = Opponents.FirstOrDefault(x => x.Number == number);
            
            if (opponent != null)
            {
                opponent.Update(input);
                if (opponent.UsedBackInTime)
                {
                    if (!OpponentsBackInTimeUsage[opponent.Number])
                    {
                        OpponentsBackInTimeUsage[opponent.Number] = true;
                        usedBackInTime = true;
                    }
                }
            }
            else
            {
                opponent = new Opponent(number, input);
                OpponentsBackInTimeUsage.Add(opponent.Number, opponent.UsedBackInTime);
                Opponents.Add(opponent);
            }

            if (usedBackInTime)
                OppentUsedBackInTime();
        }

        protected abstract void OppentUsedBackInTime();

        public virtual void UpdatedGamneRound(int gameRound)
        {
            LastGameRound = GameRound;
            GameRound = gameRound;
        }
    }

    public class OrganicRectangle : Strategy
    {
        protected StrategyStatus Status = StrategyStatus.Init;
        protected StrategyStatus LastStatus;
        protected Rectangle CurrrentRectangle;
        protected List<Point> StrategyPoints = new List<Point>();

        public OrganicRectangle()
            : base()
        {
            Status = StrategyStatus.Init;
        }

        public OrganicRectangle(Board board, Player player, IList<Opponent> opponents)
            : base(board, player, opponents)
        {
        }

        protected override Point GetNextNextFreePosition()
        {
            var neutralNodes = Board
                .GetAllNodesByType(NodeType.Neutral)
                .OrderBy(x => x.Position.GetRoundsToGoToPoint(Player.Position));
            if (neutralNodes.Count() == 1)
                return Board.EmptyPoint;
            return neutralNodes.Skip(1).First().Position;
        }

        protected override string DefineNextAction()
        {
            Console.Error.WriteLine(Status);

            var nextPosition = Board.EmptyPoint;
            switch (Status)
            {
                case StrategyStatus.Init:
                    CurrrentRectangle = GetFirstRectangle();
                    Console.Error.WriteLine("GetFirstRectangle {0}", CurrrentRectangle);
                    ChangeStatus(StrategyStatus.CreatingFirstRow);
                    nextPosition = GetClosestNeutralPositionForRectangle(CurrrentRectangle);
                    break;

                case StrategyStatus.CreatingFirstRow:
                    if (!Board.IsValidRectangle(CurrrentRectangle))
                    {
                        ChangeStatus(StrategyStatus.FindingNewRow);
                        return DefineNextAction();
                    }
                    
                    nextPosition = GetClosestNeutralPositionForRectangle(CurrrentRectangle);
                    if (nextPosition.IsRealEmpty())
                    {
                        if (ShouldExpandFirstRow(CurrrentRectangle))
                        {
                            CurrrentRectangle = ExpandRectangle(CurrrentRectangle, Game.ExpansionOffset);
                            Console.Error.WriteLine("ExpandFirstRow {0}", CurrrentRectangle);
                        }
                        else
                        {
                            CurrrentRectangle = ExpandFirstRow(CurrrentRectangle);
                            Console.Error.WriteLine("ExpandFirstRow for rectangle {0}", CurrrentRectangle);
                            ChangeStatus(StrategyStatus.CreatingRectangle);
                        }

                        nextPosition = GetClosestNeutralPositionForRectangle(CurrrentRectangle);
                    }
                    break;

                case StrategyStatus.CreatingRectangle:
                    if (Board.IsValidRectangle(CurrrentRectangle))
                    {
                        if (!IsBuildingRectangle(CurrrentRectangle))
                        {
                            Console.Error.WriteLine("Is not Building a Rectangle");
                            nextPosition = GetClosestNeutralPositionForRectangle(CurrrentRectangle);
                        }
                        else
                        {
                            if (CanExpandRectangle(CurrrentRectangle))
                            {
                                CurrrentRectangle = ExpandRectangle(CurrrentRectangle, Game.ExpansionOffset);
                                Console.Error.WriteLine("Expanded to {0}", CurrrentRectangle);
                            }
                            nextPosition = GetClosestNeutralPositionForRectangle(CurrrentRectangle);
                            if (nextPosition.IsRealEmpty())
                            {
                                Console.Error.WriteLine("Finished {0}", CurrrentRectangle);
                                ChangeStatus(StrategyStatus.FindingNewRow);
                                return DefineNextAction();
                            }
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("{0} is not valid ", CurrrentRectangle);
                        ChangeStatus(StrategyStatus.FindingNewRow);
                        return DefineNextAction();
                    }
                    break;

                case StrategyStatus.FindingNewRow:
                    CurrrentRectangle = GetNextRectangle();
                    if (CurrrentRectangle.IsEmpty)
                    {
                        Console.Error.WriteLine("Couldn't find any rectangle");
                        var neutralNodes = Board
                            .GetAllNodesByType(NodeType.Neutral)
                            .OrderBy(x => x.Position.GetRoundsToGoToPoint(Player.Position));

                        if (neutralNodes.Any())
                            nextPosition = neutralNodes.First().Position;
                    }
                    else
                    {
                        Console.Error.WriteLine("Found {0}", CurrrentRectangle);
                        ChangeStatus(StrategyStatus.CreatingRectangle);
                        nextPosition = GetFirstPositionForRectangle(CurrrentRectangle);
                        Console.Error.WriteLine("GetFirstPositionForRectangle {0}", nextPosition);
                    }
                    break;
            }

            if (!nextPosition.IsRealEmpty())
                NextPoints.Add(nextPosition);

            return GetNextExistentAction();
        }

        public bool ShouldExpandFirstRow(Rectangle rectangle)
        {
            switch (Player.Direction)
            {
                case Direction.Right:
                    return rectangle.GetRightX() != Board.Width - 1 && 
                        Board.IsEmptyColumn(rectangle.GetRightX() + 1) &&
                        rectangle.Width < 13;
                
                case Direction.Left:
                    return rectangle.X != 0 &&
                        Board.IsEmptyColumn(rectangle.X - 1) &&
                        rectangle.Width < 13;
                
                case Direction.Up:
                    return rectangle.Y != 0 &&
                        Board.IsEmptyRow(rectangle.Y - 1) &&
                        rectangle.Height < 10;
                
                case Direction.Down:
                    return rectangle.GetBottomY() != Board.Height - 1 && 
                        Board.IsEmptyRow(rectangle.GetBottomY() + 1) &&
                        rectangle.Height < 10;

                default:
                    return false;
            }
        }

        public Point GetFirstPositionForRectangle(Rectangle rectangle)
        {
            var center = rectangle.GetCenter();

            if (Board.IsBottomEdgeFilled(rectangle))
            {
                return center.X > Board.Width - center.X ? 
                    new Point(rectangle.GetRightX(), rectangle.GetBottomY() - 1) :
                    new Point(rectangle.X, rectangle.GetBottomY() - 1);
            }

            if (Board.IsTopEdgeFilled(rectangle))
            {
                return center.X > Board.Width - center.X ?
                    new Point(rectangle.GetRightX(), rectangle.Y + 1) :
                    new Point(rectangle.X, rectangle.Y + 1);
            }

            if (Board.IsLeftEdgeFilled(rectangle))
            {
                return center.Y > Board.Height - center.Y ?
                    new Point(rectangle.X + 1, rectangle.GetBottomY()) :
                    new Point(rectangle.X + 1, rectangle.Y);
            }

            if (Board.IsRightEdgeFilled(rectangle))
            {
                return center.Y > Board.Height - center.Y ?
                    new Point(rectangle.GetRightX() - 1, rectangle.GetBottomY()) :
                    new Point(rectangle.GetRightX() - 1, rectangle.Y);
            }

            return GetClosestNeutralPositionForRectangle(CurrrentRectangle);
        }

        protected override void OppentUsedBackInTime()
        {
            Console.Error.WriteLine("UsedBackInTime");
            ChangeStatus(StrategyStatus.FindingNewRow);

            Player.RemoveActionsUntilGameRound(GameRound);

            NextPoints.Clear();
        }

        public bool IsBuildingRectangle(Rectangle rectangle)
        {
            var nodes = Board.GetEdgeAndNeutralNodes(rectangle);
            return nodes.All(x => x.Position != Player.Position);
        }

        public Rectangle GetNextRectangle()
        {
            return Board.GetPlayerRectangles(4, Player.Position)
                .OrderBy(x => x.GetClosestPointToHit(Player.Position).GetRoundsToGoToPoint(Player.Position))
                .FirstOrDefault();
        }

        public Rectangle ExpandFirstRow(Rectangle rectangle)
        {
            if (rectangle.IsHorizontal()) // horizontal
            {
                rectangle.Height++;
                if (rectangle.Y < Board.Width / 2) // going up
                {
                    rectangle.Y--;
                }
            }
            else if (rectangle.IsVertical()) // vertical
            {
                rectangle.Width++;
                if (rectangle.X > Board.Width / 2) // going left
                {
                    rectangle.X--;
                }
            }

            return rectangle;
        }

        public Rectangle ExpandRectangle(Rectangle rectangle, int offset)
        {
            switch (Player.Direction)
            {
                case Direction.Up:
                    return rectangle.ExpandToTop(Game.ExpansionOffset);

                case Direction.Down:
                    return rectangle.ExpandToBottom(Game.ExpansionOffset);

                case Direction.Left:
                    return rectangle.ExpandToLeft(Game.ExpansionOffset);

                case Direction.Right:
                    return rectangle.ExpandToRight(Game.ExpansionOffset);
            }

            return rectangle;
        }

        public bool CanExpandRectangle(Rectangle rectangle)
        {
            if (!rectangle.IsCorner(Player.Position))
            {
                Console.Error.WriteLine("Not in a corner");
                return false;
            }
            
            var newRectangle = ExpandRectangle(CurrrentRectangle, Game.ExpansionOffset);

            if (!Board.IsValidRectangle(newRectangle))
            {
                Console.Error.WriteLine("Is not a valid rectangle {0}", newRectangle);
                return false;
            }

            if (SomeoneWillHitRectangle(rectangle))
            {
                Console.Error.WriteLine("Someone Will Hit Rectangle {0}", rectangle);
                return false;
            }

            if (!ShouldExpandRectangle(rectangle))
            {
                Console.Error.WriteLine("Should not Expand Rectangle {0}", rectangle);
                return false;
            }

            return true;
        }

        public bool ShouldExpandRectangle(Rectangle rectangle)
        {
            if (rectangle.Width > Board.Width / 2 || rectangle.Height > Board.Height / 2)
                return false;

            var isRightEdgeFilled = Board.IsRightEdgeFilled(rectangle);
            var isLeftEdgeFilled = Board.IsLeftEdgeFilled(rectangle);
            var isTopEdgeFilled = Board.IsTopEdgeFilled(rectangle);
            var isBottomEdgeFilled = Board.IsBottomEdgeFilled(rectangle);

            var amountEdgesFinished = 0;

            if (isRightEdgeFilled)
                amountEdgesFinished++;

            if (isLeftEdgeFilled)
                amountEdgesFinished++;

            if (isTopEdgeFilled)
                amountEdgesFinished++;

            if (isBottomEdgeFilled)
                amountEdgesFinished++;

            if (amountEdgesFinished == 3)
            {
                var remainingNodesCount = Board.GetEdgeAndNeutralNodes(rectangle).Count();
                Console.Error.WriteLine("amountEdgesFinished == 3");
                Console.Error.WriteLine("remainingNodesCount {0}", remainingNodesCount);


                if (isRightEdgeFilled && isLeftEdgeFilled) // missing horizontal edge
                {
                    if (remainingNodesCount < rectangle.Width - 2)
                        return false;
                }
                else if (isTopEdgeFilled && isBottomEdgeFilled) // missing vertical edge
                {
                    if (remainingNodesCount < rectangle.Height - 2)
                        return false;
                }
            }

            return true;
        }

        public bool SomeoneWillHitRectangle(Rectangle rectangle)
        {
            var opponents = Opponents.Where(opponent => rectangle.WillCharacterHit(opponent));

            if (!opponents.Any())
                return false;

            var closestOpponent = opponents.OrderBy(x => x.GetRoundsToHit(rectangle)).First();
            //Console.Error.WriteLine("closestOpponent {0}", closestOpponent);

            var closestPointToHit = rectangle.GetClosestPointToHit(closestOpponent.Position);

            if (Board.Nodes[closestPointToHit.X, closestPointToHit.Y].Type == NodeType.Player)
            {
                Console.Error.WriteLine("Opponent target node is already filled");
                return false;
            }
                
            var opponentRoundsToHit = closestOpponent.GetRoundsToHit(rectangle);
            var roundsToCloseOpponentPath = Player.Position.GetRoundsToGoToPoint(closestPointToHit);

            Console.Error.WriteLine("distanceToHit {0}", opponentRoundsToHit);
            Console.Error.WriteLine("roundsToCloseOpponentPath {0}", roundsToCloseOpponentPath);

            return roundsToCloseOpponentPath >= opponentRoundsToHit - 2;
        }

        public int GetRoundsToCloseRectangle(Rectangle rectangle, Character character)
        {
            switch (character.Direction)
            {
                case Direction.Left:
                case Direction.Right:
                    return Math.Abs(rectangle.Y - character.Position.Y);

                case Direction.Up:
                case Direction.Down:
                    return Math.Abs(rectangle.X - character.Position.X);
            }

            return 0;
        }

        public Rectangle GetFirstRectangle()
        {
            var rectangle = new Rectangle();

            if (Player.Position.Y < Game.MinHeight)
            {
                // vertical line going down
                rectangle.X = Player.Position.X;
                rectangle.Y = Player.Position.Y;
                rectangle.Width = 1;
                rectangle.Height = Game.MaxHeight;
            }
            else if (Player.Position.Y > Board.Height - (Game.MinHeight + 1))
            {
                // vertical line going up
                rectangle.X = Player.Position.X;
                rectangle.Y = Player.Position.Y - (Game.MaxHeight - 1);
                rectangle.Width = 1;
                rectangle.Height = Game.MaxHeight;
            }
            else // horizontal line
            {
                rectangle.Y = Player.Position.Y;
                rectangle.Width = Game.MaxWidth;
                rectangle.Height = 1;

                if (Player.Position.X < Board.Width / 2) // going right
                {
                    rectangle.X = Player.Position.X;
                }
                else // going left
                {
                    rectangle.X = Player.Position.X - (rectangle.Width - 1);
                }
            }

            return rectangle;
        }

        protected void ChangeStatus(StrategyStatus newStatus)
        {
            LastStatus = Status;
            Status = newStatus;
        }

        public override void UpdatedGamneRound(int gameRound)
        {
            GameRound = gameRound;
        }

        public enum StrategyStatus
        {
            Init,
            CreatingFirstRow,
            CreatingRectangle,
            FindingNewRow,
            UsedBackInTime,
        }
    }

    public class Board
    {
        public const int Width = 35;
        public const int Height = 20;
        public static readonly Point EmptyPoint = new Point(-1, -1);
        public int PlayerPoints { get; protected set; }
        public int Opponent1Points { get; protected set; }
        public int Opponent2Points { get; protected set; }
        public int Opponent3Points { get; protected set; }
        public int NeutralPoints { get; protected set; }

        public Board(IList<string> rows)
        {
            Nodes = new Node[Width, Height];
            Opponent1Points = 0;
            Opponent2Points = 0;
            Opponent3Points = 0;
            NeutralPoints = 0;

            for (var y = 0; y < rows.Count(); y++)
            {
                for (var x = 0; x < rows[y].Length; x++)
                {
                    var nodeType = Node.ParseNodeType(rows[y][x]);

                    switch (nodeType)
                    {
                        case NodeType.Player:
                            PlayerPoints++;
                            break;

                        case NodeType.Opponent1:
                            Opponent1Points++;
                            break;

                        case NodeType.Opponent2:
                            Opponent2Points++;
                            break;

                        case NodeType.Opponent3:
                            Opponent3Points++;
                            break;

                        case NodeType.Neutral:
                            NeutralPoints++;
                            break;
                    }

                    Nodes[x, y] = new Node()
                    {
                        Position = new Point(x, y),
                        Type = nodeType
                    };
                }
            }
        }

        public Node[,] Nodes { get; set; }

        public bool IsValidRow(Rectangle row)
        {
            var isValid = false;

            // Vertical
            if (row.Width == 1)
            {

                // validate left
                if (row.X - 1 >= 0)
                {
                    isValid = true;
                    for (var y = row.Y; y < row.Y + row.Height && row.Y < Height && isValid; y++)
                    {
                        if (Nodes[row.X - 1, y].Type != NodeType.Neutral)
                            isValid = false;
                    }
                }

                // validate Right
                if (!isValid && row.X + 1 < Width)
                {
                    isValid = true;
                    for (var y = row.Y; y < row.Y + row.Height && row.Y < Height && isValid; y++)
                    {
                        if (Nodes[row.X + 1, y].Type != NodeType.Neutral)
                            isValid = false;
                    }
                }
            }
            else // Horizontal
            {
                // validate Up
                if (row.Y - 1 >= 0)
                {
                    isValid = true;
                    for (var x = row.X; x < row.X + row.Width && row.X < Width && isValid; x++)
                    {
                        if (Nodes[x, row.Y - 1].Type != NodeType.Neutral)
                            isValid = false;
                    }
                }

                // validate Down
                if (!isValid && row.Y + 1 < Height)
                {
                    isValid = true;
                    for (var x = row.X; x < row.X + row.Width && row.X < Width && isValid; x++)
                    {
                        if (Nodes[x, row.Y + 1].Type != NodeType.Neutral)
                            isValid = false;
                    }
                }
            }

            return isValid;
        }

        public IList<Rectangle> GetPlayerRectangles(int squareLength, Point playerPosition)
        {
            var rectangles = new List<Rectangle>();

            for (var y = 0; y <= Height - squareLength; y++)
            {
                for (var x = 0; x <= Width - squareLength; x++)
                {
                    var rectangle = GetNeutralRectangle(new Point(x, y), squareLength);

                    if (rectangle.IsEmpty)
                        continue;

                    var expandedRectanle = rectangle.Clone().ExpandToTop(Game.ExpansionOffset);
                    if (expandedRectanle.Y >= 0 && IsTopEdgeFilled(expandedRectanle))
                    {
                        rectangles.Add(expandedRectanle);
                        continue;
                    }

                    expandedRectanle = rectangle.Clone().ExpandToBottom(Game.ExpansionOffset);
                    if (expandedRectanle.GetBottomY() < Height && IsBottomEdgeFilled(expandedRectanle))
                    {
                        rectangles.Add(expandedRectanle);
                        continue;
                    }

                    expandedRectanle = rectangle.Clone().ExpandToLeft(Game.ExpansionOffset);
                    if (expandedRectanle.X >= 0 && IsLeftEdgeFilled(expandedRectanle))
                    {
                        rectangles.Add(expandedRectanle);
                        continue;
                    }
                    
                    expandedRectanle = rectangle.Clone().ExpandToRight(Game.ExpansionOffset);
                    if (expandedRectanle.GetRightX() < Width && IsRightEdgeFilled(expandedRectanle))
                    {
                        rectangles.Add(expandedRectanle);
                    }
                }
            }

            return rectangles;
        }

        private Rectangle GetNeutralRectangle(Point point, int squareLength)
        {
            for (var y = 0; y < squareLength; y++)
            {
                for (var x = 0; x < squareLength; x++)
                {
                    if (Nodes[x + point.X, y + point.Y].Type != NodeType.Neutral)
                        return Rectangle.Empty;
                }
            }

            return new Rectangle(point.X, point.Y, squareLength, squareLength);
        }

        public bool IsTopEdgeFilled(Rectangle rectangle)
        {
            return GetTopEdge(rectangle)
                .All(x => x.Type == NodeType.Player);
        }

        public bool IsBottomEdgeFilled(Rectangle rectangle)
        {
            return GetBottomEdge(rectangle)
                .All(x => x.Type == NodeType.Player);
        }

        public bool IsLeftEdgeFilled(Rectangle rectangle)
        {
            return GetLeftEdge(rectangle)
                .All(x => x.Type == NodeType.Player);
        }

        public bool IsRightEdgeFilled(Rectangle rectangle)
        {
            return GetRightEdge(rectangle)
                .All(x => x.Type == NodeType.Player);
        }

        public IEnumerable<Node> GetTopEdge(Rectangle rectangle)
        {
            for (var x = rectangle.X; x <= rectangle.GetRightX(); x++)
            {
                yield return Nodes[x, rectangle.Y];
            }
        }

        public IEnumerable<Node> GetBottomEdge(Rectangle rectangle)
        {
            for (var x = rectangle.X; x <= rectangle.GetRightX(); x++)
            {
                yield return Nodes[x, rectangle.GetBottomY()];
            }
        }

        public IEnumerable<Node> GetRightEdge(Rectangle rectangle)
        {
            for (var y = rectangle.Y; y <= rectangle.GetBottomY(); y++)
            {
                yield return Nodes[rectangle.GetRightX(), y];
            }
        }

        public IEnumerable<Node> GetLeftEdge(Rectangle rectangle)
        {
            for (var y = rectangle.Y; y <= rectangle.GetBottomY(); y++)
            {
                yield return Nodes[rectangle.X, y];
            }
        }

        public Point GetPointSearchingLeft(int squareLength, Point playerPosition)
        {
            // search up
            for (var y = playerPosition.Y; y > playerPosition.Y - squareLength && y >= 0; y--)
            {
                var playerCellCount = 0;
                for (var x = playerPosition.X; x > playerPosition.X - squareLength && x >= 0; x--)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            // search down
            for (var y = playerPosition.Y + 1; y < playerPosition.Y + squareLength && y < Height; y++)
            {
                var playerCellCount = 0;
                for (var x = playerPosition.X; x > playerPosition.X - squareLength && x >= 0; x--)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            return EmptyPoint;
        }

        public Point GetPointSearchingRight(int squareLength, Point playerPosition)
        {
            // search up
            for (var y = playerPosition.Y; y > playerPosition.Y - squareLength && y >= 0; y--)
            {
                var playerCellCount = 0;
                for (var x = playerPosition.X; x < playerPosition.X + squareLength && x < Width; x++)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            // search down
            for (var y = playerPosition.Y + 1; y < playerPosition.Y + squareLength && y < Height; y++)
            {
                var playerCellCount = 0;
                for (var x = playerPosition.X; x < playerPosition.X + squareLength && x < Width; x++)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            return EmptyPoint;
        }

        public Point GetPointSearchingUp(int squareLength, Point playerPosition)
        {
            // search left
            for (var x = playerPosition.X; x > playerPosition.X - squareLength && x >= 0; x--)
            {
                var playerCellCount = 0;
                for (var y = playerPosition.Y; y > playerPosition.Y - squareLength && y >= 0; y--)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            // search Right
            for (var x = playerPosition.X + 1; x < playerPosition.X + squareLength && x < Width; x++)
            {
                var playerCellCount = 0;
                for (var y = playerPosition.Y; y > playerPosition.Y - squareLength && y >= 0; y--)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            return EmptyPoint;
        }

        public Point GetPointSearchingDown(int squareLength, Point playerPosition)
        {
            // search left
            for (var x = playerPosition.X; x > playerPosition.X - squareLength && x >= 0; x--)
            {
                var playerCellCount = 0;
                for (var y = playerPosition.Y; y > playerPosition.Y - squareLength && y < Height; y++)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            // search Right
            for (var x = playerPosition.X + 1; x < playerPosition.X + squareLength && x < Width; x++)
            {
                var playerCellCount = 0;
                for (var y = playerPosition.Y; y > playerPosition.Y - squareLength && y < Height; y++)
                {
                    if (Nodes[x, y].Type == NodeType.Neutral || Nodes[x, y].Type == NodeType.Player)
                        playerCellCount++;
                    else
                    {
                        playerCellCount = 0;
                    }

                    if (playerCellCount < squareLength)
                        continue;

                    return new Point(x, y);
                }
            }

            return EmptyPoint;
        }

        public Rectangle GetRow(Point startPoint, Point endPoint, int squareLength)
        {
            if (startPoint.X == endPoint.X) // vertical
            {
                if (startPoint.Y < endPoint.Y) // start at top
                    return new Rectangle(startPoint.X, startPoint.Y, squareLength, 1);
                else
                    return new Rectangle(endPoint.X, endPoint.Y, 1, squareLength);
            }
            else // horizontal
            {
                if (startPoint.X < endPoint.X) // start at left
                    return new Rectangle(startPoint.X, startPoint.Y, 1, squareLength);
                else
                    return new Rectangle(endPoint.X, endPoint.Y, squareLength, 1);
            }
        }

        public Point GetNextFreePointByOrderBy(Point playerPosition, int squareLength)
        {
            var neutralNodes = GetAllNodesByType(NodeType.Neutral);
            if (!neutralNodes.Any())
                return EmptyPoint;

            if (neutralNodes.Any(x => (int)x.Position.GetDistance(playerPosition) == squareLength))
                return neutralNodes.First(x => (int)x.Position.GetDistance(playerPosition) == squareLength).Position;

            neutralNodes = neutralNodes
                .OrderBy(x => x.Position.GetDistance(playerPosition)).ToList();

            return neutralNodes.First().Position;
        }

        public IList<Node> GetAllNodesByType(NodeType nodeType)
        {
            var nodes = new List<Node>();
            foreach (var node in Nodes)
            {
                if (node.Type == nodeType)
                    nodes.Add(node);
            }

            return nodes;
        }

        public IList<Point> GetBestPathForRectangle(Rectangle rectangle, Point playerPosition, IList<Point> edgeRow)
        {
            var bestPath = new List<Point>();
            var startPoint = MoveEdgePointToRectangle(edgeRow.First(), rectangle);
            var lastPoint = MoveEdgePointToRectangle(edgeRow.Last(), rectangle);
            var rectangleOffset = rectangle.Width - 1;

            bestPath.Add(startPoint);

            if (startPoint.X == lastPoint.X) // vertical row
            {
                if (startPoint.Y == rectangle.Y) // Rectangle in the bottom of startPoint
                {
                    if (startPoint.X == rectangle.X) // Rectangle at right
                    {
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y));
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y + rectangleOffset));
                    }
                    else  // Rectangle at left
                    {
                        bestPath.Add(new Point(rectangle.X, rectangle.Y));
                        bestPath.Add(new Point(rectangle.X, rectangle.Y + rectangleOffset));
                    }
                }
                else if (startPoint.Y == rectangle.Y + rectangleOffset) // Rectangle in the top of startPoint
                {
                    if (startPoint.X == rectangle.X)  // Rectangle at right
                    {
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y + rectangleOffset));
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y));
                    }
                    else  // Rectangle at left
                    {
                        bestPath.Add(new Point(rectangle.X, rectangle.Y + rectangleOffset));
                        bestPath.Add(new Point(rectangle.X, rectangle.Y));
                    }
                }
            }
            else if (startPoint.Y == lastPoint.Y) // horizontal row
            {
                if (startPoint.X == rectangle.X) // Rectangle in the right of startPoint
                {
                    if (startPoint.Y == rectangle.Y) // Rectangle at bottom
                    {
                        bestPath.Add(new Point(rectangle.X, rectangle.Y + rectangleOffset));
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y + rectangleOffset));
                    }
                    else // Rectangle at top
                    {
                        bestPath.Add(new Point(rectangle.X, rectangle.Y));
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y));
                    }
                }
                else if (startPoint.X == rectangle.X + rectangleOffset) // Rectangle in the left of startPoint
                {
                    if (startPoint.Y == rectangle.Y) // Rectangle at bottom
                    {
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y + rectangleOffset));
                        bestPath.Add(new Point(rectangle.X, rectangle.Y + rectangleOffset));

                    }
                    else // Rectangle at top
                    {
                        bestPath.Add(new Point(rectangle.X + rectangleOffset, rectangle.Y));
                        bestPath.Add(new Point(rectangle.X, rectangle.Y));
                    }
                }
            }

            bestPath.Add(lastPoint);
            return bestPath;
        }

        public Point MoveEdgePointToRectangle(Point point, Rectangle rectangle)
        {
            if (point.X >= rectangle.X + rectangle.Width)
                point.X--;
            else if (point.X < rectangle.X)
                point.X++;

            if (point.Y >= rectangle.Y + rectangle.Height)
                point.Y--;
            else if (point.Y < rectangle.Y)
                point.Y++;

            return point;
        }

        public IList<Point> OrderRowByPlayerPosition(IList<Point> row, Point playerPosition)
        {
            var first = row.First();
            var last = row.Last();

            if (first.X == last.X) // vertical
            {
                if (playerPosition.Y <= first.Y || playerPosition.Y >= last.Y)
                    return row.OrderBy(x => x.GetDistance(playerPosition)).ToList();
            }
            else if (first.Y == last.Y) // horizontal
            {
                if (playerPosition.X <= first.X || playerPosition.X >= last.X)
                    return row.OrderBy(x => x.GetDistance(playerPosition)).ToList();
            }

            return row;
        }

        public IList<Node> GetAllNodesByRectangle(Rectangle rectangle)
        {
            var nodes = new List<Node>();

            for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height && y < Height; y++)
            {
                for (var x = rectangle.X; x < rectangle.X + rectangle.Width && x < Width; x++)
                {
                    nodes.Add(Nodes[x, y]);
                }
            }

            return nodes;
        }

        public IList<Node> GetAllEdgeNodesByRectangleAndPlayer(Rectangle rectangle, Point playerPosition)
        {
            IList<Node> left = null;
            IList<Node> right = null;
            IList<Node> top = null;
            IList<Node> bottom = null;

            if (IsLeftEdgeFilled(rectangle))
            {
                left = new List<Node>();
                for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
                    left.Add(Nodes[rectangle.X - 1, y]);
            }

            if (IsRightEdgeFilled(rectangle))
            {
                right = new List<Node>();
                for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
                    right.Add(Nodes[rectangle.X + rectangle.Width, y]);
            }

            if (IsTopEdgeFilled(rectangle))
            {
                top = new List<Node>();
                for (var x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                    top.Add(Nodes[x, rectangle.Y - 1]);
            }

            if (IsBottomEdgeFilled(rectangle))
            {
                bottom = new List<Node>();
                for (var x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
                    bottom.Add(Nodes[x, rectangle.Y + rectangle.Height]);
            }

            IList<Node> selected = null;

            if (left != null)
                selected = left;

            if (right != null)
            {
                if (selected == null)
                    selected = right;
                else
                {
                    if (selected.Sum(x => x.Position.GetDistance(playerPosition)) > right.Sum(x => x.Position.GetDistance(playerPosition)))
                    {
                        selected = right;
                    }
                }
            }

            if (top != null)
            {
                if (selected == null)
                    selected = top;
                else
                {
                    if (selected.Sum(x => x.Position.GetDistance(playerPosition)) > top.Sum(x => x.Position.GetDistance(playerPosition)))
                    {
                        selected = top;
                    }
                }
            }

            if (selected == null)
            {
                selected = bottom;
            }
            else if (bottom != null)
            {
                if (selected.Sum(x => x.Position.GetDistance(playerPosition)) > bottom.Sum(x => x.Position.GetDistance(playerPosition)))
                {
                    selected = bottom;
                }
            }

            var firstPoint = selected.First().Position;
            var lastPoint = selected.Last().Position;
            return playerPosition.GetDistance(firstPoint) > playerPosition.GetDistance(lastPoint) ?
                selected.Reverse().ToList() :
                selected;
        }

        public void ShowBoardStatus()
        {
            Console.Error.WriteLine("Player    has {0:n2} %", (double)PlayerPoints * 100.0 / (double)Nodes.Length);
            Console.Error.WriteLine("Opponent1 has {0:n2} %", (double)Opponent1Points * 100.0 / (double)Nodes.Length);

            if (Opponent2Points > 0)
                Console.Error.WriteLine("Opponent2 has {0:n2} %", (double)Opponent2Points * 100.0 / (double)Nodes.Length);

            if (Opponent3Points > 0)
                Console.Error.WriteLine("Opponent3 has {0:n2} %", (double)Opponent3Points * 100.0 / (double)Nodes.Length);
        }

        public IEnumerable<Node> GetEdgeNodes(Rectangle rectangle)
        {
            var nodes = new Collection<Node>();

            // horizontal
            for (var x = 0; x < rectangle.Width; x++)
            {
                nodes.Add(Nodes[x + rectangle.X, rectangle.Y]);
                if (rectangle.Height > 1)
                    nodes.Add(Nodes[x + rectangle.X, rectangle.Y + (rectangle.Height - 1)]);
            }

            // vertical
            for (var y = 1; y < rectangle.Height - 1; y++)
            {
                nodes.Add(Nodes[rectangle.X, y + rectangle.Y]);
                if (rectangle.Width > 1)
                    nodes.Add(Nodes[rectangle.X + (rectangle.Width - 1), y + rectangle.Y]);
            }

            return nodes;
        }

        public IEnumerable<Node> GetEdgeAndNeutralNodes(Rectangle rectangle)
        {
            return GetEdgeNodes(rectangle)
                .Where(x => x.Type == NodeType.Neutral);
        }

        public bool IsValidRectangle(Rectangle rectangle)
        {
            return !rectangle.IsOutOfBounds() && GetAllNodesByRectangle(rectangle).All(x => x.Type == NodeType.Neutral || x.Type == NodeType.Player);
        }

        public bool IsEmptyColumn(int x)
        {
            var nodes = GetAllNodesByRectangle(new Rectangle(x, 0, 1, Height));
            return nodes.All(node => node.Type == NodeType.Neutral);
        }

        public bool IsEmptyRow(int y)
        {
            var nodes = GetAllNodesByRectangle(new Rectangle(0, y, Width, 1));
            return nodes.All(node => node.Type == NodeType.Neutral);
        }
    }

    public class Player : Character
    {
        public Player(string input, Direction direction = Direction.None, bool usedBackInTime = false, IList<Point> actionHistory = null)
            : base(input, direction, usedBackInTime, actionHistory)
        { }

        public Player(string input)
        {
            Update(input);
        }

        public void RemoveActionsUntilGameRound(int gameRound)
        {
            while (ActionsHistory.Count > gameRound)
            {
                ActionsHistory.Remove(ActionsHistory.Last());
            }
        }
    }

    public abstract class Character
    {
        public Point Position { get; protected set; }
        public bool UsedBackInTime { get; protected set; }
        public Direction Direction { get; protected set; }
        public IList<Point> ActionsHistory { get; protected set; }

        protected Character(string input, Direction direction, bool usedBackInTime, IList<Point> actionHistory)
            : this()
        {
            if (actionHistory != null)
                ActionsHistory = actionHistory;
            Update(input);
            Direction = direction;
            UsedBackInTime = usedBackInTime;
        }

        protected Character()
        {
            Direction = Direction.None;
            Position = Board.EmptyPoint;
            ActionsHistory = new List<Point>();
        }

        public void Update(string input)
        {
            var inputs = input.Split(' ');
            var nextPosition = new Point(Convert.ToInt32(inputs[0]), Convert.ToInt32(inputs[1]));

            if (!Position.IsRealEmpty())
            {
                Direction = Position.GetDirection(nextPosition);
            }

            Position = nextPosition;
            ActionsHistory.Add(Position);

            UsedBackInTime = inputs[2] == "0";
        }

        public override string ToString()
        {
            return string.Format(
                "Character at ({0}, {1}) going {2} BackInTime {3}",
                Position.X,
                Position.Y,
                Direction,
                UsedBackInTime);
        }

        public int GetRoundsToHit(Rectangle rectangle)
        {
            return Position.GetRoundsToGoToPoint(rectangle.GetClosestPointToHit(Position));
        }
    }

    public class Opponent : Character
    {
        public int Number { get; protected set; }

        public Opponent(string input, Direction direction = Direction.None, bool usedBackInTime = false, IList<Point> actionHistory = null)
            : base(input, direction, usedBackInTime, actionHistory)
        { }

        public Opponent(int number, string input)
            : base()
        {
            Number = number;
            Update(input);
        }

        public override string ToString()
        {
            return string.Format(
                "Opponent {0} at ({1}, {2}) going {3} BackInTime {4}",
                Number,
                Position.X,
                Position.Y,
                Direction,
                UsedBackInTime);
        }
    }

    public enum Direction
    {
        None,
        Left,
        Right,
        Up,
        Down,
    }

    public class Node
    {
        public Point Position { get; set; }
        public NodeType Type { get; set; }

        public override string ToString()
        {
            return string.Format(
                "({0}, {1}) {2}",
                Position.X,
                Position.Y,
                Type
            );
        }

        public static NodeType ParseNodeType(char input)
        {
            switch (input)
            {
                case '.':
                    return NodeType.Neutral;
                case '0':
                    return NodeType.Player;
                case '1':
                    return NodeType.Opponent1;
                case '2':
                    return NodeType.Opponent2;
                case '3':
                    return NodeType.Opponent3;
                default:
                    throw new Exception("Invalid NodeType.");
            }
        }
    }

    public enum NodeType
    {
        Neutral,
        Player,
        Opponent1,
        Opponent2,
        Opponent3
    }

    public static class PointExtensions
    {
        public static string ToAction(this Point point)
        {
            return string.Format("{0} {1}", point.X, point.Y);
        }

        public static double GetDistance(this Point point, Point targetPoint)
        {
            return Math.Sqrt(Math.Pow(targetPoint.X - point.X, 2) + Math.Pow(targetPoint.Y - point.Y, 2));
        }

        public static int GetRoundsToGoToPoint(this Point currentPoint, Point targetPoint)
        {
            return Math.Abs(currentPoint.X - targetPoint.X) + Math.Abs(currentPoint.Y - targetPoint.Y);
        }

        public static bool IsRealEmpty(this Point point)
        {
            return point.X < 0 || point.Y < 0;
        }

        public static Direction GetDirection(this Point currentPosition, Point nextPosition)
        {
            if (currentPosition.X > nextPosition.X)
                return Direction.Left;

            if (currentPosition.X < nextPosition.X)
                return Direction.Right;

            if (currentPosition.Y < nextPosition.Y)
                return Direction.Down;

            if (currentPosition.Y > nextPosition.Y)
                return Direction.Up;

            return Direction.None;
        }
    }

    public static class RectangleExtensions
    {
        public static double GetDistance(this Rectangle rectangle, Point playerPosition)
        {
            return rectangle
                .GetCenter()
                .GetDistance(playerPosition);
        }

        public static Point GetCenter(this Rectangle rectangle)
        {
            var x = Math.Floor(rectangle.X + rectangle.Width / 2.0);
            var y = Math.Floor(rectangle.Y + rectangle.Height / 2.0);
            return new Point((int)x, (int)y);
        }

        public static bool IsCorner(this Rectangle rectangle, Point point)
        {
            return (point.X == rectangle.X || point.X == rectangle.X + (rectangle.Width - 1)) &&
                   (point.Y == rectangle.Y || point.Y == rectangle.Y + (rectangle.Height - 1));
        }

        public static bool IsHorizontal(this Rectangle rectangle)
        {
            return rectangle.Width > rectangle.Height;
        }

        public static bool IsVertical(this Rectangle rectangle)
        {
            return rectangle.Width < rectangle.Height;
        }

        public static int GetBottomY(this Rectangle rectangle)
        {
            return rectangle.Y + rectangle.Height - 1;
        }

        public static int GetRightX(this Rectangle rectangle)
        {
            return rectangle.X + rectangle.Width - 1;
        }

        public static bool IsInHorizontalArea(this Rectangle rectangle, int targetY)
        {
            return rectangle.Y <= targetY && rectangle.GetBottomY() >= targetY;
        }

        public static bool IsInVerticalArea(this Rectangle rectangle, int targetX)
        {
            return rectangle.X <= targetX && rectangle.GetRightX() >= targetX;
        }

        public static bool WillCharacterHit(this Rectangle rectangle, Character character)
        {
            switch (character.Direction)
            {
                case Direction.Left:
                    return
                        rectangle.GetRightX() < character.Position.X &&
                        rectangle.IsInHorizontalArea(character.Position.Y);

                case Direction.Right:
                    return
                        rectangle.X > character.Position.X &&
                        rectangle.IsInHorizontalArea(character.Position.Y);

                case Direction.Up:
                    return
                        rectangle.GetBottomY() > character.Position.Y &&
                        rectangle.IsInVerticalArea(character.Position.X);

                case Direction.Down:
                    return
                        rectangle.Y > character.Position.Y &&
                        rectangle.IsInVerticalArea(character.Position.X);
                default:
                    return false;
            }
        }

        public static Point GetClosestPointToHit(this Rectangle rectangle, Point targetPoint)
        {
            if (rectangle.IsInHorizontalArea(targetPoint.Y))
            {
                if (rectangle.X > targetPoint.X)
                    return new Point(rectangle.X, targetPoint.Y);
                
                if (rectangle.GetRightX() < targetPoint.X)
                    return new Point(rectangle.GetRightX(), targetPoint.Y);
            }
            else if (rectangle.IsInVerticalArea(targetPoint.X))
            {
                if (rectangle.Y > targetPoint.Y)
                    return new Point(targetPoint.X, rectangle.Y);

                if (rectangle.GetBottomY() < targetPoint.Y)
                    return new Point(targetPoint.X, rectangle.GetBottomY());
            }
            else // get corner points
            {
                if (rectangle.X > targetPoint.X && rectangle.Y > targetPoint.Y) // top left
                    return new Point(rectangle.X, rectangle.Y);
                
                if (rectangle.GetRightX() < targetPoint.X && rectangle.Y > targetPoint.Y) // top right
                    return new Point(rectangle.GetRightX(), rectangle.Y);

                if (rectangle.GetRightX() < targetPoint.X && rectangle.GetBottomY() < targetPoint.Y) // Bottom right
                    return new Point(rectangle.GetRightX(), rectangle.GetBottomY());

                if (rectangle.X > targetPoint.X && rectangle.GetBottomY() < targetPoint.Y) // Bottom left
                    return new Point(rectangle.X, rectangle.GetBottomY());
            }

            return Board.EmptyPoint;
        }

        public static bool IsOutOfBounds(this Rectangle rectangle)
        {
            return 
                rectangle.X < 0 || rectangle.X >= Board.Width || rectangle.GetRightX() >= Board.Width ||
                rectangle.Y < 0 || rectangle.Y >= Board.Height || rectangle.GetBottomY() >= Board.Height;
        }

        public static Rectangle Clone(this Rectangle rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Rectangle ExpandToRight(this Rectangle rectangle, int offset)
        {
            rectangle.Width += offset;
            return rectangle;
        }

        public static Rectangle ExpandToLeft(this Rectangle rectangle, int offset)
        {
            rectangle.Width += offset;
            rectangle.X -= offset;
            return rectangle;
        }

        public static Rectangle ExpandToTop(this Rectangle rectangle, int offset)
        {
            rectangle.Y -= offset;
            rectangle.Height += offset;
            return rectangle;
        }

        public static Rectangle ExpandToBottom(this Rectangle rectangle, int offset)
        {
            rectangle.Height += offset;
            return rectangle;
        }
    }
}