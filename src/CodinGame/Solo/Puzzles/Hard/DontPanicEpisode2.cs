using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;

namespace Codingame.TheParanoidAndroidOneStepFurther
{
    class Player
    {
        static void Main(string[] args)
        {
            //var test = Game.Tests["01"];
            //var test = Game.Tests["04"];
            //var test = Game.Tests["06"];
            //var test = Game.Tests["07"];
            //var test = Game.Tests["08"];
            var test = Game.Tests["09"];

            string input;
            if (args.Length == 0)
            {
                input = Console.ReadLine()!;
                Console.Error.WriteLine(input);
            }
            else
            {
                input = test.BoardSettings;
            }

            var game = new Game(input);

            //Console.Error.WriteLine(game);

            if (args.Length == 0)
            {
                for (var i = 0; i < game.AmountOfElevators; i++)
                {
                    var elevatorInput = Console.ReadLine()!;
                    Console.Error.WriteLine(elevatorInput);
                    game.AddElevatorPosition(elevatorInput);
                }
            }
            else
            {
                foreach (var elevatorInput in test.Elevators)
                {
                    game.AddElevatorPosition(elevatorInput);
                }
            }

            while (true)
            {
                Clone leadClone;
                if (args.Length == 0)
                {
                    var leadCloneInput = Console.ReadLine()!;
                    //Console.Error.WriteLine(leadCloneInput);
                    leadClone = new Clone(leadCloneInput);
                }
                else
                {
                    leadClone = new Clone(test.FirstClone);
                }

                //Console.Error.WriteLine(leadClone);

                if (!game.IsCalculated)
                {
                    Console.Error.WriteLine(leadClone);
                    game.CreateBoard(leadClone);
                    game.CalculateActions();
                }

                var action = game.GetNextAction();

                if (action == null)
                    break;

                Console.WriteLine(action);
            }
        }
    }

    public class Game
    {
        public int AmountOfFloors { get; set; }
        public Point ExitPosition { get; set; }
        public Clone InitialClone { get; set; } = null!;
        public int AmountOfAdditionalElevators { get; set; }
        public int AmountOfElevators { get; set; }
        public IList<Point> ElevatorsPositions { get; set; }
        public int BoardWidth { get; set; }
        public int MaxActions { get; set; }
        public int TotalClones { get; set; }
        public Board Board { get; set; } = null!;
        public ICollection<CloneAction> Actions { get; set; }

        public bool IsCalculated
        {
            get { return Board != null; }
        }

        public static Dictionary<string, Test> Tests = new Dictionary<string, Test>();

        static Game()
        {
            Tests.Add(
                "01",
                new Test()
                {
                    BoardSettings = "2 13 100 1 11 10 1 0",
                    Elevators = new Collection<string>(),
                    FirstClone = "0 2 RIGHT"
                }
            );

            Tests.Add(
                "04",
                new Test()
                {
                    BoardSettings = "6 13 100 5 1 10 2 3",
                    Elevators = new Collection<string> { "2 7", "0 4", "4 1", },
                    FirstClone = "0 10 RIGHT"
                }
            );

            Tests.Add(
                "06",
                new Test()
                {
                    BoardSettings = "10 19 47 9 9 41 0 17",
                    Elevators = new Collection<string>
                        { "0 9", "5 4", "2 9", "6 9", "0 3", "7 4", "5 17", "3 17", "2 3", "4 9", "8 9", "7 17", "4 3", "1 17", "1 4", "3 4", "6 3" },
                    FirstClone = "0 6 RIGHT"
                }
            );

            Tests.Add(
                "07",
                new Test()
                {
                    BoardSettings = "10 19 42 9 9 41 1 16",
                    Elevators = new Collection<string>
                        { "0 9", "5 4", "2 9", "6 9", "0 3", "7 4", "5 17", "3 17", "2 3", "4 9", "8 9", "7 17", "4 3", "1 17", "1 4", "6 3" },
                    FirstClone = "0 6 RIGHT"
                }
            );

            Tests.Add(
                "08",
                new Test()
                {
                    BoardSettings = "13 36 67 11 12 41 4 34",
                    Elevators = new Collection<string>
                    {
                        "2 34", "5 34", "4 9", "8 23", "0 34", "4 23", "8 1", "10 3", "6 34", "3 17", "4 34", "5 4", "11 13", "7 34", "9 34", "11 11",
                        "1 34", "7 17", "6 13", "1 4", "2 24", "8 9", "1 17", "11 4", "6 22", "1 24", "10 23", "3 34", "9 17", "2 3", "8 34", "2 23",
                        "10 34", "9 2"
                    },
                    FirstClone = "0 6 RIGHT"
                }
            );

            Tests.Add(
                "09",
                new Test()
                {
                    BoardSettings = "13 69 79 11 39 8 5 30",
                    Elevators = new Collection<string>
                    {
                        "5 46", "8 66", "5 4", "10 23", "1 50", "10 3", "1 34", "8 34", "6 65", "3 17", "11 42", "7 17", "8 23", "8 56", "11 13",
                        "2 58", "8 9", "1 4", "11 11", "1 24", "1 17", "6 13", "2 24", "8 1", "11 38", "2 23", "2 3", "6 57", "11 4", "6 34"
                    },
                    FirstClone = "0 33 RIGHT"
                }
            );
        }

        public Game(string input)
        {
            var inputs = input.Split(' ');

            BoardWidth = Convert.ToInt32(inputs[1]);
            MaxActions = Convert.ToInt32(inputs[2]);
            TotalClones = Convert.ToInt32(inputs[5]);

            AmountOfFloors = Convert.ToInt32(inputs[0]);
            ExitPosition = new Point(Convert.ToInt32(inputs[4]), Convert.ToInt32(inputs[3]));
            AmountOfAdditionalElevators = Convert.ToInt32(inputs[6]);
            AmountOfElevators = Convert.ToInt32(inputs[7]);
            ElevatorsPositions = new List<Point>();

            Actions = new Collection<CloneAction>();
        }

        public void AddElevatorPosition(string input)
        {
            var inputs = input.Split(' ');
            ElevatorsPositions.Add(new Point(Convert.ToInt32(inputs[1]), Convert.ToInt32(inputs[0])));
            ElevatorsPositions = ElevatorsPositions.OrderBy(x => x.Y).ToList();
        }

        public override string ToString()
        {
            return string.Format(
                "Amount of Floors {0}\n" +
                "Board Width {1}\n" +
                "Max Rounds {2}\n" +
                "Total Clones {3}\n" +
                "Amount of Additional Elevators {4} \n" +
                "Amount of Elevators {5} \n" +
                "Exit Position {6}",
                AmountOfFloors,
                BoardWidth,
                MaxActions,
                TotalClones,
                AmountOfAdditionalElevators,
                AmountOfElevators,
                ExitPosition);
        }

        public string? GetNextAction()
        {
            if (Actions.Any())
            {
                var nextAction = Actions.FirstOrDefault();
                Actions.Remove(nextAction);
                return nextAction.ToString().ToUpper();
            }

            return null;
        }

        public void CreateBoard(Clone initialClone)
        {
            InitialClone = initialClone;

            Board = new Board(BoardWidth, AmountOfFloors, InitialClone.Position, ExitPosition, ElevatorsPositions);
        }

        public void CalculateActions()
        {
            Actions = CalculateNextAction(AmountOfAdditionalElevators, TotalClones + 10, new Collection<CloneAction>(), InitialClone);

            if (Actions == null)
                Console.Error.WriteLine("Couldn't calculate all actions.");
        }

        public ICollection<CloneAction> CalculateNextAction(int additionalElevatorsLeft, int clonesLeft, ICollection<CloneAction> actions,
            Clone currentClone)
        {
            if (MaxActions <= actions.Count || clonesLeft < 0)
                return new List<CloneAction>();

            if (currentClone.Position.X == 23 && currentClone.Position.Y == 11)
            {
            }

            if (currentClone.Position.X < 0 || currentClone.Position.X >= BoardWidth || currentClone.Position.Y < 0 ||
                currentClone.Position.Y >= AmountOfFloors)
                return new List<CloneAction>();

            ICollection<CloneAction> allActions;
            List<CloneAction> newActions;
            Clone nextLeadClone;

            var exitPoint = GetNodesByTypeAndFloor(NodeType.ExitPoint, currentClone.Position.Y).FirstOrDefault();
            if (exitPoint != null && CanGoToPoint(currentClone.Position, exitPoint.Position))
            {
                newActions = CreateNewActionList(actions, GetActionsToReachNode(currentClone, exitPoint));
                newActions.Add(CloneAction.Wait);

                return newActions.Count > MaxActions
                    ? new List<CloneAction>()
                    : newActions;
            }

            var elevators = GetNodesByTypeAndFloor(NodeType.Elevator, currentClone.Position.Y);
            foreach (var elevator in elevators)
            {
                if (!CanGoToPoint(currentClone.Position, elevator.Position))
                    continue;

                var actionsToNode = GetActionsToReachNode(currentClone, elevator);
                if (actionsToNode.Contains(CloneAction.Block))
                    clonesLeft--;

                newActions = CreateNewActionList(actions, actionsToNode);
                newActions.Add(CloneAction.Wait); // elevator
                nextLeadClone = CreateNextLeadClone(currentClone, new Point(elevator.Position.X, elevator.Position.Y + 1));
                allActions = CalculateNextAction(additionalElevatorsLeft, clonesLeft, newActions, nextLeadClone);

                if (allActions != null)
                    return allActions;
            }

            // can build an elevator?
            if (additionalElevatorsLeft > 0 && clonesLeft > 0)
            {
                if (IsExitPointInNextFloor(currentClone.Position))
                {
                    var targetNode = Board.Nodes[ExitPosition.X, ExitPosition.Y - 1];
                    if (CanGoToPoint(currentClone.Position, targetNode.Position))
                    {
                        newActions = CreateNewActionList(actions, GetActionsToReachNode(currentClone, targetNode));
                        newActions.AddRange(GetActionsToCreateElevator());
                        newActions.Add(CloneAction.Wait); // reach elevator

                        if (newActions.Count <= this.MaxActions)
                            return newActions;
                    }
                }

                var targetNodes = new List<Node>()
                {
                    Board.Nodes[currentClone.Position.X, currentClone.Position.Y]
                };

                if (IsElevatorAbove(currentClone.Position))
                {
                    if (currentClone.Position.X > 0)
                    {
                        var nextNode = Board.Nodes[currentClone.Position.X + 1, currentClone.Position.Y];
                        if (nextNode.Type == NodeType.Empty)
                            targetNodes.Add(nextNode);
                    }

                    if (currentClone.Position.X < BoardWidth - 1)
                    {
                        var previewNode = Board.Nodes[currentClone.Position.X - 1, currentClone.Position.Y];
                        if (previewNode.Type == NodeType.Empty)
                            targetNodes.Add(previewNode);
                    }
                }

                foreach (var targetNode in targetNodes)
                {
                    var actionsToNode = GetActionsToReachNode(currentClone, targetNode);
                    newActions = CreateNewActionList(actions, actionsToNode);
                    newActions.AddRange(GetActionsToCreateElevator());
                    nextLeadClone = CreateNextLeadClone(currentClone, new Point(targetNode.Position.X, targetNode.Position.Y + 1));
                    allActions = CalculateNextAction(additionalElevatorsLeft - 1, clonesLeft - 1, newActions, nextLeadClone);
                    if (allActions != null)
                        return allActions;
                }
            }

            //// Can go right
            //var distanceNextElevatorToRight = GetDistanceFromNextNodeType(NodeType.Elevator, currentClone.Position, CloneDirection.Right);

            //// The elevator is in the same position of the clone
            //if (distanceNextElevatorToRight == 0)
            //{
            //    newActions = new List<CloneAction>(actions);

            //    AddStepsToReachNode(newActions, distanceNextElevatorToRight);

            //    nextLeadClone = new Clone()
            //    {
            //        CloneDirection = currentClone.CloneDirection,
            //        Position = new Point(currentClone.Position.X + distanceNextElevatorToRight, currentClone.Position.Y + 1)
            //    };

            //    allActions = CalculateNextAction(maxActions, additionalElevatorsLeft, newActions, nextLeadClone);

            //    if (allActions != null)
            //        return allActions;
            //}

            //if (distanceNextElevatorToRight != -1)
            //{
            //    newActions = new List<CloneAction>(actions);

            //    if (currentClone.CloneDirection != CloneDirection.Right)
            //        AddStepsToBlock(newActions);

            //    AddStepsToReachNode(newActions, distanceNextElevatorToRight);

            //    nextLeadClone = new Clone()
            //    {
            //        CloneDirection = CloneDirection.Right,
            //        Position = new Point(currentClone.Position.X + distanceNextElevatorToRight, currentClone.Position.Y + 1)
            //    };

            //    allActions = CalculateNextAction(maxActions, additionalElevatorsLeft, newActions, nextLeadClone);

            //    if (allActions != null)
            //        return allActions;
            //}

            //// Can go left
            //var distanceNextElevatorToLeft = GetDistanceFromNextNodeType(NodeType.Elevator, currentClone.Position, CloneDirection.Left);
            //if (distanceNextElevatorToLeft != -1)
            //{
            //    newActions = new List<CloneAction>(actions);

            //    if (currentClone.CloneDirection != CloneDirection.Left)
            //        AddStepsToBlock(newActions);

            //    AddStepsToReachNode(newActions, distanceNextElevatorToLeft);

            //    nextLeadClone = new Clone()
            //    {
            //        CloneDirection = CloneDirection.Left,
            //        Position = new Point(currentClone.Position.X - distanceNextElevatorToLeft, currentClone.Position.Y + 1)
            //    };

            //    allActions = CalculateNextAction(maxActions, additionalElevatorsLeft, newActions, nextLeadClone);

            //    if (allActions != null)
            //        return allActions;
            //}

            return new List<CloneAction>();
        }

        private bool IsElevatorAbove(Point currentPosition)
        {
            return currentPosition.Y + 1 < AmountOfFloors && Board.Nodes[currentPosition.X, currentPosition.Y + 1].Type == NodeType.Elevator;
        }

        private bool IsExitPointInNextFloor(Point position)
        {
            return ExitPosition.Y == position.Y + 1;
        }

        private Clone CreateNextLeadClone(Clone clone, Point destination)
        {
            var leadClone = new Clone()
            {
                CloneDirection = clone.CloneDirection,
                Position = destination
            };

            var offset = clone.Position.X - destination.X;

            if ((offset > 0 && clone.CloneDirection == CloneDirection.Right) ||
                (offset < 0 && clone.CloneDirection == CloneDirection.Left))
            {
                leadClone.CloneDirection = (clone.CloneDirection == CloneDirection.Right)
                    ? CloneDirection.Left
                    : CloneDirection.Right;
            }

            return leadClone;
        }

        private IEnumerable<CloneAction> GetActionsToReachNode(Clone clone, Node targetNode)
        {
            var actions = new List<CloneAction>();

            var distance = clone.Position.X - targetNode.Position.X;

            // should block?
            if ((distance > 0 && clone.CloneDirection == CloneDirection.Right) ||
                (distance < 0 && clone.CloneDirection == CloneDirection.Left))
            {
                actions.AddRange(GetStepsToBlock());
            }

            distance = Math.Abs(distance);

            // Reach Elevator
            for (var i = 0; i < distance; i++)
            {
                actions.Add(CloneAction.Wait);
            }

            return actions;
        }

        private List<CloneAction> CreateNewActionList(IEnumerable<CloneAction> actions1, IEnumerable<CloneAction> actions2)
        {
            var newList = new List<CloneAction>();
            newList.AddRange(actions1);
            newList.AddRange(actions2);
            return newList;
        }

        private bool CanGoToPoint(Point currentPoint, Point destinationPoint)
        {
            if (currentPoint.X == destinationPoint.X)
                return true;

            if (currentPoint.X < destinationPoint.X)
            {
                for (var i = currentPoint.X + 1; i < destinationPoint.X; i++)
                {
                    if (Board.Nodes[i, currentPoint.Y].Type != NodeType.Empty)
                        return false;
                }
            }
            else if (currentPoint.X > destinationPoint.X)
            {
                for (var i = currentPoint.X - 1; i > destinationPoint.X; i--)
                {
                    if (Board.Nodes[i, currentPoint.Y].Type != NodeType.Empty)
                        return false;
                }
            }

            return true;
        }

        private IEnumerable<Node> GetNodesByTypeAndFloor(NodeType nodeType, int floor)
        {
            for (var i = 0; i < BoardWidth; i++)
            {
                var node = Board.Nodes[i, floor];
                if (node.Type == nodeType)
                    yield return node;
            }
        }

        private IEnumerable<CloneAction> GetActionsToCreateElevator()
        {
            var newActions = new Collection<CloneAction>
            {
                CloneAction.Elevator,
                CloneAction.Wait,
                CloneAction.Wait,
                CloneAction.Wait
            };

            return newActions;
        }

        private IEnumerable<CloneAction> GetStepsToBlock()
        {
            return new Collection<CloneAction>
            {
                CloneAction.Block,
                CloneAction.Wait,
                CloneAction.Wait
            };
        }

        //private bool IsAnyElevatorInTheFloor(int floor)
        //{
        //    for (var i = 0; i < BoardWidth; i++)
        //    {
        //        if (Board.Nodes[i, floor].Type == NodeType.Elevator)
        //            return true;
        //    }

        //    return false;
        //}

        //private int GetDistanceFromNextNodeType(NodeType nodeType, Point position, CloneDirection direction)
        //{
        //    if (direction == CloneDirection.Right)
        //    {
        //        for (var i = position.X; i < BoardWidth; i++)
        //        {
        //            if (Board.Nodes[i, position.Y].Type == nodeType)
        //                return i - position.X;
        //        }
        //    }
        //    else if (direction == CloneDirection.Left)
        //    {
        //        for (var i = position.X; i >= 0; i--)
        //        {
        //            if (Board.Nodes[i, position.Y].Type == nodeType)
        //                return position.X - i;
        //        }
        //    }

        //    return -1;
        //}
    }

    public class Board
    {
        public Node[,] Nodes { get; set; }

        public Board(int width, int height, Point startPosition, Point exitPosition, ICollection<Point> elevators)
        {
            Nodes = new Node[width, height];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var node = new Node
                    {
                        Position = new Point(i, j)
                    };

                    if (node.Position == startPosition)
                        node.Type = NodeType.StartPoint;
                    else if (node.Position == exitPosition)
                        node.Type = NodeType.ExitPoint;
                    else if (elevators.Contains(node.Position))
                        node.Type = NodeType.Elevator;
                    else
                        node.Type = NodeType.Empty;

                    Nodes[i, j] = node;
                }
            }
        }
    }

    public class Test
    {
        public string BoardSettings { get; set; } = null!;
        public IEnumerable<string> Elevators { get; set; } = null!;
        public string FirstClone { get; set; } = null!;
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
    }

    public enum NodeType
    {
        Empty,
        Clone,
        Elevator,
        StartPoint,
        ExitPoint
    }

    public enum CloneAction
    {
        Wait,
        Block,
        Elevator,
    }

    public class Clone
    {
        public Point Position { get; set; }
        public CloneDirection CloneDirection { get; set; }

        public Clone()
        {
        }

        public Clone(string input)
        {
            var inputs = input.Split(' ');
            Position = new Point(Convert.ToInt32(inputs[1]), Convert.ToInt32(inputs[0]));

            switch (inputs[2])
            {
                case "LEFT":
                    CloneDirection = CloneDirection.Left;
                    break;

                case "RIGHT":
                    CloneDirection = CloneDirection.Right;
                    break;

                case "NONE":
                    CloneDirection = CloneDirection.None;
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "Clone is at {0} going to {1}",
                Position,
                CloneDirection);
        }
    }

    public enum CloneDirection
    {
        None,
        Left,
        Right
    }
}