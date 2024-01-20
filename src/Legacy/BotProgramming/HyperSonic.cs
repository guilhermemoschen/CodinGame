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
                Board.Process();

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
            //Console.Error.WriteLine("Player {0} ExplosionCounter {1}", Game.Board.Player.Position, Game.Board.Player.ExplosionCounter);

            if (DodgeService.PlayerNeedsToMove())
            {
                Console.Error.WriteLine("PlayerNeedsToMove");

                var safePosition = Game.Board.GetClosestSafePosition();
                if (safePosition != null)
                {
                    Console.Error.WriteLine("Going To {0}", safePosition);
                    CurrentAction = new MoveAction(safePosition.Value);
                }
                else
                {
                    Console.Error.WriteLine("No Places to go");
                }
            }
            else if (Game.Board.Player.HasBombs)
            {
                Console.Error.WriteLine("Player HasBombs");
                CurrentAction = BombService.GetPlaceBomb();
            }
            else if (Game.Board.BombPossibilities.Any())
            {
                Console.Error.WriteLine("Moving for the next Possible Bomb");
                CurrentAction = new MoveAction(Game.Board.BombPossibilities.First().Bomb.Position);
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
        

        public bool PlayerNeedsToMove()
        {
            Console.Error.WriteLine("Player {0} ExplosionCounter {1} Distance {2}", Game.Board.Player.Position, Game.Board.Player.ExplosionCounter, Game.Board.Player.DistanceOfPlayer);

            if (!Game.Board.Player.WillExplode)
                return false;

            Console.Error.WriteLine("Player.WillExplode");

            if (!Game.Board.SafeNodes.Any())
                return false;

            Console.Error.WriteLine("Has Safe Nodes");

            if (Game.Board.Player.ExplosionCounter == 1)
                return true;

            Console.Error.WriteLine("Will not explode now");

            if (Game.Board.SafeNodes.Count == 1)
                return true;

            Console.Error.WriteLine("Has more than 1 safe node");

            foreach (var node in Game.Board.SafeNodes.OrderBy(x => x.DistanceOfPlayer).Take(5))
            {
                //Console.Error.WriteLine("safeNode Top 5 {0} DistanceOfPlayer {1} ExplosionCounter {2}", node.Position, node.DistanceOfPlayer, node.ExplosionCounter);
            }

            var safeNode = Game.Board.SafeNodes
                .OrderBy(x => x.DistanceOfPlayer)
                .First();

            Console.Error.WriteLine("Best safeNode {0} ExplosionCounter {1} Distance {2}", safeNode.Position, safeNode.ExplosionCounter, safeNode.DistanceOfPlayer);

            if (safeNode.DistanceOfPlayer == 1)
            {
                return safeNode.ExplosionCounter == 1;
            }

            var pathToSafeNode = Game.Board.GetPathForPlayerReachNode(safeNode);

            var beforeClosestSafeNode = pathToSafeNode
                .OrderByDescending(x => x.DistanceOfPlayer)
                .Skip(1)
                .First();

            Console.Error.WriteLine("beforeClosestSafeNode {0} DistanceOfPlayer {1} ExplosionCounter {2}", beforeClosestSafeNode.Position, beforeClosestSafeNode.DistanceOfPlayer, beforeClosestSafeNode.ExplosionCounter);

            return beforeClosestSafeNode.DistanceOfPlayer <= beforeClosestSafeNode.ExplosionCounter;
        }
    }

    public class PlaceBombAction : Action
    {
        public Point TargetPosition { get; set; }
        public Point? NextTargetPosition { get; set; }

        public PlaceBombAction(Bomb bomb, Node safeNode)
        {
            TargetPosition = bomb.Position;
            if (safeNode != null)
                NextTargetPosition = safeNode.Position;
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

    public class MovementService
    {
        
    }

    public class BombService
    {
        public BombService()
        {
        }

        public Action GetPlaceBomb()
        {
            if (!Game.Board.BombPossibilities.Any())
                return null;

            var bombPossibility = Game.Board.BombPossibilities.First();
            var safeNode = Game.Board.GetClosestSafeNode(bombPossibility);
            return new PlaceBombAction(bombPossibility.Bomb, safeNode);
        }
    }

    public class BombPossiblity
    {
        public IEnumerable<Node> AffectedNodes { get; set; }
        public Bomb Bomb { get; set; }
        public BombPossiblity()
        {
            AffectedNodes = new Collection<Node>();
        }

        public int Score
        {
            get
            {
                return (AffectedNodes.Where(x => x.Type == NodeType.Box).Count() * 2) - Bomb.DistanceOfPlayer;
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
        public const int RoundForABombToExplode = 8;

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

        public Bomb(Node node, int explosionRange)
        {
            Id = -1;
            Position = node.Position;
            RoundsToExplode = RoundForABombToExplode;
            ExplosionRange = explosionRange;
            DistanceOfPlayer = node.DistanceOfPlayer;
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
        public bool WillExplode
        {
            get
            {
                return ExplosionCounter != -1;
            }
        }
        public int ExplosionCounter { get; set; }

        public Point Position { get; set; }
        public NodeType Type { get; set; }

        public int DistanceOfPlayer { get; set; }

        public Node()
        {
            DistanceOfPlayer = -1;
            ExplosionCounter = -1;
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
        public IList<Node> ReachableNodes { get; private set; }
        public IList<Node> SafeNodes { get; private set; }
        public IList<BombPossiblity> BombPossibilities { get; set; }

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

        public void Process()
        {
            Player = BomberMans.First(x => x.Id == PlayerId);

            UpdateNodesThatWillExplode();
            ProcessReachableNodes();
            ProcessPossibleBombs();
            ProcessSafeNodes();
        }

        private void ProcessPossibleBombs()
        {
            BombPossibilities = new List<BombPossiblity>();

            foreach (var node in Game.Board.ReachableNodes.Where(x => !x.WillExplode))
            {
                var newBom = new Bomb(node, Game.Board.Player.ExplosionRange);

                var affectedNodes = GetAffectedNodes(newBom);
                if (affectedNodes.All(x => x.Type != NodeType.Box))
                    continue;

                BombPossibilities.Add(new BombPossiblity()
                {
                    Bomb = newBom,
                    AffectedNodes = affectedNodes,
                });
            }

            RemovePossibilitiesWhichKillPlayer();

            BombPossibilities = BombPossibilities
                .OrderByDescending(x => x.Score)
                .ToList();

            if (BombPossibilities.Any())
            {
                Console.Error.WriteLine("Best Possible Bomb {0} Score {1} Affected Boxes {2}", BombPossibilities.First().Bomb.Position, BombPossibilities.First().Score, string.Join(",", BombPossibilities.First().AffectedNodes.Where(x => x.Type == NodeType.Box).Select(x => x.Position.ToString())));
            }
        }

        private void RemovePossibilitiesWhichKillPlayer()
        {
            var wrongPossibilities = (
                from possibility in BombPossibilities
                let communNodes = possibility.AffectedNodes
                    .Join(ReachableNodes, x => x.Position, y => y.Position, (affected, reacable) => affected)
                where communNodes.Count() == ReachableNodes.Count()
                select possibility)
                .ToList();

            foreach (var wrongBomb in wrongPossibilities)
            {
                BombPossibilities.Remove(wrongBomb);
            }

            for (var i = 0; i < BombPossibilities.Count; i++)
            {
                var possibility = BombPossibilities[i];
                var pathToPlaceBomb = GetPathForPlayerReachNode(GetNode(possibility.Bomb.Position));
                if (pathToPlaceBomb.Any(x => x.DistanceOfPlayer == x.ExplosionCounter))
                {
                    //Console.Error.WriteLine("Remove possiblity due wrong path {0}", possibility.Bomb.Position);
                    BombPossibilities.Remove(possibility);
                    i--;
                }
            }
        }

        private void ProcessSafeNodes()
        {
            SafeNodes = ReachableNodes
                .Where(x => !x.WillExplode)
                .ToList();
        }

        public Node GetClosestSafeNode(BombPossiblity bombPossiblity)
        {
            return SafeNodes
                .Where(x => x.Position != bombPossiblity.Bomb.Position)
                .Except(bombPossiblity.AffectedNodes, new NodeEqualityComparer())
                .OrderBy(x => x.DistanceOfPlayer)
                .FirstOrDefault();
        }

        public Point? GetClosestSafePosition()
        {
            var safeNodes = Game.Board.SafeNodes
                .Where(x => x.Type != NodeType.Item)
                .OrderBy(x => x.DistanceOfPlayer)
                .ToList();

            for (var i = 0; i < safeNodes.Count; i++)
            {
                var safeNode = safeNodes[i];
                var path = Game.Board.GetPathForPlayerReachNode(safeNode);
                // Remove which in the path has an item
                if (path.Any(x => x.Type == NodeType.Item))
                {
                    safeNodes.Remove(safeNode);
                    i--;
                    continue;
                }

                // Remove position witch player doesnt have enough time
                if (path.Any(x => x.DistanceOfPlayer == x.ExplosionCounter))
                {
                    Console.Error.WriteLine("Removing SafeNodes {0} doesnt have enough time", safeNode.Position);
                    safeNodes.Remove(safeNode);
                    i--;
                    continue;
                }
            }

            if (safeNodes.Any())
                return safeNodes.First().Position;

            var items = Game.Board.ReachableNodes
                        .Where(x => x.Type == NodeType.Item)
                        .OrderBy(x => x.DistanceOfPlayer);
            if (items.Any())
            {
                return items.First().Position;
            }

            return null;
        }

        public void Update(IEnumerable<string> rows, IList<string[]> entities)
        {
            BomberMans.Clear();
            Bombs.Clear();
            Items.Clear();

            foreach (var entity in entities)
            {
                switch (entity[0])
                {
                    case Entity.BomberManEntityType:
                        BomberMans.Add(new BomberMan(entity));
                        break;

                    case Entity.BombEntityType:
                        Bombs.Add(new Bomb(entity));
                        break;

                    case Entity.ItemEntityType:
                        Items.Add(new Item(entity));
                        break;
                }
            }

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var row = rows
                        .Skip(y)
                        .First();

                    var nodeType = ParseCellType(row.Skip(x).First().ToString());

                    var bomberman = BomberMans.FirstOrDefault(query => query.Position.X == x && query.Position.Y == y);
                    if (bomberman != null)
                    {
                        Nodes[x, y] = bomberman;
                        continue;
                    }

                    var bomb = Bombs.FirstOrDefault(query => query.Position.X == x && query.Position.Y == y);
                    if (bomb != null)
                    {
                        Nodes[x, y] = bomb;
                        continue;
                    }

                    var item = Items.FirstOrDefault(query => query.Position.X == x && query.Position.Y == y);
                    if (item != null)
                    {
                        Nodes[x, y] = item;
                        continue;
                    }

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
        }

        public void UpdateNodesThatWillExplode()
        {
            foreach (var bomb in Bombs)
            {
                foreach (var node in GetAffectedNodes(bomb))
                {
                    if (node.ExplosionCounter == -1 || bomb.RoundsToExplode - 1 < node.ExplosionCounter)
                        node.ExplosionCounter = bomb.RoundsToExplode - 1;
                }
            }
            //var node12 = GetNode(new Point(1, 2));
            //Console.Error.WriteLine("node12 Type {0} Distance {1} ExplosionCounter {2}", node12.Type, node12.DistanceOfPlayer, node12.ExplosionCounter);
        }

        public IEnumerable<Node> GetAffectedNodes(Bomb bomb)
        {
            var affectedNodes = new List<Node>();
            var playerWithSamePosition = BomberMans.FirstOrDefault(x => x.Position == bomb.Position);
            if (playerWithSamePosition != null)
                affectedNodes.Add(playerWithSamePosition);
            else
                affectedNodes.Add(bomb);

            // Test Up
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(0, -i);
                if (IsOutOfBounds(currentPosition) || IsWall(currentPosition))
                    break;

                affectedNodes.Add(GetNode(currentPosition));

                if (IsItem(currentPosition) || IsBox(currentPosition))
                    break;
            }

            // Test Down
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(0, i);
                if (IsOutOfBounds(currentPosition) || IsWall(currentPosition))
                    break;

                affectedNodes.Add(GetNode(currentPosition));

                if (IsItem(currentPosition) || IsBox(currentPosition))
                    break;
            }

            // Test Left
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(-i, 0);
                if (IsOutOfBounds(currentPosition) || IsWall(currentPosition))
                    break;

                affectedNodes.Add(GetNode(currentPosition));

                if (IsItem(currentPosition) || IsBox(currentPosition))
                    break;
            }

            // Test Right
            for (var i = 1; i < bomb.ExplosionRange; i++)
            {
                var currentPosition = bomb.Position.Clone();
                currentPosition.Offset(i, 0);
                if (IsOutOfBounds(currentPosition) || IsWall(currentPosition))
                    break;

                affectedNodes.Add(GetNode(currentPosition));

                if (IsItem(currentPosition) || IsBox(currentPosition))
                    break;
            }

            return affectedNodes;
        }

        private void ProcessReachableNodes()
        {
            ReachableNodes = new List<Node>();
            var currentNode = (Node) Player;
            currentNode.DistanceOfPlayer = 0;
            ReachableNodes.Add(currentNode);
            while (true)
            {
                var nextNode = GetAboveNodeForPlayer(currentNode.Position);

                if (nextNode != null && ReachableNodes.Any(x => x.Position == nextNode.Position && x.DistanceOfPlayer <= currentNode.DistanceOfPlayer + 1))
                {
                    //Console.Error.WriteLine("Not using GetAboveNodeForPlayer {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);
                    nextNode = null;
                }

                nextNode = nextNode ?? GetBelowNodeForPlayer(currentNode.Position);

                if (nextNode != null && ReachableNodes.Any(x => x.Position == nextNode.Position && x.DistanceOfPlayer <= currentNode.DistanceOfPlayer + 1))
                {
                    //Console.Error.WriteLine("Not using GetBelowNodeForPlayer {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);
                    nextNode = null;
                }

                nextNode = nextNode ?? GetLeftNodeForPlayer(currentNode.Position);

                if (nextNode != null && ReachableNodes.Any(x => x.Position == nextNode.Position && x.DistanceOfPlayer <= currentNode.DistanceOfPlayer + 1))
                {
                    //Console.Error.WriteLine("Not using GetLeftNodeForPlayer {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);
                    nextNode = null;
                }

                nextNode = nextNode ?? GetRightNodeForPlayer(currentNode.Position);

                if (nextNode != null && ReachableNodes.Any(x => x.Position == nextNode.Position && x.DistanceOfPlayer <= currentNode.DistanceOfPlayer + 1))
                {
                    //Console.Error.WriteLine("Not using GetRightNodeForPlayer {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);
                    nextNode = null;
                }

                if (nextNode == null)
                {
                    //Console.Error.WriteLine("End of line for {0} Distance {1}", currentNode.Position, currentNode.DistanceOfPlayer);

                    if (ReachableNodes.IndexOf(currentNode) == 0)
                        break;

                    currentNode = ReachableNodes[ReachableNodes.IndexOf(currentNode) - 1];
                    continue;
                }

                nextNode.DistanceOfPlayer = currentNode.DistanceOfPlayer + 1;
                if (!ReachableNodes.Contains(nextNode, new NodeEqualityComparer()))
                {
                    //Console.Error.WriteLine("Add New Node {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);
                    ReachableNodes.Add(nextNode);
                }
                //else
                //    Console.Error.WriteLine("Existent Node {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);

                //Console.Error.WriteLine("currentNode {0} Distance {1}", currentNode.Position, currentNode.DistanceOfPlayer);
                //Console.Error.WriteLine("nextNode {0} Distance {1}", nextNode.Position, nextNode.DistanceOfPlayer);
                currentNode = nextNode;
            }
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
            return IsFloor(position) || IsItem(position) || Player.Position == position;
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

        public IList<Node> GetPathForPlayerReachNode(Node targetNode)
        {
            if (ReachableNodes.All(x => x.Position != targetNode.Position))
                return null;

            //Console.Error.WriteLine("From {0} To {1} ", Player.Position, targetNode.Position);

            var path = new List<Node>();
            path.Add(targetNode);
            var currentNode = targetNode;
            while (true)
            {
                var possibleNodes = new List<Node>
                {
                    GetAboveNodeForPlayer(currentNode.Position),
                    GetBelowNodeForPlayer(currentNode.Position),
                    GetLeftNodeForPlayer(currentNode.Position),
                    GetRightNodeForPlayer(currentNode.Position)
                };

                //Console.Error.WriteLine("currentNode {0} DistanceOfPlayer {1} ", currentNode.Position, currentNode.DistanceOfPlayer);
                //foreach (var node in possibleNodes.Where(x => x != null))
                //    Console.Error.WriteLine("possibleNodes {0} DistanceOfPlayer {1} ", node.Position, node.DistanceOfPlayer);

                currentNode = possibleNodes
                    .Where(x => x != null)
                    .OrderBy(x => x.DistanceOfPlayer)
                    .First();

                if (currentNode.Position == Player.Position)
                    break;

                path.Insert(0, currentNode);
            }

            path.Insert(0, Player);
            return path;
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

    public class NodeEqualityComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            return x.Position == y.Position;
        }

        public int GetHashCode(Node obj)
        {
            return obj.GetHashCode();
        }
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
