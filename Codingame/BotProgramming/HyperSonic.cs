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
        public static Board Board;

        static void Main(string[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var width = Convert.ToInt32(inputs[0]);
            var height = Convert.ToInt32(inputs[1]);
            var playerId = Convert.ToInt32(inputs[2]);
            Board = new Board(width, height, playerId);
            var actionManager = new ActionManager();

            // game loop
            while (true)
            {
                var rows = new Collection<string>();
                for (var i = 0; i < height; i++)
                {
                    rows.Add(Console.ReadLine());
                }

                //foreach (var row in rows)
                //    Console.Error.WriteLine(row);

                var entities = new List<string[]>();
                var entitiesCount = Convert.ToInt32(Console.ReadLine());
                for (var i = 0; i < entitiesCount; i++)
                {
                    entities.Add(Console.ReadLine().Split(' '));
                }

                Board.Update(rows, entities);

                actionManager.Process();

                Console.WriteLine(actionManager.GetNextAction());
            }
        }
    }

    public class ActionManager
    {
        public Action CurrentAction { get; set; }
        public BombService BombService { get; set; }
        public DodgeService DodgeService { get; set; }

        public ActionManager()
        {
            BombService = new BombService();
            DodgeService = new DodgeService();
        }

        public void Process()
        {
            BombService.CalculateBestOptionToPlaceBomb();

            if (Game.Board.Player.WillExplode)
            {
                Console.Error.WriteLine("Player In Danger");
                var safePosition = DodgeService.GetClosestSafePosition();
                if (safePosition != null)
                    CurrentAction = new MoveAction(safePosition.Value);
            }
            else if (Game.Board.Player.HasBombs)
            {
                CurrentAction = BombService.GetPlaceBomb();
            }
            else if (BombService.ExplosionPossibilities.Any())
            {
                CurrentAction = new MoveAction(BombService.ExplosionPossibilities.First().BombNode.Position);
            }

            if (CurrentAction == null)
            {
                CurrentAction = new WaitAction();
            }

            CurrentAction.Process();
        }

        public string GetNextAction()
        {
            return CurrentAction.GetAction();
        }
    }

    public class DodgeService
    {
        public Point? GetClosestSafePosition()
        {
            var safeNode = Game.Board.GetReachableNodes()
                .Where(x => !x.WillExplode)
                .OrderBy(x => x.DistanceOfPlayer)
                .FirstOrDefault();

            if (safeNode == null)
                return null;

            return safeNode.Position;
        }
    }

    public class PlaceBombAction : Action
    {
        public Point TargetPosition { get; set; }
        public Point? NextTargetPosition { get; set; }

        public PlaceBombAction(Point targetPosition, Point? nextTargetPosition)
        {
            TargetPosition = targetPosition;
            NextTargetPosition = nextTargetPosition;
        }

        public override void Process()
        {
            if (Game.Board.Player.Position == TargetPosition && Game.Board.Player.AvailableBombs > 0)
            {
                if (NextTargetPosition != null)
                    NextAction = string.Format("BOMB {0} {1}", NextTargetPosition.Value.X, NextTargetPosition.Value.Y);
                else
                    NextAction = string.Format("BOMB {0} {1}", TargetPosition.X, TargetPosition.Y);
            }
            else
            {
                NextAction = string.Format("MOVE {0} {1}", TargetPosition.X, TargetPosition.Y);
            }
        }
    }

    public class MoveAction : Action
    {
        public Point TargetPosition { get; set; }

        public MoveAction(Point targetPosition)
        {
            TargetPosition = targetPosition;
        }

        public override void Process()
        {
            NextAction = string.Format("MOVE {0} {1}", TargetPosition.X, TargetPosition.Y);
        }
    }

    public class WaitAction : Action
    {
        public override void Process()
        {
            NextAction = string.Format("MOVE {0} {1} Waiting", Game.Board.Player.Position.X, Game.Board.Player.Position.Y);
        }
    }

    public abstract class Action
    {
        protected string NextAction;

        public bool IsFinished { get; protected set; }

        protected Action()
        {
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

    public class MovementService
    {
        
    }

    public class BombService
    {
        public IList<ExplosionPossiblity> ExplosionPossibilities { get; set; }

        public BombService()
        {
            ExplosionPossibilities = new List<ExplosionPossiblity>();
        }

        public void CalculateBestOptionToPlaceBomb()
        {
            ExplosionPossibilities.Clear();

            foreach (var node in Game.Board.GetReachableNodes().Where(x => !x.WillExplode))
            {
                var affectedBoxes = Game.Board.GetBoxesInRange(node, Game.Board.Player.ExplosionRange);

                ExplosionPossibilities.Add(new ExplosionPossiblity()
                {
                    BombNode = node,
                    Player = Game.Board.Player,
                    AffectedBoxes = affectedBoxes.Where(x => !x.WillExplode),
                });
            }

            ExplosionPossibilities = ExplosionPossibilities
                .Where(x => x.AffectedBoxes.Any())
                .OrderByDescending(x => x.Score)
                .ToList();

            foreach (var p in ExplosionPossibilities.Take(5))
            {
                Console.Error.WriteLine("Bomb {0} Score {1} Boxes {2} Distance {3}", p.BombNode.Position, p.Score, p.AffectedBoxes.Count(), p.BombNode.DistanceOfPlayer);
            }
        }

        public Action GetPlaceBomb()
        {
            if (!ExplosionPossibilities.Any())
                return null;

            Point? nextTargetPosition = null;
            if (ExplosionPossibilities.Count() > 1)
            {
                nextTargetPosition = ExplosionPossibilities.Skip(1).First().BombNode.Position;
            }

            return new PlaceBombAction(ExplosionPossibilities.First().BombNode.Position, nextTargetPosition);
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
                return (AffectedBoxes.Count() * 4) - BombNode.DistanceOfPlayer;
            }
        }
    }

    public class BomberMan : Entity
    {
        public int AvailableBombs { get; set; }
        public int ExplosionRange { get; set; }

        public bool HasBombs
        {
            get { return AvailableBombs > 0; }
        }

        public BomberMan(string[] inputs)
        {
            Id = int.Parse(inputs[1]);
            Position = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));
            AvailableBombs = Convert.ToInt32(inputs[4]);
            ExplosionRange = Convert.ToInt32(inputs[5]);
            Type = NodeType.Bomberman;
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
            Type = NodeType.Bomb;
        }
    }

    public class Item : Entity
    {
        public ItemType ItemType { get; set; }
        public Item(string[] inputs)
        {
            Id = int.Parse(inputs[1]);
            Position = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));
            ItemType = ParseItemType(inputs[3]);
            Type = NodeType.Item;
        }

        public ItemType ParseItemType(string itemType)
        {
            switch (itemType)
            {
                case Node.ItemExtraRangeCell:
                    return ItemType.ExtraRange;

                case Node.ItemExtraBombCell:
                    return ItemType.ExtraBomb;

                default:
                    return ItemType.Undefined;
            }
        }
    }

    public enum ItemType
    {
        ExtraRange,
        ExtraBomb,
        Undefined,
    }

    public class Entity : Node
    {
        public const string BomberManEntityType = "0";
        public const string BombEntityType = "1";
        public const string ItemEntityType = "2";

        public int Id { get; set; }
    }

    public class Node
    {
        public const string FloorCell = ".";
        public const string WallCell = "X";
        public const string EmptyCell = "0";
        public const string ItemExtraRangeCell = "1";
        public const string ItemExtraBombCell = "2";
        public bool WillExplode { get; set; }

        public Point Position { get; set; }
        public NodeType Type { get; set; }

        public int DistanceOfPlayer { get; set; }

        public Node()
        {
            DistanceOfPlayer = -1;
        }

        public bool CanPlaceBomb
        {
            get
            {
                return Type == NodeType.Floor || Game.Board.IsPlayer(this);
            }
        }
    }

    public class Box : Node
    {
        
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
        public IList<Item> Items { get; set; }

        public Board(int width, int height, int playerId)
        {
            Width = width;
            Height = height;
            PlayerId = playerId;
            Nodes = new Node[Width, Height];
            BomberMans = new List<BomberMan>();
            Bombs = new List<Bomb>();
            Items = new List<Item>();
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

                    var nodeType = ParseCellType(row.Skip(x).First().ToString());

                    if (nodeType == NodeType.Box)
                    {
                        Nodes[x, y] = new Box()
                        {
                            Position = new Point(x, y),
                            Type = nodeType,
                        };
                    }
                    else
                    {
                        Nodes[x, y] = new Node()
                        {
                            Position = new Point(x, y),
                            Type = nodeType,
                        };
                    }
                }
            }

            BomberMans.Clear();
            Bombs.Clear();
            Items.Clear();

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

                    case Entity.ItemEntityType:
                        AddItem(new Item(entity));
                        break;
                }
            }

            Player = BomberMans.First(x => x.Id == PlayerId);
            BomberMans.Remove(Player);
        }

        public void AddBomberMan(BomberMan bomberMan)
        {
            BomberMans.Add(bomberMan);
            Nodes[bomberMan.Position.X, bomberMan.Position.Y] = bomberMan;
        }

        public void AddBomb(Bomb bomb)
        {
            Bombs.Add(bomb);
            Nodes[bomb.Position.X, bomb.Position.Y] = bomb;

            UpdateBoxes(bomb);
            UpdateFloors(bomb);
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
            Nodes[item.Position.X, item.Position.Y] = item;
        }

        public void UpdateFloors(Bomb bomb)
        {
            // Test Up
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(0, -i);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (!IsFloor(currentPosition) && !IsBomberMan(currentPosition))
                    break;

                var floor = GetNode(currentPosition);
                floor.WillExplode = true;
            }

            // Test Down
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(0, i);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (!IsFloor(currentPosition) && !IsBomberMan(currentPosition))
                    break;

                var floor = GetNode(currentPosition);
                floor.WillExplode = true;
            }

            // Test Left
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(-i, 0);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (!IsFloor(currentPosition) && !IsBomberMan(currentPosition))
                    break;

                var floor = GetNode(currentPosition);
                floor.WillExplode = true;
            }

            // Test Right
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(i, 0);

                if (IsOutOfBounds(currentPosition))
                    break;

                if (!IsFloor(currentPosition) && !IsBomberMan(currentPosition))
                    break;

                var floor = GetNode(currentPosition);
                floor.WillExplode = true;
            }
        }

        public void UpdateBoxes(Bomb bomb)
        {
            foreach (var box in GetBoxesInRange(bomb, bomb.ExplosionRange))
            {
                box.WillExplode = true;
                //Console.Error.WriteLine("Box {0} Will explode", box.Position);
            }
        }

        public IEnumerable<Node> GetReachableNodes()
        {
            var reachableNodes = new List<Node>();
            var currentNode = (Node) Player;
            currentNode.DistanceOfPlayer = 0;
            reachableNodes.Add(currentNode);
            while (true)
            {
                var nextNode = GetAboveNodeForPlayer(currentNode.Position);

                if (nextNode == null || reachableNodes.Any(x => x.Position == nextNode.Position))
                    nextNode = GetBelowNodeForPlayer(currentNode.Position);

                if (nextNode == null || reachableNodes.Any(x => x.Position == nextNode.Position))
                    nextNode = GetLeftNodeForPlayer(currentNode.Position);

                if (nextNode == null || reachableNodes.Any(x => x.Position == nextNode.Position))
                    nextNode = GetRightNodeForPlayer(currentNode.Position);

                if (nextNode == null || reachableNodes.Any(x => x.Position == nextNode.Position))
                {
                    if (reachableNodes.IndexOf(currentNode) == 0)
                        break;

                    currentNode = reachableNodes[reachableNodes.IndexOf(currentNode) - 1];
                    continue;
                }

                nextNode.DistanceOfPlayer = currentNode.DistanceOfPlayer + 1;
                reachableNodes.Add(nextNode);
                currentNode = nextNode;
            }

            return reachableNodes;
        }

        public Node GetAboveNodeForPlayer(Point playerPosition)
        {
            if (!CanPlayerMoveUp(playerPosition))
                return null;

            return Nodes[playerPosition.X, playerPosition.Y - 1];
        }

        public bool CanPlayerMoveUp(Point playerPosition)
        {
            if (playerPosition.Y == 0)
                return false;

            var nextNode = Nodes[playerPosition.X, playerPosition.Y - 1];
            return CanPlayerMoveTo(nextNode.Position);
        }

        public Node GetBelowNodeForPlayer(Point playerPosition)
        {
            if (!CanPlayerMoveDown(playerPosition))
                return null;

            return Nodes[playerPosition.X, playerPosition.Y + 1];
        }

        public bool CanPlayerMoveDown(Point playerPosition)
        {
            if (playerPosition.Y == Height - 1)
                return false;

            var nextNode = Nodes[playerPosition.X, playerPosition.Y + 1];
            return CanPlayerMoveTo(nextNode.Position);
        }

        public Node GetLeftNodeForPlayer(Point playerPosition)
        {
            if (!CanPlayerMoveLeft(playerPosition))
                return null;

            return Nodes[playerPosition.X - 1, playerPosition.Y];
        }

        public bool CanPlayerMoveLeft(Point playerPosition)
        {
            if (playerPosition.X == 0)
                return false;

            var nextNode = Nodes[playerPosition.X - 1, playerPosition.Y];
            return CanPlayerMoveTo(nextNode.Position);
        }

        public Node GetRightNodeForPlayer(Point playerPosition)
        {
            if (!CanPlayerMoveRight(playerPosition))
                return null;

            return Nodes[playerPosition.X + 1, playerPosition.Y];
        }

        public bool CanPlayerMoveRight(Point playerPosition)
        {
            if (playerPosition.X == Width - 1)
                return false;

            var nextNode = Nodes[playerPosition.X + 1, playerPosition.Y];
            return CanPlayerMoveTo(nextNode.Position);
        }

        public bool CanPlayerMoveTo(Point position)
        {
            return IsFloor(position) || IsItem(position);
        }

        public bool IsBomberMan(Point position)
        {
            return IsBomberMan(GetNode(position));
        }

        public bool IsBomberMan(Node node)
        {
            return node.Type == NodeType.Bomberman;
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

        internal Node GetNode(Point position)
        {
            return Nodes[position.X, position.Y];
        }

        public IEnumerable<Box> GetBoxesInRange(Node node, int explosionRange)
        {
            var boxes = new Collection<Box>();

            // Test Up
            for (var i = 1; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(0, -i);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsItem(currentPosition) || IsWall(currentPosition) || IsBomb(currentPosition))
                    break;

                if (IsBox(currentPosition))
                {
                    boxes.Add((Box)GetNode(currentPosition));
                    break;
                }
            }

            // Test Down
            for (var i = 1; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(0, i);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsItem(currentPosition) || IsWall(currentPosition) || IsBomb(currentPosition))
                    break;

                if (IsBox(currentPosition))
                {
                    boxes.Add((Box)GetNode(currentPosition));
                    break;
                }
            }

            // Test Left
            for (var i = 1; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(-i, 0);
                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsItem(currentPosition) || IsWall(currentPosition) || IsBomb(currentPosition))
                    break;

                if (IsBox(currentPosition))
                {
                    boxes.Add((Box)GetNode(currentPosition));
                    break;
                }
            }

            // Test Right
            for (var i = 1; i < explosionRange; i++)
            {
                var currentPosition = node.Position.Clone();
                currentPosition.Offset(i, 0);

                if (IsOutOfBounds(currentPosition))
                    break;

                if (IsFloor(currentPosition))
                    continue;

                if (IsItem(currentPosition) || IsWall(currentPosition) || IsBomb(currentPosition))
                    break;

                if (IsBox(currentPosition))
                {
                    boxes.Add((Box)GetNode(currentPosition));
                    break;
                }
            }

            //Console.Error.WriteLine("Boxes for Node {0} Range {1}", node.Position, explosionRange);
            //foreach (var box in boxes)
            //{
            //    Console.Error.WriteLine("Box {0}", box.Position);
            //}

            return boxes;
        }

        public bool IsBomb(Point position)
        {
            return IsBomb(Nodes[position.X, position.Y]);
        }

        public bool IsBomb(Node node)
        {
            return node.Type == NodeType.Bomb;
        }

        public bool IsWall(Point position)
        {
            return IsWall(Nodes[position.X, position.Y]);
        }

        public bool IsWall(Node node)
        {
            return node.Type == NodeType.Wall;
        }

        public bool IsItem(Point position)
        {
            return IsItem(Nodes[position.X, position.Y]);
        }

        public bool IsItem(Node node)
        {
            return node.Type == NodeType.Item;
        }

        public bool IsPlayer(Node node)
        {
            return node.Position == Player.Position;
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

        public NodeType ParseCellType(string cellType)
        {
            switch (cellType)
            {
                case Node.FloorCell:
                    return NodeType.Floor;

                case Node.EmptyCell:
                case Node.ItemExtraBombCell:
                case Node.ItemExtraRangeCell:
                    return NodeType.Box;

                case Node.WallCell:
                    return NodeType.Wall;

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
        Bomberman,
        Item,
        Wall,
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
