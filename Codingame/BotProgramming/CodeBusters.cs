using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CodinGame.BotProgramming.CodeBusters
{

    /**
     * Send your busters out into the fog to trap ghosts and bring them home!
     **/
    public class Player
    {
        static void Main(string[] args)
        {
            var totalBusters = int.Parse(Console.ReadLine()); // the amount of busters you control
            var totalGhosts = int.Parse(Console.ReadLine());  // the amount of ghosts on the map
            var teamId = int.Parse(Console.ReadLine()); // if this is 0, your base is on the top left of the map, if it is one, on the bottom right
            var board = new Board(teamId, totalBusters, totalGhosts);
            Console.Error.WriteLine("Our Team {0}", board.TeamId);

            while (true)
            {
                board.StartTurn();

                var entities = int.Parse(Console.ReadLine()); // the number of busters and ghosts visible to you
                for (int i = 0; i < entities; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');

                    var entity = new Entity()
                    {
                        Id = int.Parse(inputs[0]), // buster id or ghost id
                        Position = new Point(int.Parse(inputs[1]), int.Parse(inputs[2])) // position of this buster / ghost
                    };

                    var entityType = int.Parse(inputs[3]); // the team id if it is a buster, -1 if it is a ghost.
                    var entityState = int.Parse(inputs[4]); // For busters: 0=idle, 1=carrying a ghost.
                    var entityValue = int.Parse(inputs[5]); // For busters: Ghost id being carried. For ghosts: number of busters attempting to trap this ghost.

                    if (entityType == board.TeamId)
                    {
                        board.UpdateBuster(entity, entityState, entityValue);
                    }
                    else if (entityType == -1)
                    {
                        board.UpdateGhost(entity, entityState, entityValue);
                    }
                    else
                    {
                        board.UpdateEnemy(entity, entityState);
                    }
                }

                board.Process();

                foreach (var buster in board.Busters)
                {

                    // Write an action using Console.WriteLine()
                    // To debug: Console.Error.WriteLine("Debug messages...");

                    Console.WriteLine(buster.NextAction); // MOVE x y | BUST id | RELEASE
                }
            }
        }
    }

    public enum BusterState
    {
        IdleOrMovingBuster,
        CarryingGhost,
        Stunned,
        TrappingAGhost,
    }

    public abstract class Action
    {
        public int TeamId { get; private set; }

        protected string NextAction;

        public bool IsFinished { get; protected set; }

        protected Action(int teamId)
        {
            TeamId = teamId;
        }

        public abstract void Process();

        public string GetAction()
        {
            return NextAction;
        }
    }

    public class SearchGhostAction : Action
    {
        public const int SightOffset = 200;

        protected readonly IList<Point> Path = new List<Point>();
        protected int PathIndex = 0;
        protected Buster Buster;

        protected SearchGhostAction(int teamId, Buster buster) : base(teamId)
        {
            Buster = buster;
        }

        public Point TargetPoint
        {
            get { return Path[PathIndex]; }
        }

        public void GoToNextPoint()
        {
            PathIndex++;
            if (PathIndex == Path.Count)
                PathIndex = 0;
        }

        public override void Process()
        {
            if (Buster.Position == TargetPoint)
                GoToNextPoint();

            NextAction = TargetPoint.GetAction();
        }
    }

    public class BottomSearchGhostAction : SearchGhostAction
    {
        protected static int UsageCount = 0;

        public BottomSearchGhostAction(int teamId, Buster buster) : base(teamId, buster)
        {
            if (UsageCount % 2 == 0)
            {
                Path.Add(new Point(Board.Width - (Buster.Sight - SightOffset), Board.Height - (Buster.Sight - SightOffset))); // bottom right
                Path.Add(new Point(Buster.Sight - SightOffset, Board.Height - (Buster.Sight - SightOffset))); // bottom left
            }
            else
            {
                Path.Add(new Point(Buster.Sight - SightOffset, Board.Height - (Buster.Sight - SightOffset))); // bottom left
                Path.Add(new Point(Board.Width - (Buster.Sight - SightOffset), Board.Height - (Buster.Sight - SightOffset))); // bottom right
            }

            UsageCount++;
        }
    }

    public class TopSearchGhostAction : SearchGhostAction
    {
        protected static int UsageCount = 0;

        public TopSearchGhostAction(int teamId, Buster buster) : base(teamId, buster)
        {
            if (UsageCount % 2 == 0)
            {
                Path.Add(new Point(Board.Width - (Buster.Sight - SightOffset), Buster.Sight - SightOffset)); // top right
                Path.Add(new Point(Buster.Sight - SightOffset, Buster.Sight - SightOffset)); // top left
            }
            else
            {
                Path.Add(new Point(Buster.Sight - SightOffset, Buster.Sight - SightOffset)); // top left
                Path.Add(new Point(Board.Width - (Buster.Sight - SightOffset), Buster.Sight - SightOffset)); // top right
            }

            UsageCount++;
        }
    }

    public class TrapGhostAction : Action
    {
        public const int MinRangeToTrap = 1760;
        public const int MaxRangeToTrap = 900;

        public Ghost Ghost { get; protected set; }
        public PlayerBuster Buster { get; protected set; }
        public Action PreviewsAction { get; protected set; }

        public TrapGhostAction(int teamId, PlayerBuster buster, Ghost ghost, Action previewsAction) : base(teamId)
        {
            Ghost = ghost;
            Buster = buster;
            PreviewsAction = previewsAction;
        }

        public override void Process()
        {
            if (!Ghost.Initialized)
            {
                Console.Error.WriteLine("Buster {0} lost Ghost {1}", Buster.Id, Ghost.Id);
                Buster.Action = PreviewsAction;
                return;
            }

            var distance = Buster.Position.GetDistance(Ghost.Position);
            if (distance <= MinRangeToTrap && distance >= MaxRangeToTrap)
            {
                NextAction = $"BUST {Ghost.Id}";
            }
            else
            {
                Point targetPosition;

                if (distance <= MaxRangeToTrap)
                {
                    targetPosition = Buster.Position.GetMovedAwayPoint(Ghost.Position, MaxRangeToTrap);
                }
                else
                {
                    var targetDistance = Buster.Position.GetDistance(Ghost.Position) - MinRangeToTrap - (MaxRangeToTrap - MinRangeToTrap) / 2;
                    targetPosition = Buster.Position.GetCloserPoint(Ghost.Position, targetDistance);
                }
                
                NextAction = targetPosition.GetAction();
            }
        }
    }

    public class ReleaseGhostAction : Action
    {
        public const int ReleaseAreaRatio = 1600;
        public const int ReleaseAreaRatioOffset = 10;
        public PlayerBuster Buster;

        protected Point TargetPosition;

        public ReleaseGhostAction(int teamId, PlayerBuster buster) : base(teamId)
        {
            Buster = buster;

            Point releasePoint;

            switch (TeamId)
            {
                case 0:
                    releasePoint = new Point(0, 0);
                    break;

                case 1:
                    releasePoint = new Point(Board.Width, Board.Height);
                    break;

                default:
                    throw new Exception("Wrong Team Id");
            }


            var distance = Buster.Position.GetDistance(releasePoint);
            TargetPosition = Buster.Position.GetCloserPoint(releasePoint, Math.Abs(distance - (ReleaseAreaRatio - ReleaseAreaRatioOffset)));
        }

        public override void Process()
        {
            if (Buster.Position == TargetPosition)
            {
                NextAction = "RELEASE";
                Console.Error.WriteLine("Buster {0} released a Ghost", Buster.Id);
                IsFinished = true;
            }
            else
            {
                NextAction = TargetPosition.GetAction();
            }
        }
    }

    public class FindAndStuntAction : StuntAction
    {
        public Ghost ReferenceGhost { get; protected set; }
        public IList<EnemyBuster> Enemies { get; protected set; }

        public FindAndStuntAction(int teamId, PlayerBuster buster, Ghost referenceGhost, IList<EnemyBuster> enemies) : base(teamId, buster, null)
        {
            ReferenceGhost = referenceGhost;
            Enemies = enemies;
        }

        public override void Process()
        {
            if (Enemy != null)
                base.Process();
            else
            {
                var targetEnemies = Board.GetEnemiesTrappingAGhost(ReferenceGhost, Enemies);
                if (targetEnemies.Any())
                {
                    Enemy = targetEnemies.OrderBy(x => x.Position.GetDistance(Buster.Position)).First();
                }
                else
                {
                    Enemy = Enemies
                        .Where(x => x.Initialized && x.Position.GetDistance(Buster.Position) <= CodeBusters.Buster.Sight && !x.IsBeingChased)
                        .OrderBy(x => x.Position.GetDistance(Buster.Position))
                        .FirstOrDefault();
                }

                if (Enemy != null)
                {
                    Enemy.IsBeingChased = true;
                    base.Process();
                }
                else if (Buster.Position == ReferenceGhost.Position)
                {
                    Console.Error.WriteLine("Buster {0} couldnt find an enemy", Buster.Id);
                    Buster.Reset();
                }
                else
                {
                    NextAction = ReferenceGhost.Position.GetAction();
                }
            }
        }
    }

    public class StuntAction : Action
    {
        public const int MinRangeToStunt = 1760;

        public const int MaxAttempts = 3;
        public PlayerBuster Buster { get; protected set; }
        public EnemyBuster Enemy { get; protected set; }
        public int Attempts { get; protected set; }

        public StuntAction(int teamId, PlayerBuster buster, EnemyBuster enemy) : base(teamId)
        {
            Buster = buster;
            Enemy = enemy;
            if (Enemy != null)
                Enemy.IsBeingChased = true;
            Attempts = 0;
        }

        public override void Process()
        {
            if (Attempts == MaxAttempts || Enemy.State == BusterState.Stunned || !Enemy.Initialized)
            {
                Console.Error.WriteLine("Buster {0} gave up to stun Enemy {1}", Buster.Id, Enemy.Id);
                Buster.Reset();
                Enemy.IsBeingChased = false;
                return;
            }

            var distance = Buster.Position.GetDistance(Enemy.Position);
            if (distance < MinRangeToStunt)
            {
                Buster.UseStun();
                NextAction = $"STUN {Enemy.Id}";
                IsFinished = true;
                Enemy.State = BusterState.Stunned;
                Enemy.IsBeingChased = false;
            }
            else
            {
                NextAction = Enemy.Position.GetAction();
                Attempts++;
            }
        }
    }

    public class PlayerBuster : Buster
    {
        public const int WorthStepsToTrapGhost = 5;

        public bool CanUseStun => StunEnergy == MaxStunEnergy && State != BusterState.Stunned && !(Action is StuntAction);

        public bool CanReceiveNewAction => Action == null && State != BusterState.Stunned;

        public int GhostIdBeingCarried { get; set; }

        public bool IsSearchingGhosts
        {
            get
            {
                return
                    Initialized &&
                    State != BusterState.Stunned &&
                    (Action == null || Action is SearchGhostAction);
            }
        }

        public Action Action { get; set; }

        public string NextAction
        {
            get
            {
                return Action != null ?
                    Action.GetAction() :
                    Point.Empty.GetAction();
            }
        }

        public bool IsIdle => Action == null && State == BusterState.IdleOrMovingBuster;

        public void Process()
        {
            Action?.Process();
        }

        public PlayerBuster(int id) : base(id)
        {
            GhostIdBeingCarried = 0;
        }

        public void UpdateBuster(Entity entity, BusterState state, int ghostIdBeingCarried)
        {
            UpdateBuster(entity, state);
            GhostIdBeingCarried = ghostIdBeingCarried;
        }

        public override string ToString()
        {
            return $"Buster {Id} {State} GhostIdBeingCarried {GhostIdBeingCarried} {Action}";
        }

        public bool WorthStunEnemy(EnemyBuster enemy)
        {
            var distance = Position.GetDistance(enemy.Position);
            var stepsToStunt = distance / StuntAction.MinRangeToStunt;
            if (stepsToStunt <= 1)
                return true;

            if (State == BusterState.CarryingGhost || State == BusterState.TrappingAGhost)
                return false;

            return enemy.StunAvailable && stepsToStunt < StuntAction.MaxAttempts;
        }

        public bool WorthTrapGhost(Ghost ghost)
        {
            var distance = Position.GetDistance(ghost.Position);
            return (ghost.Stamina > 8 || ghost.Stamina == 0|| ghost.BustersTryingToTrap == 0) && (distance / Speed) <= 5;
        }

        public void Reset()
        {
            Action = null;
        }
    }

    public class Buster : Entity
    {
        public const int Sight = 2200;

        public const int Speed = 800;

        public const int MaxStunEnergy = 20;

        protected int StunEnergy;

        public bool StunAvailable => StunEnergy == MaxStunEnergy;

        public BusterState State { get; set; }

        public Buster(int id)
        {
            Id = id;
            State = BusterState.IdleOrMovingBuster;
            StunEnergy = MaxStunEnergy;
        }

        public void UseStun()
        {
            StunEnergy = 0;
        }

        public void UpdateBuster(Entity entity, BusterState state)
        {
            Position = entity.Position;
            State = state;
            Initialized = true;

            if (StunEnergy < MaxStunEnergy)
                StunEnergy++;
        }

        public static BusterState ConvertToBusterState(int state)
        {
            switch (state)
            {
                case 0:
                    return BusterState.IdleOrMovingBuster;
                case 1:
                    return BusterState.CarryingGhost;
                case 2:
                    return BusterState.Stunned;
                case 3:
                    return BusterState.TrappingAGhost;
                default:
                    throw new Exception("Error on parsing EntityState");
            }
        }
    }

    public class EnemyBuster : Buster
    {
        public bool IsBeingChased { get; set; }

        public EnemyBuster(int id) : base(id) { }

        public override string ToString()
        {
            return $"Ghost {Id} {State} StunAvailable {StunAvailable} BeingChased {IsBeingChased}";
        }
    }

    public class Ghost : Entity
    {
        public int BustersTryingToTrap { get; set; }

        public int Stamina { get; protected set; }

        public bool CanBeTrapped => Initialized;

        public Ghost(int id)
        {
            Id = id;
            BustersTryingToTrap = 0;
        }

        public void UpdateGhost(Entity entity, int stamina, int bustersTryingToTrap)
        {
            Position = entity.Position;
            Stamina = stamina;
            BustersTryingToTrap = bustersTryingToTrap;
            Initialized = true;
        }

        public override string ToString()
        {
            return $"Ghost {Id} Stamina {Stamina} BustersTryingToTrap {BustersTryingToTrap}";
        }
    }

    public class Entity
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public bool Initialized { get; set; }
        public Entity() { }

        public void Merge(Entity entity)
        {
            Position = entity.Position;
        }

        public override string ToString()
        {
            return $"{Id} - ({Position.X}, {Position.Y}) Initialized {Initialized}";
        }
    }

    public class Board
    {
        protected int SearchActionCount = 0;

        public const int Height = 9001;
        public const int Width = 16001;

        public int Turn { get; private set; }

        public int TeamId { get; private set; }

        public IList<PlayerBuster> Busters { get; private set; }

        public IList<Ghost> Ghosts { get; private set; }

        public IList<EnemyBuster> Enemies { get; private set; }

        public Board(int teamId, int totalBusters, int totalGhosts)
        {
            Turn = 0;
            TeamId = teamId;
            CreateBusters(totalBusters);
            CreateEnemies(totalBusters);
            CreateGhosts(totalGhosts);
        }

        public void StartTurn()
        {
            Turn++;

            foreach (var buster in Busters)
            {
                buster.Initialized = false;
            }

            foreach (var ghost in Ghosts)
            {
                ghost.Initialized = false;
            }

            foreach (var enemy in Enemies)
            {
                enemy.Initialized = false;
            }
        }

        public void UpdateBuster(Entity entity, int entityState, int entityValue)
        {
            var state = Buster.ConvertToBusterState(entityState);
            var buster = Busters.First(x => x.Id == entity.Id);
            buster.UpdateBuster(entity, state, entityValue);
            Console.Error.WriteLine(buster);
        }

        public void UpdateGhost(Entity entity, int entityState, int entityValue)
        {
            var ghost = Ghosts.First(x => x.Id == entity.Id);
            ghost.UpdateGhost(entity, entityState, entityValue);
            Console.Error.WriteLine(ghost);
        }

        public void UpdateEnemy(Entity entity, int entityState)
        {
            var state = Buster.ConvertToBusterState(entityState);
            var enemy = Enemies.First(x => x.Id == entity.Id);
            enemy.UpdateBuster(entity, state);

            if (enemy.Initialized)
                Console.Error.WriteLine("Enemy {0}", enemy);
        }

        protected void CreateBusters(int totalBusters)
        {
            Busters = new List<PlayerBuster>();

            for (var i = 0; i < totalBusters; i++)
            {
                var offset = (TeamId == 0) ? 0 : totalBusters;
                Busters.Add(new PlayerBuster(i + offset));
            }
        }

        protected void CreateEnemies(int totalBusters)
        {
            Enemies = new List<EnemyBuster>();

            for (var i = 0; i < totalBusters; i++)
            {
                var offset = (TeamId == 0) ? totalBusters : 0;
                Enemies.Add(new EnemyBuster(i + offset));
            }
        }

        protected void CreateGhosts(int totalGhosts)
        {
            Ghosts = new List<Ghost>();

            for (var i = 0; i < totalGhosts; i++)
            {
                Ghosts.Add(new Ghost(i));
            }
        }

        public void Process()
        {
            CleanBustersActions();
            ReleaseGhosts();
            ProcessGhosts();
            ProcessVisibleEnemies();
            ProcessStunTrapping();
            SearchForGhosts();
            ProcessBusters();
            ProcessRemainingIdleBusters();
        }

        private void ProcessRemainingIdleBusters()
        {
            while (Busters.Any(x => x.IsIdle))
            {
                var buster = Busters.First(x => x.IsIdle);
                buster.Action = GetNextSearchGhostAction(buster);
                buster.Action.Process();
            }
        }

        private void ReleaseGhosts()
        {
            foreach (var buster in Busters.Where(x => x.Action is TrapGhostAction && x.State == BusterState.CarryingGhost))
            {
                Console.Error.WriteLine("Buster {0} will release a Ghost", buster.Id);
                buster.Action = new ReleaseGhostAction(TeamId, buster);
            }
        }

        private void CleanBustersActions()
        {
            foreach (var buster in Busters.Where(x => x.Action != null && (x.State == BusterState.Stunned || x.Action.IsFinished)))
            {
                if (buster.State == BusterState.Stunned)
                {
                    var enemy = GetEnemyWhoStun(buster);
                    if (enemy != null)
                    {
                        enemy.UseStun();
                        Console.Error.WriteLine("Buster {0} got stun by {1}", buster.Id, enemy.Id);
                    }
                    else
                    {
                        Console.Error.WriteLine("Could not find who stun Buster {0}", buster);
                    }
                }

                Console.Error.WriteLine("Buster {0} Reseted", buster.Id);
                buster.Reset();
            }
        }

        private void SearchForGhosts()
        {
            while (Busters.Any(x => x.CanReceiveNewAction))
            {
                var buster = Busters.First(x => x.Action == null);
                buster.Action = GetNextSearchGhostAction(buster);
            }
        }

        private void ProcessStunTrapping()
        {
            var busters = Busters
                .Where(x => x.Action is TrapGhostAction && x.StunAvailable)
                .ToList();

            if (busters.Count == 1)
            {
                var buster = busters.First();
                var trapAction = (TrapGhostAction)buster.Action;
                if (!trapAction.Ghost.Initialized)
                    return;

                var totalEnemies = trapAction.Ghost.BustersTryingToTrap - 1;
                //Console.Error.WriteLine("1 Buster {0} Enemies", totalEnemies);
                if (totalEnemies == 1)
                {
                    if (WorthStunEnemy(totalEnemies, trapAction.Ghost))
                    {
                        var enemies = GetEnemiesTrappingAGhost(trapAction.Ghost, Enemies);
                        if (enemies.Any())
                        {
                            buster.Action = new StuntAction(TeamId, buster, enemies[0]);
                            Console.Error.WriteLine("Buster {0} going to stun Enemy {1}", buster.Id, enemies[0].Id);
                        }
                        else
                        {
                            buster.Action = new FindAndStuntAction(TeamId, buster, trapAction.Ghost, enemies);
                            Console.Error.WriteLine("Buster {0} going to find and stun an Enemy", buster.Id);
                        }
                    }
                }
                return;
            }

            foreach (var buster in busters)
            {
                if (!(buster.Action is TrapGhostAction))
                    continue;

                //Console.Error.WriteLine("ProcessStunTrapping Buster {0} ", buster);

                var trapAction = (TrapGhostAction)buster.Action;

                if (trapAction.Ghost.BustersTryingToTrap == 0)
                    continue;

                //if (trapAction.Ghost.Stamina > 50)
                //    continue;

                var compatibleBusters = busters
                    .Where(x => x.Id != buster.Id && x.Action is TrapGhostAction && ((TrapGhostAction) x.Action).Ghost.Id == trapAction.Ghost.Id)
                    .ToList();

                var totalBusters = compatibleBusters.Count + 1;
                var totalEnemies = trapAction.Ghost.BustersTryingToTrap - totalBusters;

                if (totalEnemies <= 0)
                    continue;

                if (totalBusters - totalEnemies > 0)
                {
                    Console.Error.WriteLine("Buster {0} is winning", buster.Id);
                    continue;
                }

                if (totalBusters - totalEnemies < -1)
                {
                    Console.Error.WriteLine("Buster {0} cannot win", buster.Id);
                    buster.Reset();
                    continue;
                }

                Console.Error.WriteLine("{0} Buster {1} Enemies", totalBusters, totalEnemies);

                if (WorthStunEnemy(totalEnemies, trapAction.Ghost))
                {
                    var enemies = GetEnemiesTrappingAGhost(trapAction.Ghost, Enemies);
                    if (enemies.Any())
                    {
                        var enemy = enemies
                            .OrderBy(x => x.State == BusterState.CarryingGhost)
                            .ThenBy(x => x.Position.GetDistance(buster.Position))
                            .First();

                        buster.Action = new StuntAction(TeamId, buster, enemy);
                        Console.Error.WriteLine("Buster {0} going to stun Enemy {1}", buster.Id, enemy.Id);
                    }
                    else
                    {
                        buster.Action = new FindAndStuntAction(TeamId, buster, trapAction.Ghost, Enemies);
                        Console.Error.WriteLine("Buster {0} going to find and stun an Enemy", buster.Id);
                    }

                    for (var i = 0; i < compatibleBusters.Count; i++)
                    {
                        var enemy = enemies
                            .Where(x => !x.IsBeingChased)
                            .OrderBy(x => x.State == BusterState.CarryingGhost)
                            .ThenBy(x => x.Position.GetDistance(compatibleBusters[i].Position))
                            .FirstOrDefault();

                        if (enemy != null)
                        {
                            compatibleBusters[i].Action = new StuntAction(TeamId, compatibleBusters[i], enemy);
                            Console.Error.WriteLine("Buster {0} going to stun Enemy {1}", compatibleBusters[i].Id, enemy.Id);
                        }
                        else
                        {
                            compatibleBusters[i].Action = new FindAndStuntAction(TeamId, compatibleBusters[i], trapAction.Ghost, Enemies);
                            Console.Error.WriteLine("Buster {0} going to find and stun an Enemy", compatibleBusters[i].Id);
                        }
                    }
                }
            }

        }

        private bool WorthStunEnemy(int totalEnemies, Ghost ghost)
        {
            return ghost.Stamina < (totalEnemies * 5) + 1;
        }

        public static IList<EnemyBuster> GetEnemiesTrappingAGhost(Ghost ghost, IList<EnemyBuster> enemies)
        {
            return enemies
                .Where(x => x.Initialized && x.State == BusterState.TrappingAGhost && x.Position.GetDistance(ghost.Position) <= TrapGhostAction.MinRangeToTrap)
                .ToList();
        }

        private void ProcessVisibleEnemies()
        {
            foreach (var enemy in Enemies.Where(x => x.Initialized && !x.IsBeingChased && x.State != BusterState.Stunned))
            {
                var buster = Busters
                    .Where(x => x.CanUseStun && x.WorthStunEnemy(enemy))
                    .OrderBy(x => x.Position.GetDistance(enemy.Position))
                    .FirstOrDefault();

                if (buster == null)
                    return;

                buster.Action = new StuntAction(TeamId, buster, enemy);
                Console.Error.WriteLine("Buster {0} going to stun Enemy {1}", buster.Id, enemy.Id);
            }
        }

        private void ProcessBusters()
        {
            foreach (var buster in Busters)
            {
                buster.Process();
            }
        }

        private void ProcessGhosts()
        {
            foreach (var ghost in Ghosts.Where(x => x.CanBeTrapped).OrderBy(x => x.Stamina))
            {
                var busters = GetAvailableBusters(ghost);

                while (busters.Any(x => x.WorthTrapGhost(ghost)))
                {
                    var buster = busters.First();
                    buster.Action = new TrapGhostAction(TeamId, buster, ghost, buster.Action);
                    Console.Error.WriteLine("Buster {0} going to trap Ghost {1}", buster.Id, ghost.Id);
                    busters = GetAvailableBusters(ghost);
                }
            }
        }

        private IEnumerable<PlayerBuster> GetBustersTrappingAGhost(Ghost ghost)
        {
            return 
                from buster in Busters.Where(x => x.State == BusterState.TrappingAGhost && x.Action is TrapGhostAction)
                let trapAction = buster.Action as TrapGhostAction
                where trapAction.Ghost.Id == ghost.Id
                select buster;
        }

        private EnemyBuster GetEnemyWhoStun(PlayerBuster buster)
        {
            return Enemies
                .Where(x => x.StunAvailable && x.Position.GetDistance(buster.Position) <= StuntAction.MinRangeToStunt)
                .OrderBy(x => x.Position.GetDistance(buster.Position))
                .FirstOrDefault();
        }

        private IList<PlayerBuster> GetAvailableBusters(Ghost ghost)
        {
            return Busters
                .Where(x => x.IsSearchingGhosts)
                .OrderBy(x => x.Position.GetDistance(ghost.Position))
                .ToList();
        }

        public SearchGhostAction GetNextSearchGhostAction(Buster buster)
        {
            SearchGhostAction action;

            if (SearchActionCount % 2 == 0)
            {
                action = new BottomSearchGhostAction(TeamId, buster);
            }
            else
            {
                action = new TopSearchGhostAction(TeamId, buster);
            }

            SearchActionCount++;

            return action;
        }
    }

    public static class Extensions
    {
        public static double GetDistance(this Point point, Point targetPoint)
        {
            return Math.Sqrt(Math.Pow(targetPoint.X - point.X, 2) + Math.Pow(targetPoint.Y - point.Y, 2));
        }

        public static string GetAction(this Point point)
        {
            return string.Format("MOVE {0} {1}", point.X, point.Y);
        }

        public static Point GetMovedAwayPoint(this Point point, Point referencePoint, int distanceToMoveAway)
        {
            var dx = point.X - referencePoint.X;
            var dy = point.Y - referencePoint.Y;

            var result = new Point(point.X, point.Y);

            if (dx > 0)
                result.X += distanceToMoveAway;
            else
                result.X -= distanceToMoveAway;

            if (dy > 0)
                result.Y += distanceToMoveAway;
            else
                result.Y -= distanceToMoveAway;

            if (result.X < 0)
                result.X = 0;
            else if (result.X > Board.Width)
                result.X = Board.Width;

            if (result.Y < 0)
                result.Y = 0;
            else if (result.Y > Board.Height)
                result.Y = Board.Height;

            return result;
        }

        public static Point GetCloserPoint(this Point point, Point targetPoint, double distanceToCloserPoint)
        {
            var opposite = targetPoint.X - point.X;
            var adjacent = targetPoint.Y - point.Y;
            var hypotenuse = point.GetDistance(targetPoint);
            var angle = Math.Asin(Math.Abs(opposite) / hypotenuse);
            var newOpposite = Math.Sin(angle) * distanceToCloserPoint;
            var newAdjacent = Math.Cos(angle) * distanceToCloserPoint;
            var c = new Point((int)newOpposite, (int)newAdjacent);

            if (opposite > 0)
                c.X = point.X + c.X;
            else if (opposite < 0)
                c.X = point.X - c.X;

            if (adjacent > 0)
                c.Y = point.Y + c.Y;
            else if (adjacent < 0)
                c.Y = point.Y - c.Y;

            return c;
        }
    }
}