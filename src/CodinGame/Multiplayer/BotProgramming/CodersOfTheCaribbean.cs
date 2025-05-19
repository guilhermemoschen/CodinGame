using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace CodinGame.Multiplayer.BotProgramming
{
    public class Board2
    {
        public const int Height = 21;
        public const int Width = 23;
        private ICollection<Entity2> entities;

        public Board2()
        {
            entities = new Collection<Entity2>();
        }

        internal void AddShip(int entityId, int x, int y, string arg1, string arg2, string arg3, string arg4)
        {
            var ship = new Ship(
                entityId, 
                new Point(x, y), 
                ConvertToShipRotation(arg1), 
                Convert.ToInt32(arg2), 
                Convert.ToInt32(arg3), 
                arg4 == "1");

            entities.Add(ship);
        }

        private Direction ConvertToShipRotation(string arg1)
        {
            switch (arg1)
            {
                case "0":
                    return Direction.East;

                case "1":
                    return Direction.NorthEast;

                case "2":
                    return Direction.NorthWest;

                case "3":
                    return Direction.West;

                case "4":
                    return Direction.SouthWest;

                case "5":
                    return Direction.SouthEast;

                default:
                    throw new ArgumentException();
            }
        }

        public void AddBarrel(int entityId, int x, int y, string arg1)
        {
            var barrel = new Barrel(entityId, new Point(x, y))
            {
                AmountOfRum = Convert.ToInt32(arg1),
            };

            entities.Add(barrel);
        }

        public void AddCannonBall(int entityId, int x, int y, string arg1, string arg2)
        {
            var cannonBall = new CannonBall(entityId, new Point(x, y))
            {
                OwnerId = Convert.ToInt32(arg1),
                TurnsBeforeImpact = Convert.ToInt32(arg2),
            };

            entities.Add(cannonBall);
        }

        public void UpdateCannonBallOwership()
        {
            var cannonBalls = entities
                .OfType<CannonBall>();

            foreach (var cannonBall in cannonBalls)
            {
                var ship = entities
                    .OfType<Ship>()
                    .FirstOrDefault(x => x.Id == cannonBall.OwnerId);

                if (ship == null)
                    continue;

                cannonBall.IsOwnedByPlayer = ship.IsControlledByPlayer;
            }
        }

        public void AddMine(int entityId, int x, int y)
        {
            var mine = new Mine(entityId, new Point(x, y));

            entities.Add(mine);
        }

        public IEnumerable<Ship> GetPlayerShips()
        {
            return entities
                .OfType<Ship>()
                .Where(x => x.IsControlledByPlayer);
        }

        public IEnumerable<Barrel> GetClosestBarrel(Point position, IEnumerable<Barrel> avoidedBarrels)
        {
            return entities
                .OfType<Barrel>()
                .Where(x => avoidedBarrels.All(y => y.Position != x.Position))
                .OrderBy(x => HexagonalExtensions.GetDistance(x.Position, position));
        }

        public Ship GetClosestEnemy(Point playerPosition)
        {
            return entities
                .OfType<Ship>()
                .Where(x => !x.IsControlledByPlayer)
                .OrderBy(x => HexagonalExtensions.GetDistance(x.Position, playerPosition))
                .First();
        }

        public IEnumerable<Mine> GetMines()
        {
            return entities
                .OfType<Mine>();
        }

        public Direction GetRotationForNextPosition(Point bowPosition, Direction shipRotation, Point targetPosition)
        {
            var forwardPosition = bowPosition.GetNextPosition(shipRotation);
            if (forwardPosition == targetPosition)
                return shipRotation;

            var westPosition = bowPosition.GetNextPosition(Direction.West);
            var northWestPosition = bowPosition.GetNextPosition(Direction.NorthWest);
            var northEastPosition = bowPosition.GetNextPosition(Direction.NorthEast);
            var southEastPosition = bowPosition.GetNextPosition(Direction.SouthEast);
            var southWestPosition = bowPosition.GetNextPosition(Direction.SouthWest);
            var eastPosition = bowPosition.GetNextPosition(Direction.East);

            switch (shipRotation)
            {
                case Direction.NorthWest:
                    if (westPosition == targetPosition)
                        return Direction.West;
                    if (northEastPosition == targetPosition)
                        return Direction.NorthEast;
                    break;

                case Direction.NorthEast:
                    if (northWestPosition == targetPosition)
                        return Direction.NorthWest;
                    if (eastPosition == targetPosition)
                        return Direction.East;
                    break;

                case Direction.East:
                    if (northEastPosition == targetPosition)
                        return Direction.NorthEast;
                    if (southEastPosition == targetPosition)
                        return Direction.SouthEast;
                    break;

                case Direction.SouthEast:
                    if (eastPosition == targetPosition)
                        return Direction.East;
                    if (southWestPosition == targetPosition)
                        return Direction.SouthWest;
                    break;

                case Direction.SouthWest:
                    if (southEastPosition == targetPosition)
                        return Direction.SouthEast;
                    if (westPosition == targetPosition)
                        return Direction.West;
                    break;

                case Direction.West:
                    if (southWestPosition == targetPosition)
                        return Direction.SouthWest;
                    if (northWestPosition == targetPosition)
                        return Direction.NorthWest;
                    break;
            }

            throw new Exception("Invalid! Ship Rotation");
        }

        public IEnumerable<Point> GetNextReachablePositions(Point bowPosition, Direction shipRoration, int depth)
        {
            var points = new List<Point>();

            foreach (var position in bowPosition.GetNextPossiblePositions(shipRoration))
            {
                points.Add(position);
                if (depth > 0 )
                {
                    var newShipRotation = GetRotationForNextPosition(bowPosition, shipRoration, position);
                    points.AddRange(GetNextReachablePositions(position, newShipRotation, depth - 1));
                }
            }

            return points.Distinct();
        }
        
        public IEnumerable<Entity2> GetEntities(IEnumerable<Point> positions)
        {
            return entities
                .Where(x => positions.Any(y => y == x.Position));
        }

        public IEnumerable<Entity2> GetThreats(IEnumerable<Point> positions)
        {
            var threats = new List<Entity2>();

            threats.AddRange(
                entities
                    .OfType<Mine>()
                    .Where(x => positions.Any(y => y == x.Position)));

            threats.AddRange(
                entities
                    .OfType<CannonBall>()
                    .Where(x => positions.Any(position => position == x.Target) && !x.IsOwnedByPlayer && x.TurnsBeforeImpact > 2));

            return threats;
        }

        public bool IsMine(Point position)
        {
            var entity = GetEntity(position);
            return entity is Mine;
        }

        public Entity2? GetEntity(Point position)
        {
            return entities
                .FirstOrDefault(x => x.Position == position);
        }

        public IEnumerable<Mine> GetAlreadyTargetMines()
        {
            var cannonBalls = entities
                .OfType<CannonBall>();

            var mines = entities
                .OfType<Mine>()
                .Where(x => cannonBalls.Any(y => y.Target == x.Position));

            return mines;
        }

        public IEnumerable<Point> GetPointsToTarget(Point from, Point to)
        {
            var path = new List<Point>();

            if (from == to)
                return path;

            var currentPoint = from.Clone();

            while (currentPoint != to)
            {
                var nextDirection = currentPoint.GetDirectionToPoint(to);
                path.AddRange(GetNextOccupiedPositions(currentPoint, nextDirection));
                currentPoint = currentPoint.GetNextPosition(nextDirection);
            }

            return path;
        }

        public bool IsThereAnyThreat(IEnumerable<Point> path)
        {
            return path
                .Any(x => IsThereAnyThreat(x));
        }

        public bool IsThereAnyThreat(Point position)
        {
            var entity = GetEntity(position);
            if (entity == null)
                return false;

            if (entity is Mine)
                return true;

            return false;
        }

        public IEnumerable<Point> GetNextOccupiedPositions(Ship ship, Direction direction)
        {
            switch (ship.Speed)
            {
                case 0:
                    return GetNextOccupiedPositions(ship.Position, direction);

                case 1:
                    return GetNextOccupiedPositions(ship.Bow, direction);

                case 2:
                    var nextPosition = ship.Bow
                        .GetNextPosition(direction)
                        .GetNextPosition(direction);

                    return GetNextOccupiedPositions(nextPosition, direction);
            }

            throw new Exception("Invalid Ship Speed");
        }

        public IEnumerable<Point> GetNextOccupiedPositions(Point position, Direction direction)
        {
            return new[]
            {
                position,
                position.GetNextPosition(direction),
                position.GetPreviewPosition(direction)
            };
        }

        public Direction GetNextDirection(Direction currentDirection, ShipMotion shipMotion)
        {
            if (shipMotion == ShipMotion.Forward || shipMotion == ShipMotion.None)
                return currentDirection;

            switch (currentDirection)
            {
                case Direction.NorthWest:
                    if (shipMotion == ShipMotion.Port)
                        return Direction.West;
                    else
                        return Direction.NorthEast;

                case Direction.NorthEast:
                    if (shipMotion == ShipMotion.Port)
                        return Direction.NorthWest;
                    else
                        return Direction.East;

                case Direction.East:
                    if (shipMotion == ShipMotion.Port)
                        return Direction.NorthEast;
                    else
                        return Direction.SouthEast;

                case Direction.SouthEast:
                    if (shipMotion == ShipMotion.Port)
                        return Direction.East;
                    else
                        return Direction.SouthWest;

                case Direction.SouthWest:
                    if (shipMotion == ShipMotion.Port)
                        return Direction.SouthEast;
                    else
                        return Direction.West;

                case Direction.West:
                    if (shipMotion == ShipMotion.Port)
                        return Direction.SouthWest;
                    else
                        return Direction.NorthWest;
            }

            throw new ArgumentException();
        }
    }

    public abstract class Entity2
    {
        public int Id { get; private set; }
        public Point Position { get; private set; }

        public Entity2(int id, Point position)
        {
            Id = id;
            Position = position;
        }
    }

    public class Barrel : Entity2
    {
        public const int MaxRumPerBarrel = 26;
        public const int MinRumPerBarrel = 10;

        public int AmountOfRum { get; set; }

        public Barrel(int id, Point position)
            : base(id, position)
        {

        }
    }

    public class Ship : Entity2
    {
        public const int Height = 3;
        public const int Width = 1;

        public bool IsControlledByPlayer { get; private set; }

        public int Speed { get; private set; }
        public Direction Direction { get; private set; }
        public int StockOfRum { get; private set; }

        public Point Bow { get; private set; }
        public Point Stern { get; private set; }

        public Ship(int id, Point position, Direction rotation, int speed, int stockOfRum, bool isControlledByPlayer)
            : base(id, position)
        {
            Direction = rotation;
            Speed = speed;
            StockOfRum = stockOfRum;
            IsControlledByPlayer = isControlledByPlayer;

            Initialize();
        }

        public void Initialize()
        {
            Bow = Position.GetNextPosition(Direction);
            Stern = Position.GetPreviewPosition(Direction);
        }

        public bool IsStopped
        {
            get
            {
                return Speed == 0;
            }
        }
    }

    public class CannonBall : Entity2
    {
        public int TurnsBeforeImpact { get; set; }
        public int OwnerId { get; set; }
        public bool IsOwnedByPlayer { get; set; }
        public Point Target { get; set; }

        public CannonBall(int id, Point position)
            : base(id, position)
        {
            Target = position;
        }
    }

    public class Mine : Entity2
    {
        public Mine(int id, Point position)
            : base(id, position)
        {
        }
    }

    public enum Direction
    {
        NorthWest,
        NorthEast,
        East,
        SouthEast,
        SouthWest,
        West,
    }

    public abstract class Command
    {
        public abstract string GetOutput();
    }

    public class ChangeSpeedCommand : Command
    {
        public ChangeSpeedOptions Option { get; private set; }

        public ChangeSpeedCommand(ChangeSpeedOptions option)
        {
            Option = option;
        }

        public override string GetOutput()
        {
            switch (Option)
            {
                case ChangeSpeedOptions.Faster:
                    return "FASTER";

                case ChangeSpeedOptions.Slower:
                    return "SLOWER";

                default:
                    throw new ArgumentException();
            }
        }
    }

    public enum ChangeSpeedOptions
    {
        Faster,
        Slower,
    }

    public class ChangeRotationCommand : Command
    {
        public ShipMotion Option { get; private set; }

        public ChangeRotationCommand(ShipMotion option)
        {
            Option = option;
        }

        public override string GetOutput()
        {
            switch (Option)
            {
                case ShipMotion.Port:
                    return "PORT";

                case ShipMotion.Starboard:
                    return "STARBOARD";

                case ShipMotion.Forward:
                    return "WAIT";

                default:
                    throw new ArgumentException();
            }
        }
    }

    public enum ShipMotion
    {
        Port,
        Starboard,
        Forward,
        None,
    }

    public class MoveCommand : Command
    {
        public MoveCommand(Point target)
        {
            Target = target;
        }

        public Point Target { get; private set; }

        public override string GetOutput()
        {
            return string.Format("MOVE {0} {1}", Target.X, Target.Y);
        }
    }

    public class WaitCommand : Command
    {
        public override string GetOutput()
        {
            return "WAIT";
        }
    }

    public class PlaceMineCommand : Command
    {
        public const int ReloadTime = 2;

        public override string GetOutput()
        {
            return "MINE";
        }
    }

    public class FireCommand : Command
    {
        public const int ReloadTime = 2;
        public const int FireSpeed = 3;
        public const int WeaponRange = 10;

        private Point target;

        public FireCommand(Point target, PlayerInfo info)
        {
            this.target = target;
            info.Fire();
        }

        public override string GetOutput()
        {
            return string.Format("FIRE {0} {1}", target.X, target.Y);
        }
    }

    public class CodersOfTheCaribbeanAI
    {
        private Random random;
        private Board2 board2 = null!;
        private ICollection<PlayerInfo> playerInfos = null!;
        private ICollection<Barrel> alreadyTargetBarrels;
        private IEnumerable<Mine> mines = null!;
        private IList<Entity2> alreadyTargetEntities;

        public CodersOfTheCaribbeanAI()
        {
            random = new Random();
            alreadyTargetBarrels = new Collection<Barrel>();
            alreadyTargetEntities = new Collection<Entity2>();
        }

        private PlayerInfo GetPlayerInfo(Ship playerShip)
        {
            return playerInfos.First(x => x.Ship.Id == playerShip.Id);
        }

        internal Command GetNextCommand(Ship playerShip)
        {
            Command? command = null;
            var info = GetPlayerInfo(playerShip);
            var barrels = board2.GetClosestBarrel(info.Ship.Position, alreadyTargetBarrels);
            var closestEnemy = board2.GetClosestEnemy(info.Ship.Position);
            var reachablePositions = board2.GetNextReachablePositions(info.Ship.Bow, info.Ship.Direction, info.Ship.Speed - 1);

            //foreach (var position in reachablePositions)
            //    Console.Error.WriteLine("{0} - reachablePositions {1}", info.Ship.Id, position);

            if (command == null)
                command = Dodge(info, reachablePositions);

            //if (command == null)
            //    command = ShootMine(info);

            if (command == null)
                command = GetNearByBarrels(info, reachablePositions);

            if (command == null)
                command = CreateRefuelCommand(info, barrels.FirstOrDefault());

            if (command == null && !info.Ship.IsStopped)
                command = FireOnEnemy(info, closestEnemy);

            //if (CanPlaceMine())
            //{
            //    mineCounter = PlaceMineCommand.ReloadTime;
            //    return new PlaceMineCommand();
            //}

            if (command == null)
            {
                command = GotoNextBarrel(info, barrels);
            }

            if (command == null)
            {
                Console.Error.WriteLine("{0} There is none barrels", info.Ship.Id);
                command = CreateRandomMoveCommand(info, reachablePositions);
            }

            info.LastCommand = command;
            Console.Error.WriteLine("{0} Command {1}", info.Ship.Id, command.GetOutput());
            return command;
        }

        private Command? GotoNextBarrel(PlayerInfo info, IEnumerable<Barrel> barrels)
        {
            if (!barrels.Any())
                return null;

            Barrel? targetBarrel = null;

            foreach (var barrel in barrels)
            {
                if (alreadyTargetBarrels.Contains(barrel))
                    continue;

                var pathToBarrel = board2.GetPointsToTarget(info.Ship.Position, barrel.Position);
                if (board2.IsThereAnyThreat(pathToBarrel))
                    continue;

                targetBarrel = barrel;
                break;
            }

            if (targetBarrel == null)
                return null;

            Console.Error.WriteLine("{0} Going to Next Barrel {1}", info.Ship.Id, targetBarrel.Position);
            alreadyTargetBarrels.Add(targetBarrel);
            return new MoveCommand(targetBarrel.Position);
        }

        private Command? Dodge(PlayerInfo info, IEnumerable<Point> reachablePositions)
        {
            var threats = board2.GetThreats(reachablePositions);
            var closestThreats = threats
                .Where(x => HexagonalExtensions.GetDistance(x.Position, info.Ship.Bow) == 1);

            //foreach (var threat in closestThreats)
            //    Console.Error.WriteLine("{0} threat {1}", info.Ship.Id, threat.Position);

            if (!closestThreats.Any())
                return null;

            var nextPosition = info.Ship.Bow.GetNextPosition(info.Ship.Direction);
            if (closestThreats.Any(x => x.Position == nextPosition) && !info.Ship.IsStopped)
            {
                Console.Error.WriteLine("{0} Dodge {1} with stop", info.Ship.Id, nextPosition);
                return new ChangeSpeedCommand(ChangeSpeedOptions.Slower);
            }

            var safePosition = reachablePositions
                .Where(x => !closestThreats.Any(y => y.Position == x))
                .FirstOrDefault();

            if (safePosition == null)
            {
                Console.Error.WriteLine("{0} no safe place do dodge", info.Ship.Id);
                if (info.Ship.IsStopped)
                    return new ChangeSpeedCommand(ChangeSpeedOptions.Faster);
                else
                    return new WaitCommand();
            }

            Console.Error.WriteLine("{0} Dodge to {1}", info.Ship.Id, safePosition);

            if (info.Ship.IsStopped)
            {
                if (safePosition == info.Ship.Bow.GetNextPosition(info.Ship.Direction))
                {
                    
                    return new ChangeSpeedCommand(ChangeSpeedOptions.Faster);
                }
            }

            var direction = GetDirectionForNextPosition(info.Ship, safePosition);
            return new ChangeRotationCommand(direction);
        }

        private Command? ShootMine(PlayerInfo info)
        {
            if (!info.CanFire())
                return null;

            var closestMine = mines
                .FirstOrDefault(x => HexagonalExtensions.GetDistance(x.Position, info.Ship.Bow) < 5);

            if (closestMine == null)
                return null;

            if (HasBeenTarget(closestMine))
                return null;

            return FireOnMine(info, closestMine);
        }

        private bool HasBeenTarget(Entity2 entity2)
        {
            return alreadyTargetEntities.Any(x => x == entity2);
        }

        private Command FireOnMine(PlayerInfo info, Mine closestMine)
        {
            Console.Error.WriteLine("{0} Fire on nearby mine", info.Ship.Id);
            alreadyTargetEntities.Add(closestMine);
            return new FireCommand(closestMine.Position, info);
        }

        private Command? CreateRefuelCommand(PlayerInfo info, Barrel? closestBarrel)
        {
            if (!ShouldRefuel(info) || closestBarrel == null)
                return null;

            Console.Error.WriteLine("ShouldRefuel");
            return new MoveCommand(closestBarrel.Position);
        }

        private Command? GetNearByBarrels(PlayerInfo info, IEnumerable<Point> nearByPositions)
        {
            var entities = board2.GetEntities(nearByPositions);

            var barrels = entities
                .OfType<Barrel>()
                .Where(x => HexagonalExtensions.GetDistance(x.Position, info.Ship.Bow) == 1);

            foreach (var barrel in barrels)
            {
                var shipMotion = GetDirectionForNextPosition(info.Ship, barrel.Position);
                var nextDirection = board2.GetNextDirection(info.Ship.Direction, shipMotion);
                var nextPositions = board2.GetNextOccupiedPositions(info.Ship.Bow, nextDirection);
                foreach (var p in nextPositions)
                {
                    Console.Error.WriteLine("{0} nextPositions {1} nextDirection {2}", info.Ship.Id, p, nextDirection);
                }

                if (board2.IsThereAnyThreat(nextPositions))
                    continue;

                Console.Error.WriteLine("{0} Nearby barrel", info.Ship.Id);
                return CreateCommandToReachPosition(info, barrel.Position);
            }

            return null;
        }

        private Command? CreateCommandToAvoidPosition(PlayerInfo info, Point targetPosition)
        {
            var nextPositions = board2.GetNextReachablePositions(info.Ship.Bow, info.Ship.Direction, 0);
            Point? safePosition = null;

            // Is Mine in front of the Ship
            if (!info.Ship.IsStopped && info.Ship.Bow.GetNextPosition(info.Ship.Direction) == targetPosition)
                return new ChangeSpeedCommand(ChangeSpeedOptions.Slower);

            foreach (var position in nextPositions)
            {
                Console.Error.WriteLine("{0} next possible safe position {1}", info.Ship.Id, position);

                if (board2.IsMine(position))
                    continue;

                safePosition = position;
                break;
            }

            if (safePosition == null)
                return null;

            var directionToMine = GetDirectionForNextPosition(info.Ship, safePosition.Value);
            return new ChangeRotationCommand(directionToMine);
        }

        private Command CreateCommandToReachPosition(PlayerInfo info,  Point targetPosition)
        {
            var distance = HexagonalExtensions.GetDistance(info.Ship.Bow, targetPosition);
            if (distance > 1)
                return new MoveCommand(targetPosition);
            else
            {
                return new ChangeRotationCommand(GetDirectionForNextPosition(info.Ship, targetPosition));
            }
        }

        public ShipMotion GetDirectionForNextPosition(Ship ship, Point targetPosition)
        {
            var forwardPosition = ship.Bow.GetNextPosition(ship.Direction);
            if (forwardPosition == targetPosition)
                return ShipMotion.Forward;

            var westPosition = ship.Bow.GetNextPosition(Direction.West);
            var northWestPosition = ship.Bow.GetNextPosition(Direction.NorthWest);
            var northEastPosition = ship.Bow.GetNextPosition(Direction.NorthEast);
            var southEastPosition = ship.Bow.GetNextPosition(Direction.SouthEast);
            var southWestPosition = ship.Bow.GetNextPosition(Direction.SouthWest);
            var eastPosition = ship.Bow.GetNextPosition(Direction.East);

            // Console.Error.WriteLine("Target{0} Bow{1} W{2} NW{3} NE{4} SE{5} SW{6} E{7}", targetPosition, ship.Bow, westPosition, northWestPosition, northEastPosition, southEastPosition, southWestPosition, eastPosition);

            switch (ship.Direction)
            {
                case Direction.NorthWest:
                    if (westPosition == targetPosition)
                        return ShipMotion.Port;
                    if (northEastPosition == targetPosition)
                        return ShipMotion.Starboard;
                    break;

                case Direction.NorthEast:
                    if (northWestPosition == targetPosition)
                        return ShipMotion.Port;
                    if (eastPosition == targetPosition)
                        return ShipMotion.Starboard;
                    break;

                case Direction.East:
                    if (northEastPosition == targetPosition)
                        return ShipMotion.Port;
                    if (southEastPosition == targetPosition)
                        return ShipMotion.Starboard;
                    break;

                case Direction.SouthEast:
                    if (eastPosition == targetPosition)
                        return ShipMotion.Port;
                    if (southWestPosition == targetPosition)
                        return ShipMotion.Starboard;
                    break;

                case Direction.SouthWest:
                    if (southEastPosition == targetPosition)
                        return ShipMotion.Port;
                    if (westPosition == targetPosition)
                        return ShipMotion.Starboard;
                    break;

                case Direction.West:
                    if (southWestPosition == targetPosition)
                        return ShipMotion.Port;
                    if (northWestPosition == targetPosition)
                        return ShipMotion.Starboard;
                    break;
            }

            throw new Exception("Invalid Ship Rotation");
        }

        public Command CreateRandomMoveCommand(PlayerInfo info, IEnumerable<Point> reachablePositions)
        {
            Console.Error.WriteLine("{0} Random Move", info.Ship.Id);

            var nextPosition = info.Ship.Bow.GetNextPosition(info.Ship.Direction);
            if (nextPosition != info.Ship.Bow && !nextPosition.IsOutOfBounds() && !board2.IsThereAnyThreat(nextPosition))
            {
                if (info.Ship.IsStopped)
                    return new ChangeSpeedCommand(ChangeSpeedOptions.Faster);
                else
                    return new WaitCommand();
            }

            var nextDirection = board2.GetNextDirection(info.Ship.Direction, ShipMotion.Port);
            var nextPositions = board2.GetNextOccupiedPositions(info.Ship, nextDirection);
            if (!board2.IsThereAnyThreat(nextPosition))
                return new ChangeRotationCommand(ShipMotion.Port);

            nextDirection = board2.GetNextDirection(info.Ship.Direction, ShipMotion.Starboard);
            nextPositions = board2.GetNextOccupiedPositions(info.Ship, nextDirection);
            if (!board2.IsThereAnyThreat(nextPosition))
                return new ChangeRotationCommand(ShipMotion.Starboard);

            return new MoveCommand(new Point(random.Next(Board2.Width), random.Next(Board2.Height)));
        }

        private bool ShouldRefuel(PlayerInfo playerInfo)
        {
            return playerInfo.Ship.StockOfRum < 30;
        }

        private bool ShouldFire(PlayerInfo playerInfo, Ship enemy)
        {
            if (enemy == null)
                return false;

            var distance = HexagonalExtensions.GetDistance(playerInfo.Ship.Position, enemy.Position);

            if (distance > FireCommand.WeaponRange)
                return false;

            return playerInfo.CanFire();
        }

        private Command? FireOnEnemy(PlayerInfo info, Ship enemy)
        {
            if (!ShouldFire(info, enemy))
                return null;

            if (enemy.IsStopped)
            {
                return new FireCommand(enemy.Position, info);
            }

            var distance = HexagonalExtensions.GetDistance(enemy.Position, info.Ship.Position);
            var targetPosition = enemy.Position.Clone();
            var shotOffset = (int)Math.Round((double)distance / (double)FireCommand.FireSpeed) + 1;
            targetPosition = targetPosition.Move(shotOffset, enemy.Direction);

            if (targetPosition.IsOutOfBounds())
                return null;

            var nextShipPosition = info.Ship.Bow.GetNextPosition(info.Ship.Direction);
            if (targetPosition == nextShipPosition)
                return null;

            alreadyTargetEntities.Add(enemy);
            return new FireCommand(targetPosition, info);
        }

        public void StartNewTurn(IEnumerable<Ship> playerShips, Board2 board2)
        {
            UpdatePlayerInfos(playerShips);
            this.board2 = board2;
            alreadyTargetBarrels.Clear();
            mines = board2.GetMines();
            alreadyTargetEntities = board2
                .GetAlreadyTargetMines()
                .OfType<Entity2>()
                .ToList();
        }

        private void UpdatePlayerInfos(IEnumerable<Ship> playerShips)
        {
            if (playerInfos == null)
            {
                playerInfos = new Collection<PlayerInfo>();
                foreach (var ship in playerShips)
                {
                    playerInfos.Add(new PlayerInfo(ship));
                }
            }
            else
            {
                foreach (var playerInfo in playerInfos.Where(x => !x.IsCrashed))
                {
                    var ship = playerShips.FirstOrDefault(x => x.Id == playerInfo.Ship.Id);
                    if (ship != null)
                    {
                        playerInfo.StartTurn(ship);
                    }
                    else
                    {
                        playerInfo.Crash();
                    }
                }
            }
        }
    }

    public class PlayerInfo
    {
        private int mineCounter;
        private int fireCounter;

        public Command LastCommand { get; set; } = null!;

        public PlayerInfo(Ship ship)
        {
            Ship = ship;
            mineCounter = 0;
            fireCounter = 0;
            IsCrashed = false;
        }

        public Ship Ship { get; private set; }
        public Barrel TargetBarrel { get; set; } = null!;

        public bool IsCrashed { get; private set; }

        public bool CanFire()
        {
            return fireCounter == 0;
        }

        public bool CanPlaceMine()
        {
            return mineCounter > 0;
        }

        public void StartTurn(Ship playerShip)
        {
            Ship = playerShip;

            if (mineCounter > 0)
                mineCounter--;

            if (fireCounter > 0)
                fireCounter--;
        }

        public void Crash()
        {
            IsCrashed = true;
        }

        internal void Fire()
        {
            fireCounter = FireCommand.ReloadTime;
        }
    }

    public class Game
    {
        private Board2 board2 = null!;
        private ICollection<Command> commands = null!;
        private CodersOfTheCaribbeanAI ai;

        public Game()
        {
            ai = new CodersOfTheCaribbeanAI();
        }

        public void GetInputs()
        {
            board2 = new Board2();

            var remainingShips = int.Parse(Console.ReadLine()!); // the number of remaining ships
            var entityCount = int.Parse(Console.ReadLine()!); // the number of entities (e.g. ships, mines or cannonballs)
            for (int i = 0; i < entityCount; i++)
            {
                var inputs = Console.ReadLine()!.Split(' ');
                var entityId = int.Parse(inputs[0]);
                var entityType = inputs[1];
                var x = int.Parse(inputs[2]);
                var y = int.Parse(inputs[3]);

                switch (entityType)
                {
                    case "SHIP":
                        board2.AddShip(entityId, x, y, inputs[4], inputs[5], inputs[6], inputs[7]);
                        break;

                    case "BARREL":
                        board2.AddBarrel(entityId, x, y, inputs[4]);
                        break;

                    case "CANNONBALL":
                        board2.AddCannonBall(entityId, x, y, inputs[4], inputs[4]);
                        break;

                    case "MINE":
                        board2.AddMine(entityId, x, y);
                        break;
                }
            }

            board2.UpdateCannonBallOwership();
        }

        internal void Process()
        {
            commands = new Collection<Command>();

            var ships = board2.GetPlayerShips();
            ai.StartNewTurn(ships, board2);

            foreach (var ship in ships)
            {
                commands.Add(ai.GetNextCommand(ship));
            }
        }

        internal IEnumerable<string> GetOutputs()
        {
            foreach (var command in commands)
            {
                yield return command.GetOutput();
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            var game = new Game();

            // game loop
            while (true)
            {
                game.GetInputs();
                game.Process();

                foreach (var output in game.GetOutputs())
                {
                    Console.WriteLine(output); // Any valid action, such as "WAIT" or "MOVE x y"
                }
            }
        }
    }

    public static class HexagonalExtensions
    {
        public static Direction GetDirectionToPoint(this Point point, Point targetPoint)
        {
            if (point == targetPoint)
                throw new Exception("The points are equal");

            if (point.Y == targetPoint.Y)
            {
                if (point.X < targetPoint.X)
                    return Direction.East;
                else
                    return Direction.West;
            }

            if (point.Y < targetPoint.Y)
            {
                if (point.X < targetPoint.X)
                    return Direction.SouthEast;
                else if(point.X > targetPoint.X)
                    return Direction.SouthWest;
                else
                {
                    if (point.IsEvenLine())
                        return Direction.SouthEast;
                    else
                        return Direction.SouthWest;
                }
            }
            else
            {
                if (point.X < targetPoint.X)
                    return Direction.NorthEast;
                else if (point.X > targetPoint.X)
                    return Direction.NorthWest;
                else
                {
                    if (point.IsEvenLine())
                        return Direction.NorthEast;
                    else
                        return Direction.NorthWest;
                }
            }
        }

        public static int GetDistance(this Point point, Point targetPoint)
        {
            if (point == targetPoint)
                return 0;

            var currentPoint = point.Clone();

            var distance = 0;

            while (currentPoint != targetPoint)
            {
                distance++;
                var direction = currentPoint.GetDirectionToPoint(targetPoint);
                currentPoint = currentPoint.GetNextPosition(direction);
            }

            //Console.Error.WriteLine("Distance from {0} to {1} is {2}", point, targetPoint, distance);

            return distance;

            //return (Math.Abs(point.Y - targetPoint.Y) + Math.Abs(point.Y + point.X - targetPoint.Y - targetPoint.X) + Math.Abs(point.X - targetPoint.X)) / 2;
        }

        public static Point Move(this Point point, int distance, Direction direction)
        {
            for (var i = 0; i < distance; i++)
            {
                switch (direction)
                {
                    case Direction.NorthWest:
                        if (point.Y % 2 != 0)
                            point.X--;
                        point.Y--;
                        break;
                    case Direction.NorthEast:
                        if (point.Y % 2 == 0)
                            point.X++;
                        point.Y--;
                        break;
                    case Direction.East:
                        point.X++;
                        break;
                    case Direction.SouthEast:
                        if (point.Y % 2 != 0)
                            point.X++;
                        point.Y++;
                        break;
                    case Direction.SouthWest:
                        if (point.Y % 2 == 0)
                            point.X--;
                        point.Y++;
                        break;
                    case Direction.West:
                        point.X--;
                        break;
                }
            }

            return point;
        }

        public static Point Clone(this Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static bool IsEvenLine(this Point point)
        {
            return point.Y % 2 == 0;
        }

        public static bool IsOddLine(this Point point)
        {
            return point.Y % 2 != 0;
        }

        public static Point GetNextPosition(this Point point, Direction direction)
        {
            Point nextPosition;

            switch (direction)
            {
                case Direction.NorthWest:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X - 1, point.Y - 1);
                    else
                        nextPosition = new Point(point.X, point.Y - 1);
                    break;

                case Direction.NorthEast:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X, point.Y - 1);
                    else
                        nextPosition = new Point(point.X + 1, point.Y - 1);
                    break;

                case Direction.East:
                    nextPosition = new Point(point.X + 1, point.Y);
                    break;

                case Direction.SouthEast:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X, point.Y + 1);
                    else
                        nextPosition = new Point(point.X + 1, point.Y + 1);
                    break;

                case Direction.SouthWest:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X - 1, point.Y + 1);
                    else
                        nextPosition = new Point(point.X, point.Y + 1);
                    break;

                case Direction.West:
                    nextPosition = new Point(point.X - 1, point.Y);
                    break;

                default:
                    throw new ArgumentException();
            }

            if (nextPosition.IsOutOfBounds())
                return point;
            else
                return nextPosition;
        }

        public static IEnumerable<Point> GetNextPossiblePositions(this Point point, Direction rotation)
        {
            var points = new Collection<Point>();
            points.Add(point.GetNextPosition(rotation));

            switch (rotation)
            {
                case Direction.NorthWest:
                    points.Add(point.GetNextPosition(Direction.West));
                    points.Add(point.GetNextPosition(Direction.NorthEast));
                    break;

                case Direction.NorthEast:
                    points.Add(point.GetNextPosition(Direction.NorthWest));
                    points.Add(point.GetNextPosition(Direction.East));
                    break;

                case Direction.East:
                    points.Add(point.GetNextPosition(Direction.NorthEast));
                    points.Add(point.GetNextPosition(Direction.SouthEast));
                    break;

                case Direction.SouthEast:
                    points.Add(point.GetNextPosition(Direction.East));
                    points.Add(point.GetNextPosition(Direction.SouthWest));
                    break;

                case Direction.SouthWest:
                    points.Add(point.GetNextPosition(Direction.West));
                    points.Add(point.GetNextPosition(Direction.SouthEast));
                    break;

                case Direction.West:
                    points.Add(point.GetNextPosition(Direction.NorthWest));
                    points.Add(point.GetNextPosition(Direction.SouthWest));
                    break;

                default:
                    throw new ArgumentException();
            }

            return points;
        }

        public static Point GetPreviewPosition(this Point point, Direction rotation)
        {
            Point nextPosition;

            switch (rotation)
            {
                case Direction.NorthWest:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X, point.Y + 1);
                    else
                        nextPosition = new Point(point.X + 1, point.Y + 1);
                    break;

                case Direction.NorthEast:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X - 1, point.Y + 1);
                    else
                        nextPosition = new Point(point.X, point.Y + 1);
                    break;

                case Direction.East:
                    nextPosition = new Point(point.X - 1, point.Y);
                    break;

                case Direction.SouthEast:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X - 1, point.Y - 1);
                    else
                        nextPosition = new Point(point.X, point.Y - 1);
                    break;

                case Direction.SouthWest:
                    if (point.IsEvenLine())
                        nextPosition = new Point(point.X, point.Y - 1);
                    else
                        nextPosition = new Point(point.X + 1, point.Y - 1);
                    break;

                case Direction.West:
                    nextPosition = new Point(point.X + 1, point.Y);
                    break;

                default:
                    throw new ArgumentException();
            }

            if (nextPosition.IsOutOfBounds())
                return point;
            else
                return nextPosition;
        }

        public static bool IsOutOfBounds(this Point point)
        {
            if (point.X < 0 || point.Y < 0)
                return true;

            if (point.X >= Board2.Width || point.Y >= Board2.Height)
                return true;

            return false;
        }

        public static IEnumerable<Point> GetNearByPositions(this Point point)
        {
            var positions = new Collection<Point>();

            // W
            positions.Add(new Point(point.X - 1, point.Y));
            // E
            positions.Add(new Point(point.X + 1, point.Y));

            if (point.IsEvenLine())
            {
                // NW
                positions.Add(new Point(point.X - 1, point.Y -1));
                // NE
                positions.Add(new Point(point.X, point.Y - 1));
                // SW
                positions.Add(new Point(point.X - 1, point.Y + 1));
                // SE
                positions.Add(new Point(point.X, point.Y + 1));
            }
            else
            {
                // NW
                positions.Add(new Point(point.X, point.Y - 1));
                // NE
                positions.Add(new Point(point.X + 1, point.Y - 1));
                // SW
                positions.Add(new Point(point.X, point.Y + 1));
                // SE
                positions.Add(new Point(point.X + 1, point.Y + 1));
            }

            return positions
                .Where(position => !position.IsOutOfBounds());
        }
    }
}