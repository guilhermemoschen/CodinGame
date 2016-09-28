using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CodinGame.BotProgramming.HyperSonic
{
    public class Game
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]);
            int height = int.Parse(inputs[1]);
            int myId = int.Parse(inputs[2]);

            var board = new Board(width, height, myId);
            var bombService = new BombService(board);
            var movimentService = new ActionService(board);

            Action currentAction = null;

            // game loop
            while (true)
            {
                var rows = new Collection<string>();

                for (int i = 0; i < height; i++)
                {
                    rows.Add(Console.ReadLine());
                }

                foreach (var row in rows)
                    Console.Error.WriteLine(row);

                var entities = new List<string[]>();
                int entitiesCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < entitiesCount; i++)
                {
                    entities.Add(Console.ReadLine().Split(' '));
                }

                board.Update(rows, entities);

                var possibility = bombService.CalculateBestOptionToPlaceBomb();

                if (currentAction == null || currentAction.IsFinished)
                {
                    currentAction = new PlaceBomb(board, possibility.BombNode.Position);
                }

                currentAction.Process();

                Console.WriteLine(currentAction.GetAction());
            }
        }
    }

    public class PlaceBomb : Action
    {
        private bool isBomPlaced = false;

        public Point TargetPosition { get; set; }

        public PlaceBomb(Board board, Point targetPosition) : base(board)
        {
            TargetPosition = targetPosition;
        }

        public override void Process()
        {
            if (isBomPlaced)
            {
                IsFinished = true;
                return;
            }

            if (Board.Player.Position == TargetPosition && Board.Player.AvailableBombs > 0)
            {
                NextAction = string.Format("BOMB {0} {1}", TargetPosition.X, TargetPosition.Y);
                isBomPlaced = true;
                return;
            }

            NextAction = string.Format("MOVE {0} {1}", TargetPosition.X, TargetPosition.Y);
        }
    }

    public abstract class Action
    {
        public Board Board { get; set; }

        protected string NextAction;

        public bool IsFinished { get; protected set; }

        protected Action(Board board)
        {
            Board = board;
            IsFinished = false;
        }

        public abstract void Process();

        public string GetAction()
        {
            return NextAction;
        }
    }


    public class ActionService
    {
        public Board Board { get; set; }

        public ActionService(Board board)
        {
            Board = board;
        }

        public string TakeAction()
        {
            return null;
        }
    }

    public class BombService
    {
        public ExplosionPossiblity[,] ExplosionPossibilities { get; set; }

        public Board Board { get; set; }

        public BombService(Board board)
        {
            Board = board;
        }

        public ExplosionPossiblity CalculateBestOptionToPlaceBomb()
        {
            ExplosionPossibilities = new ExplosionPossiblity[Board.Width, Board.Height];

            foreach (var node in Board.Nodes.Cast<Node>().Where(x => x.CanPlaceBomb))
            {
                var affectedBoxes = Board.GetBoxesInRange(node, Board.Player.ExplosionRange);
                ExplosionPossibilities[node.Position.X, node.Position.Y] = new ExplosionPossiblity()
                {
                    BombNode = node,
                    Player = Board.Player,
                    AffectedBoxes = affectedBoxes,
                };
            }

            var possibleExplosions = ExplosionPossibilities.Cast<ExplosionPossiblity>()
                .Where(x => x != null)
                .OrderByDescending(x => x.Score)
                .ToList();

            foreach (var p in possibleExplosions.Take(5))
            {
                //Console.Error.WriteLine("{0} - {1}", p.BombNode.Position, p.Score);
            }

            return possibleExplosions.FirstOrDefault();
        }
    }

    public class ExplosionPossiblity
    {

        public IEnumerable<Node> AffectedBoxes { get; set; }
        public Node BombNode { get; set; }
        public BomberMan Player { get; set; }

        public ExplosionPossiblity()
        {
            AffectedBoxes = new Collection<Node>();
        }

        public int Score
        {
            get
            {
                return (AffectedBoxes.Count() * 5) - Player.Position.GetDistance(BombNode.Position);
            }
        }
    }

    public class BomberMan : Entity
    {
        public int AvailableBombs { get; set; }
        public int ExplosionRange { get; set; }

        public BomberMan(string[] inputs)
        {
            Id = int.Parse(inputs[1]);
            Position = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));
            AvailableBombs = Convert.ToInt32(inputs[4]);
            ExplosionRange = Convert.ToInt32(inputs[5]);
        }
    }

    public class Bomb : Entity
    {
        public BomberMan Owner { get; set; }
        public int RoundsToExplode { get; set; }
        public int ExplosionRange { get; set; }

        public Bomb(string[] inputs)
        {
            Id = int.Parse(inputs[1]);
            Position = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));
            RoundsToExplode = Convert.ToInt32(inputs[4]);
            ExplosionRange = Convert.ToInt32(inputs[5]);
        }
    }

    public class Entity
    {
        public const string BomberManEntityType = "0";
        public const string BombEntityType = "1";

        public Point Position { get; set; }
        public int Id { get; set; }
    }

    public class Board
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int PlayerId { get; set; }
        public Node[,] Nodes { get; set; }
        public int MaxNodeDistance { get; set; }

        public BomberMan Player { get; set; }

        public IList<BomberMan> BomberMans { get; set; }
        public IList<Bomb> Bombs { get; set; }

        public Board(int width, int height, int playerId)
        {
            Width = width;
            Height = height;
            PlayerId = playerId;
            Nodes = new Node[Width, Height];
            BomberMans = new List<BomberMan>();
            Bombs = new List<Bomb>();
            MaxNodeDistance = Height * Width;
        }

        public void Update(IEnumerable<string> rows, IList<string[]> entities)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var row = rows
                        .Skip(y)
                        .First();

                    Nodes[x, y] = new Node(row.Skip(x).First().ToString(), x, y);

                }
            }

            BomberMans.Clear();
            Bombs.Clear();

            foreach (var entity in entities)
            {
                switch (entity[0])
                {
                    case Entity.BomberManEntityType:
                        AddBomberMan(new BomberMan(entity));
                        break;

                    case Entity.BombEntityType:
                        AddBomb(new Bomb(entity));
                        break;
                }
            }

            Player = BomberMans.First(x => x.Id == PlayerId);
            BomberMans.Remove(Player);
        }

        public void AddBomberMan(BomberMan bomberMan)
        {
            BomberMans.Add(bomberMan);
            Nodes[bomberMan.Position.X, bomberMan.Position.Y] = new Node()
            {
                Position = bomberMan.Position,
                Type = Node.ParseCellType((bomberMan.Id + 1).ToString()),
            };
        }

        public void AddBomb(Bomb bomb)
        {
            Bombs.Add(bomb);
            Nodes[bomb.Position.X, bomb.Position.Y] = new Node()
            {
                Position = bomb.Position,
                Type = NodeType.Bomb,
            };
        }

        public void PrintBoard()
        {
            Console.Error.WriteLine(string.Empty);
            for (var y = 0; y < Height; y++)
            {
                var row = string.Empty;
                for (var x = 0; x < Width; x++)
                {
                    switch (Nodes[x, y].Type)
                    {
                        case NodeType.Bomb:
                            row += "B";
                            break;

                        case NodeType.Box:
                            row += "X";
                            break;

                        case NodeType.Floor:
                            row += ".";
                            break;

                        case NodeType.Player1:
                            row += "1";
                            break;

                        case NodeType.Player2:
                            row += "2";
                            break;

                        case NodeType.Player3:
                            row += "3";
                            break;

                        case NodeType.Player4:
                            row += "4";
                            break;

                        case NodeType.Undefined:
                            row += "U";
                            break;
                    }
                }

                Console.Error.WriteLine(row);
            }
        }

        public bool IsBox(Point position)
        {
            return IsBox(Nodes[position.X, position.Y]);
        }

        public bool IsBox(Node node)
        {
            return node.Type == NodeType.Box;
        }

        public bool IsFloor(Point position)
        {
            return IsFloor(Nodes[position.X, position.Y]);
        }

        public bool IsFloor(Node node)
        {
            return node.Type == NodeType.Floor;
        }

        public bool IsOutOfBounds(Point position)
        {
            return position.Y < 0 || position.Y >= Height || position.X < 0 || position.X >= Width;
        }

        internal Node GetNode(Point currentPosition)
        {
            return Nodes[currentPosition.X, currentPosition.Y];
        }

        public IEnumerable<Node> GetBoxesInRange(Node node, int explosionRange)
        {
            var boxes = new Collection<Node>();

            // Test Up
            for (var i = 0; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(0, -i);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsBox(currentPosition))
                    boxes.Add(GetNode(currentPosition));

                break;
            }

            // Test Down
            for (var i = 0; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(0, i);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsBox(currentPosition))
                    boxes.Add(GetNode(currentPosition));

                break;
            }

            // Test Left
            for (var i = 0; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(-i, 0);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsBox(currentPosition))
                    boxes.Add(GetNode(currentPosition));

                break;
            }

            // Test Right
            for (var i = 0; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(-i, 0);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsBox(currentPosition))
                    boxes.Add(GetNode(currentPosition));

                break;
            }


            //Console.Error.WriteLine("Boxes for Node {0} Range {1}", node.Position, explosionRange);
            //foreach (var box in boxes)
            //{
            //    Console.Error.WriteLine("Box {0}", box.Position);
            //}

            return boxes;
        }

        public void ResetDistances()
        {
            foreach (var node in Nodes)
            {
                node.DistanceToPlayer = MaxNodeDistance;
            }
        }

        public void UpdateDistances(Node startingNode)
        {
            ResetDistances();

            startingNode.DistanceToPlayer = 0;

            while (true)
            {
                var madeProgress = false;

                foreach (var node in Nodes)
                {
                    // look through valid moves given the coordinates of that square.
                    var nodesToWalk = GetValidNodesToWalk(node);
                    Console.Error.WriteLine("nodesToWalk {0}", nodesToWalk.Count());
                    foreach (var nodeToWalk in nodesToWalk)
                    {


                        var newPass = node.DistanceToPlayer + 1;
                        if (nodeToWalk.DistanceToPlayer > newPass)
                        {
                            nodeToWalk.DistanceToPlayer = newPass;
                            // Console.Error.WriteLine("{0} - {1}", nodeToWalk.Position, nodeToWalk.DistanceToPlayer);
                            madeProgress = true;
                        }
                    }
                }

                Console.Error.WriteLine("madeProgress");
                if (!madeProgress)
                    break;
            }
        }

        public IEnumerable<Node> GetValidNodesToWalk(Node node)
        {
            var moves = new List<Node>();

            // can go right
            var nextPosition = node.Position.Clone();
            nextPosition.Offset(1, 0);
            if (nextPosition.X < Width - 1 && IsFloor(nextPosition))
            {
                moves.Add(GetNode(nextPosition));
            }

            // can go left
            nextPosition = node.Position.Clone();
            nextPosition.Offset(-1, 0);
            if (nextPosition.X >= 0 && IsFloor(nextPosition))
            {
                moves.Add(GetNode(nextPosition));
            }

            // can go Up
            nextPosition = node.Position.Clone();
            nextPosition.Offset(0, -1);
            if (nextPosition.Y >= 0 && IsFloor(nextPosition))
            {
                moves.Add(GetNode(nextPosition));
            }

            // can go Down
            nextPosition = node.Position.Clone();
            nextPosition.Offset(0, 1);
            if (nextPosition.Y < Height - 1 && IsFloor(nextPosition))
            {
                moves.Add(GetNode(nextPosition));
            }

            return moves;
        }

        public bool CanFindShortestPath(Point start, Point end)
        {
            var startingPoint = GetNode(start);
            var endingPoint = GetNode(end);

            Console.Error.WriteLine("Start {0} End {1}", start, end);

            if (start == end)
                return true;

            UpdateDistances(startingPoint);
            var currentPoint = endingPoint;

            while (true)
            {
                // Look through each direction and find the square
                // with the lowest number of steps marked.
                Node lowestNode = null;
                var lowestDistance = MaxNodeDistance;

                foreach (var nodeToWalk in GetValidNodesToWalk(currentPoint))
                {
                    if (nodeToWalk.DistanceToPlayer < lowestDistance)
                    {
                        lowestDistance = nodeToWalk.DistanceToPlayer;
                        lowestNode = nodeToWalk;
                    }
                }

                if (lowestNode == null || lowestNode.DistanceToPlayer == MaxNodeDistance)
                    break;

                currentPoint = lowestNode;

                if (currentPoint.Position == startingPoint.Position)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class Node
    {
        public const string FloorCell = ".";
        public const string BoxCell = "0";

        public Point Position { get; set; }
        public NodeType Type { get; set; }

        public bool CanPlaceBomb
        {
            get
            {
                return Type == NodeType.Floor || Type == NodeType.Player1 || Type == NodeType.Player2 || Type == NodeType.Player3 || Type == NodeType.Player4;
            }
        }

        public int DistanceToPlayer { get; set; }

        public Node()
        {

        }

        public Node(string cellType, int x, int y)
        {
            Type = ParseCellType(cellType);
            Position = new Point(x, y);
        }

        public static NodeType ParseCellType(string cellType)
        {
            switch (cellType)
            {
                case FloorCell:
                    return NodeType.Floor;

                case BoxCell:
                    return NodeType.Box;

                case "1":
                    return NodeType.Player1;

                case "2":
                    return NodeType.Player2;

                case "3":
                    return NodeType.Player3;

                case "4":
                    return NodeType.Player4;

                default:
                    return NodeType.Undefined;
            }
        }
    }

    public enum NodeType
    {
        Floor,
        Bomb,
        Box,
        Player1,
        Player2,
        Player3,
        Player4,
        Undefined,
    }

    public static class PointExtensions
    {
        public static Point Clone(this Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static string ToAction(this Point point)
        {
            return string.Format("{0} {1}", point.X, point.Y);
        }

        public static int GetDistance(this Point point, Point targetPoint)
        {
            return Math.Abs(point.X - targetPoint.X) + Math.Abs(point.Y - targetPoint.Y);
        }

        public static int GetRoundsToGoToPoint(this Point currentPoint, Point targetPoint)
        {
            return Math.Abs(currentPoint.X - targetPoint.X) + Math.Abs(currentPoint.Y - targetPoint.Y);
        }

        public static bool IsRealEmpty(this Point point)
        {
            return point.X < 0 || point.Y < 0;
        }
    }
}
