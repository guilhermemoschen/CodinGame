namespace Codingame.ThereIsNoSpoon24
{
    public class Program
    {
        static void Main(string[] args)
        {
            var game = new Game(args);
        }
    }

    public class Game
    {
        public Board Board { get; set; }
        public List<List<Node>> PossiblePaths { get; set; }
        public List<Node> Solution { get; set; }

        public Game(string[] args)
        {
            if (args.Length == 0)
            {
                Board = CreateBoard(null);
                Board.Print();
                CreateSolution();
                if (Board.IsFinish())
                    PrintSolution();
                else
                    Console.Error.WriteLine("Could NOT Solve");
                return;
            }

            Console.Error.WriteLine("Solving Intermediate1");
            Board = CreateBoard(Board.Intermediate1);
            Board.Print();
            CreateSolution();
            if (Board.IsFinish())
                Console.Error.WriteLine("Could solve");
            else
                Console.Error.WriteLine("Could NOT Solve");

            Console.Error.WriteLine("Solving Intermediate2");
            Board = CreateBoard(Board.Intermediate2);
            Board.Print();
            CreateSolution();
            if (Board.IsFinish())
                Console.Error.WriteLine("Could solve");
            else
                Console.Error.WriteLine("Could NOT Solve");

            Console.Error.WriteLine("Solving Intermediate3");
            Board = CreateBoard(Board.Intermediate3);
            Board.Print();
            CreateSolution();
            if (Board.IsFinish())
                Console.Error.WriteLine("Could solve");
            else
                Console.Error.WriteLine("Could NOT Solve");

            Console.Error.WriteLine("Solving MultipleSolutions2");
            Board = CreateBoard(Board.MultipleSolutions2);
            Board.Print();
            CreateSolution();
            if (Board.IsFinish())
                Console.Error.WriteLine("Could solve");
            else
                Console.Error.WriteLine("Could NOT Solve");

            Console.Error.WriteLine("Solving Exper");
            Board = CreateBoard(Board.Expert);
            Board.Print();
            CreateSolution();
            if (Board.IsFinish())
                Console.Error.WriteLine("Could solve");
            else
                Console.Error.WriteLine("Could NOT Solve");
        }

        private void CreateSolution()
        {
            var tickets = Environment.TickCount;
            GenerateSolutionByAssumptions(Board);

            if (!Board.IsFinish())
            {
                Console.Error.WriteLine("Creating Parcial Connections");

                while (FillMinimumConnections(Board))
                {
                    while (DefineBasicConnections(Board)) {}
                }

                if (!Board.IsFinish())
                {
                    Console.Error.WriteLine("Couldnt find the solution");
                    Console.Error.WriteLine("Trying Randomic solution");
                    var board = CreateRandomSolution();
                    if (board != null && board.IsFinish())
                    {
                        Board = board;
                    }
                    else
                    {
                        ConnectRemainingNodes();
                        while (Board.NeedMoreConnections() && IncreaseLinks()) { }
                    }
                }
                //var node = Board.GetNode(4, 17);
                //var a = node.KnowWhatToConnect;



                //while (ConnectNodesByChainRule())
                //{
                //    GenerateSolutionByAssumptions();
                //}
            }
            return;

            if (!Board.IsFinish())
            {
                Console.Error.WriteLine("Trying to connect everything, possible multiple solutions");
                ConnectRemainingNodes();
                while (Board.NeedMoreConnections() && IncreaseLinks()) { }
            }

            Console.Error.WriteLine("GenerateSolutionByAssumptions {0:mm:ss.fff}", new DateTime((Environment.TickCount - tickets) * 10000));

            return;



            var firstNode = Board.Nodes
                .OrderBy(x => x.RequestedLinks)
                .ThenBy(x => x.PossibleConnections)
                .FirstOrDefault();

            tickets = Environment.TickCount;
            var path = GeneratePath(firstNode, null, new List<Node>());
            Console.Error.WriteLine("GeneratePath Time {0:mm:ss.fff}", new DateTime((Environment.TickCount - tickets) * 10000));
            tickets = Environment.TickCount;

            GetAllPossiblePaths(path, new List<Node>());
            Console.Error.WriteLine("GetAllPossiblePaths Time {0:mm:ss.fff}", new DateTime((Environment.TickCount - tickets) * 10000));
        }

        private Board CreateRandomSolution()
        {
            foreach (var node in Board.UnfinishedNodes)
            {
                var newBoard = Board.Copy();
                var newNode = newBoard.GetNode(node.X, node.Y);

                var nodesToConnect = newNode.GetAvailableNodesToConnect();
                if (!nodesToConnect.Any())
                    continue;

                Console.Error.WriteLine("Random decision");
                newNode.AddConnection(nodesToConnect.FirstOrDefault(), Connection.MinLinkPerConnection);

                while (FillMinimumConnections(newBoard))
                {
                    while (DefineBasicConnections(newBoard)) { }
                }
                if (newBoard.IsFinish())
                    return newBoard;
            }

            return null;
        }

        private bool FillMinimumConnections(Board board)
        {
            foreach (var remainingNode in board.UnfinishedNodes.Where(x => x.CanConnectInSomething).ToList())
            {
                var availableNodesToConnect = remainingNode.GetAvailableNodesToConnect();
                foreach (var nodeToConnect in availableNodesToConnect)
                {
                    remainingNode.AddConnection(nodeToConnect, Connection.MinLinkPerConnection);
                }
                return true;
            }

            return false;
        }

        private bool ConnectNodesByChainRule()
        {
            var remainingNodes = Board.UnfinishedNodes.Where(x => !x.Connections.Any() && x.GetAvailableNodesToConnect().Count == 2 && x.AvailableLinks == 2 && x.RequestedLinks == 2);
            var addedAnyConnection = false;

            foreach (var node in remainingNodes)
            {
                var nodesToConnect = node.GetAvailableNodesToConnect();
                var couldCutAConnection = false;

                foreach (var nodeToConnect in nodesToConnect)
                {
                    if (Board.WillCutAnyFutureConnection(node, nodeToConnect))
                        couldCutAConnection = true;
                }

                if (couldCutAConnection)
                    continue;

                foreach (var nodeToConnect in nodesToConnect)
                {
                    node.AddConnection(nodeToConnect, Connection.MinLinkPerConnection);
                }

                addedAnyConnection = true;
            }

            return addedAnyConnection;
        }

        private void ConnectRemainingNodes()
        {
            var nodes = Board.Nodes.Where(x => x.NeedMoreLinks).OrderByDescending(x => x.RequestedLinks).ToList();
            foreach (var node in nodes)
            {
                if (!node.NeedMoreLinks)
                    continue;

                var possibleNodesToConnect = node.GetNotYetConnectedNodes().OrderByDescending(x => x.RequestedLinks).ToList();
                foreach (var nodeToConnect in possibleNodesToConnect)
                {
                    if (!nodeToConnect.NeedMoreLinks)
                        continue;
                    node.AddConnection(nodeToConnect, Connection.MinLinkPerConnection);
                }
            }
        }

        public void GenerateSolutionByAssumptions(Board board)
        {
            while (board.NeedMoreConnections() && DefineBasicConnections(board)) { }
        }

        private bool IsPathValid(List<Node> possiblePath)
        {
            if (!CreateLinks(possiblePath))
            {
                Board.ClearAllConnections();
                return false;
            }


            if (possiblePath.Any(x => x.AvailableLinks < 0))
            {
                Board.ClearAllConnections();
                return false;
            }


            if (possiblePath.Count != Board.Nodes.Count)
                return false;

            while (Board.NeedMoreConnections())
            {
                DefineBasicConnections(Board);
                //continue;

                if (IncreaseLinks())
                    continue;

                if (Board.IsFinish())
                    break;

                Board.ClearAllConnections();
                return false;
            }

            return Board.IsFinish();
        }

        private bool DefineBasicConnections(Board board)
        {
            var createdConnection = false;

            var nodes = board.Nodes.Where(x => x.KnowWhatToConnect).ToList();

            foreach (var node in nodes)
            {
                if (!node.NeedMoreLinks)
                    continue;

                var possibleNodes = node.GetAvailableNodesToConnect();

                if (!possibleNodes.Any())
                {
                    foreach (var connection in node.Connections)
                    {
                        if (!node.NeedMoreLinks && !connection.To.NeedMoreLinks)
                            continue;

                        node.IncreaseLink(connection.To);
                    }
                }
                else
                {
                    foreach (var nodeToConnect in possibleNodes)
                    {
                        if (!node.NeedMoreLinks || !nodeToConnect.NeedMoreLinks)
                            continue;

                        var links = Connection.MinLinkPerConnection;
                        if (node.AvailableLinks > Connection.MinLinkPerConnection &&
                            nodeToConnect.AvailableLinks > Connection.MinLinkPerConnection)
                            links = Connection.MaxLinkPerConnection;

                        node.AddConnection(nodeToConnect, links);
                        createdConnection = true;
                    }
                }
            }

            return createdConnection;
        }

        private List<List<Node>> GetAllPossiblePaths(Node node, List<Node> usedNodes)
        {
            var allPossiblePaths = new List<List<Node>>();

            if (Solution != null || usedNodes.Any(x => x == node))
                return allPossiblePaths;

            // is leaf
            if (node.PossiblePaths.Count == 0)
            {
                if (usedNodes.All(x => x != node))
                    allPossiblePaths.Add(new List<Node>() { node });
            }
            else
            {
                var backupList = usedNodes;
                usedNodes = new List<Node>();
                usedNodes.AddRange(backupList);
                usedNodes.Add(node);

                foreach (var multiPaths in node.PossiblePaths)
                {
                    if (!multiPaths.Any())
                        continue;

                    // is single try
                    if (multiPaths.Count == 1)
                    {
                        foreach (var possiblePath in multiPaths)
                        {
                            foreach (var generatedPaths in GetAllPossiblePaths(possiblePath, usedNodes))
                            {
                                if (generatedPaths.Any())
                                {
                                    if (!AddPossibleSolution(usedNodes, generatedPaths))
                                    {
                                        generatedPaths.Insert(0, node);
                                        allPossiblePaths.Add(generatedPaths);
                                    }
                                    else
                                        return allPossiblePaths;
                                }
                            }
                        }
                        continue;
                    }

                    if (multiPaths.Count > 2)
                    {
                        continue;
                    }

                    var possibleMultiPaths1 = GetAllPossiblePaths(multiPaths[0], usedNodes);
                    var possibleMultiPaths2 = GetAllPossiblePaths(multiPaths[1], usedNodes);

                    if (!possibleMultiPaths1.Any() || !possibleMultiPaths2.Any())
                        continue;

                    foreach (var multiPath1 in possibleMultiPaths1)
                    {
                        foreach (var multiPath2 in possibleMultiPaths2)
                        {
                            var mergedPath = new List<Node>();
                            mergedPath.AddRange(multiPath1);
                            mergedPath.AddRange(multiPath2);

                            if (!AddPossibleSolution(usedNodes, mergedPath))
                            {
                                mergedPath.Insert(0, node);
                                allPossiblePaths.Add(mergedPath);
                            }
                            else
                                return allPossiblePaths;
                        }
                    }
                }
            }

            return allPossiblePaths;
        }

        private bool AddPossibleSolution(List<Node> usedNodes, List<Node> mergedPath)
        {
            if (Solution != null || usedNodes.Count + mergedPath.Count != Board.Nodes.Count)
                return false;

            var list = new List<Node>();
            list.AddRange(usedNodes);
            list.AddRange(mergedPath);

            if (IsPathValid(list))
            {
                Solution = list;
                return true;
            }
            return false;
        }

        private bool CreateLinks(List<Node> linkedNodes)
        {
            for (var i = 0; i < linkedNodes.Count - 1; i++)
            {
                var currentNode = linkedNodes[i];

                if (!currentNode.NeedMoreLinks)
                    continue;

                if (currentNode.KnowWhatToConnect)
                {
                    currentNode.AddAllConnections();
                    continue;
                }

                var nextNode = linkedNodes[i + 1];

                if (currentNode.HasConnection(nextNode))
                    continue;

                if (!currentNode.CanConnectTo(nextNode))
                    currentNode = GetPreviewsConnectableNode(i, linkedNodes, nextNode);

                if (currentNode == null)
                    return false;

                currentNode.AddConnection(nextNode, 1);
            }

            return true;
        }

        private Node GetPreviewsConnectableNode(int startingIndex, List<Node> nodes, Node targetNode)
        {
            for (var i = startingIndex - 1; i >= 0; i--)
            {
                if (nodes[i].CanConnectTo(targetNode))
                    return nodes[i];
            }

            return null;
        }

        private bool IncreaseLinks()
        {
            var increasedAnyLink = false;

            var missingNodes = Board.Nodes
                .Where(x => x.NeedMoreLinks)
                .OrderBy(x => x.RequestedLinks)
                .ToList();

            foreach (var node in missingNodes)
            {
                foreach (var nodeToConnect in node.GetPossibleNodesToConnect())
                {
                    if (node.NeedMoreLinks && nodeToConnect.NeedMoreLinks)
                    {
                        var connection = node.GetConnection(nodeToConnect);
                        if (connection != null && connection.Links < Connection.MaxLinkPerConnection)
                        {
                            node.IncreaseLink(nodeToConnect);
                            increasedAnyLink = true;
                        }
                    }
                }
            }
            return increasedAnyLink;
        }

        private Node GeneratePath(Node currentNode, Node previewsNode, List<Node> usedNodes)
        {
            var availableLinks = currentNode.RequestedLinks;
            if (usedNodes.Any())
                availableLinks--;

            if (availableLinks == 0)
            {
                return currentNode;
            }

            if (previewsNode == null)
                previewsNode = currentNode;

            if (!usedNodes.Contains(currentNode))
            {
                usedNodes.Add(currentNode);
            }

            var backupList = usedNodes;
            usedNodes = new List<Node>();
            usedNodes.AddRange(backupList);

            var possiblesNodesToConnect = currentNode.GetPossibleNodesToConnect().Where(x => x != previewsNode).ToList();

            // try individual
            for (var i = 0; i < possiblesNodesToConnect.Count; i++)
            {
                if (usedNodes.Contains(possiblesNodesToConnect[i]))
                    continue;

                usedNodes.Add(possiblesNodesToConnect[i]);

                var paths = new List<Node>() { GeneratePath(possiblesNodesToConnect[i], currentNode, usedNodes) };
                currentNode.PossiblePaths.Add(paths);
                usedNodes.Remove(possiblesNodesToConnect[i]);
            }

            if (possiblesNodesToConnect.Count == 1 || availableLinks == 1)
                return currentNode;

            var multiplePath = new List<Node>();

            if (currentNode.RequestedLinks - 1 >= possiblesNodesToConnect.Count && possiblesNodesToConnect.Count > 1)
            {
                // try all groups at once
                for (var i = 0; i < possiblesNodesToConnect.Count; i++)
                {
                    if (usedNodes.Contains(possiblesNodesToConnect[i]))
                        continue;

                    usedNodes.Add(possiblesNodesToConnect[i]);
                    multiplePath.Add(GeneratePath(possiblesNodesToConnect[i], currentNode, usedNodes));

                }

                for (var i = 0; i < possiblesNodesToConnect.Count; i++)
                {
                    usedNodes.Remove(possiblesNodesToConnect[i]);
                }
            }

            if (multiplePath.Count != 0)
                currentNode.PossiblePaths.Add(multiplePath);

            if (possiblesNodesToConnect.Count < 3)
                return currentNode;

            // try sub groups
            for (var i = 0; i < possiblesNodesToConnect.Count; i++)
            {
                multiplePath = new List<Node>();

                var subGroupList = new List<Node>();
                subGroupList.AddRange(possiblesNodesToConnect);
                subGroupList.Remove(possiblesNodesToConnect[i]);

                for (var j = 0; j < subGroupList.Count; j++)
                {
                    if (usedNodes.Contains(subGroupList[j]))
                        continue;

                    usedNodes.Add(subGroupList[j]);
                    multiplePath.Add(GeneratePath(subGroupList[j], currentNode, usedNodes));
                }

                for (var j = 0; j < subGroupList.Count; j++)
                {
                    usedNodes.Remove(subGroupList[j]);
                }

                currentNode.PossiblePaths.Add(multiplePath);
            }

            return currentNode;
        }

        private void PrintSolution()
        {
            var alreadyPrintedConnections = new List<Connection>();

            foreach (var currentNode in Board.Nodes)
            {
                foreach (var connection in currentNode.Connections)
                {
                    var hasPrintedForward =
                        alreadyPrintedConnections.Any(x => x.From == connection.From && x.To == connection.To);

                    var hasPrintedBackward =
                        alreadyPrintedConnections.Any(x => x.To == connection.From && x.From == connection.To);

                    if (!hasPrintedForward && !hasPrintedBackward)
                    {
                        Console.WriteLine(GenerateOutput(connection));
                        alreadyPrintedConnections.Add(connection);
                    }
                }
            }
        }

        private Board CreateBoard(IList<string> boardAsString)
        {
            int width;
            int height;

            if (boardAsString == null)
            {
                width = int.Parse(Console.ReadLine()); // the number of cells on the X axis
                height = int.Parse(Console.ReadLine()); // the number of cells on the Y axis

                boardAsString = new List<string>();

                for (var i = 0; i < height; i++)
                    boardAsString.Add(Console.ReadLine());
            }
            else
            {
                width = boardAsString[0].Length;
                height = boardAsString.Count;
            }

            return new Board(width, height, boardAsString);
        }

        private string GenerateOutput(Connection connection)
        {
            return string.Format("{0} {1} {2} {3} {4}",
                connection.From.X,
                connection.From.Y,
                connection.To.X,
                connection.To.Y,
                connection.Links);
        }
    }

    public class Node
    {
        public Board Board { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int RequestedLinks { get; set; }
        public int PossibleConnections { get; private set; }
        public List<Connection> Connections { get; set; }
        public List<List<Node>> PossiblePaths { get; set; }

        public int AvailableConnections
        {
            get
            {
                return PossibleConnections - Connections.Count;
            }
        }

        public bool NeedMoreLinks
        {
            get { return AvailableLinks > 0; }
        }

        public int AvailableLinks
        {
            get { return RequestedLinks - Connections.Sum(x => x.Links); }
        }

        public bool CanConnectInSomething
        {
            get
            {
                if (!NeedMoreLinks)
                    return false;

                var linksFromExistentConnections = AvailanbleLinksFromExistentConnections;
                var linksFromNewConnections = 0;

                var availableNodesToConnect = GetAvailableNodesToConnect();
                foreach (var nodeToConnect in availableNodesToConnect)
                {
                    linksFromNewConnections += nodeToConnect.AvailableLinks > Connection.MinLinkPerConnection
                        ? Connection.MaxLinkPerConnection
                        : Connection.MinLinkPerConnection;
                }

                if (AvailableLinks == 1)
                {
                    if (linksFromNewConnections + linksFromExistentConnections == 1)
                        return true;
                }
                else if (AvailableLinks == 2)
                {
                    if (linksFromNewConnections + linksFromExistentConnections == 2)
                        return true;
                } 
                else if (AvailableLinks == linksFromNewConnections + linksFromExistentConnections - 1)
                {
                    return true;
                }

                return false;
            }
        }

        public bool KnowWhatToConnect
        {
            get
            {
                if (!NeedMoreLinks)
                    return false;

                if (RequestedLinks == 4 && PossibleConnections == 2 ||
                    RequestedLinks == 6 && PossibleConnections == 3 ||
                    RequestedLinks == 8 && PossibleConnections == 4)
                    return true;

                var nodesToConnect = GetAvailableNodesToConnect();

                if (nodesToConnect.Count == 1 && !Connections.Any())
                    return true;

                var linksFromExistentConnections = AvailanbleLinksFromExistentConnections;
                var linksFromNewConnections = 0;

                foreach (var node in nodesToConnect)
                {
                    linksFromNewConnections += node.AvailableLinks > Connection.MinLinkPerConnection
                        ? Connection.MaxLinkPerConnection
                        : Connection.MinLinkPerConnection;
                }


                if (linksFromExistentConnections == 0)
                {
                    if (nodesToConnect.Count == 1)
                    {
                        return true;
                    }
                    return AvailableLinks == linksFromNewConnections;
                }
                
                if (linksFromNewConnections == 0)
                {
                    return AvailableLinks == linksFromExistentConnections;
                }

                return AvailableLinks == linksFromExistentConnections + linksFromNewConnections;
                
            }
        }

        public int AvailanbleLinksFromExistentConnections
        {
            get
            {
                return Connections.Count(x => x.Links == Connection.MinLinkPerConnection && x.To.NeedMoreLinks);
            }
        }

        public static bool operator ==(Node left, Node right)
        {
            if (Equals(left, null) && Equals(right, null))
                return true;

            if (Equals(left, null) || Equals(right, null))
                return false;

            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Node left, Node right)
        {
            if (Equals(left, null) && Equals(right, null))
                return false;

            if (Equals(left, null) || Equals(right, null))
                return true;

            return !(left == right);
        }

        public Node()
        {
            Connections = new List<Connection>();
        }

        public bool HasConnection(Node targetNode)
        {
            return Connections.Any(x => x.To == targetNode);
        }

        public void AddConnection(Node targetNode, int links)
        {
            if (Connections.All(x => x.To != targetNode))
            {
                var forwardConnection = new Connection
                {
                    From = this,
                    To = targetNode,
                    Links = links
                };
                Connections.Add(forwardConnection);

                var backwardConnection = new Connection()
                {
                    From = targetNode,
                    To = this,
                    Links = links,
                };

                targetNode.AddConnection(backwardConnection);

                Console.Error.WriteLine("New Connection {0} {1} {2} {3} {4}",
                    forwardConnection.From.X,
                    forwardConnection.From.Y,
                    forwardConnection.To.X,
                    forwardConnection.To.Y,
                    forwardConnection.Links);
            }
        }

        public void AddConnection(Connection connection)
        {
            if (Connections.All(x => x.To != connection.From))
            {
                Connections.Add(connection);
            }
        }

        public Connection GetConnection(Node targetNode)
        {
            return Connections.FirstOrDefault(x => x.To == targetNode);
        }

        public void UpdatePossibleConnections()
        {
            if (CanConnectToRight())
                PossibleConnections++;
            if (CanConnectToBotton())
                PossibleConnections++;
            if (CanConnectToTop())
                PossibleConnections++;
            if (CanConnectToLeft())
                PossibleConnections++;
        }

        public void IncreaseLink(Node targetNode)
        {
            var forwardConnection = GetConnection(targetNode);
            forwardConnection.Links++;
            var backwardConnection = targetNode.GetConnection(this);
            backwardConnection.Links++;

            //Console.Error.WriteLine("Increased {0} to {1} Link {2}",
            //    forwardConnection.From.Position,
            //    forwardConnection.To.Position,
            //    forwardConnection.Links);
        }

        public void AddAllConnections()
        {
            if (!KnowWhatToConnect)
                return;

            foreach (var nodeToConnect in GetPossibleNodesToConnect())
            {
                var links = 1;
                if (AvailableLinks > 1 && nodeToConnect.AvailableLinks > 1)
                    links = Connection.MaxLinkPerConnection;

                AddConnection(nodeToConnect, links);
            }
        }

        public List<Node> GetAvailableNodesToConnect()
        {
            return GetNotYetConnectedNodes()
                .Where(x => NeedMoreLinks && x.NeedMoreLinks && !Board.WillCutAnyConnection(this, x) && !x.ItWillSepareteGroups(this))
                .ToList();
        }

        public List<Node> GetNotYetConnectedNodes()
        {
            var possiblesNodes = GetPossibleNodesToConnect();

            foreach (var connection in Connections)
            {
                var existentConnectedNode = possiblesNodes.FirstOrDefault(x => x == connection.To);
                if (existentConnectedNode != null)
                {
                    possiblesNodes.Remove(existentConnectedNode);
                }
            }

            return possiblesNodes;
        }

        public List<Node> GetPossibleNodesToConnect()
        {
            var nodes = new List<Node>();

            if (CanConnectToRight())
                nodes.Add(GetConnectionToRight());

            if (CanConnectToBotton())
                nodes.Add(GetConnectionToBotton());

            if (CanConnectToLeft())
                nodes.Add(GetConnectionToLeft());

            if (CanConnectToTop())
                nodes.Add(GetConnectionToTop());

            if (nodes.Any(x => x == null))
            {

            }

            return nodes;
        }

        public bool CanConnectTo(Node node)
        {
            if (HasConnection(node))
                return false;

            return GetPossibleNodesToConnect().Any(x => x == node);
        }

        private bool CanConnectToRight()
        {
            return X < Board.GetNodesByY(Y).Max(x => x.X);
        }

        private bool CanConnectToLeft()
        {
            return X > Board.GetNodesByY(Y).Min(x => x.X);
        }

        private bool CanConnectToBotton()
        {
            return Y < Board.GetNodesByX(X).Max(x => x.Y);
        }

        private bool CanConnectToTop()
        {
            return Y > Board.GetNodesByX(X).Min(x => x.Y);
        }

        public Node GetConnectionToRight()
        {
            return Board.GetNodesByY(Y)
                .OrderBy(x => x.X)
                .FirstOrDefault(x => x.X > X);
        }

        public Node GetConnectionToLeft()
        {
            return Board.GetNodesByY(Y)
                .OrderByDescending(x => x.X)
                .FirstOrDefault(x => x.X < X);
        }

        public Node GetConnectionToBotton()
        {
            return Board.GetNodesByX(X)
                .OrderBy(x => x.Y)
                .FirstOrDefault(x => x.Y > Y);
        }

        public Node GetConnectionToTop()
        {
            return Board.GetNodesByX(X)
                .OrderByDescending(x => x.Y)
                .FirstOrDefault(x => x.Y < Y);
        }

        public bool ItWillSepareteGroups(Node node)
        {
            var nodeAGroupLinks = SumAvailableLinksForAllConnections();
            var nodeBGroupLinks = node.SumAvailableLinksForAllConnections();
            var nodeACount = SumConnectedNodes();
            var nodeBCount = node.SumConnectedNodes();

            return nodeAGroupLinks == 1 && nodeBGroupLinks == 1 && nodeACount + nodeBCount < Board.Nodes.Count;
        }

        public int SumAvailableLinksForAllConnections(List<Node> usedNodes = null)
        {
            if (usedNodes == null)
                usedNodes = new List<Node>();

            if (usedNodes.Any(x => x == this))
                return 0;

            usedNodes.Add(this);

            var links = AvailableLinks;
            links += Connections.Select(x => x.To).Sum(x => x.SumAvailableLinksForAllConnections(usedNodes));
            return links;
        }

        public int SumConnectedNodes(List<Node> usedNodes = null)
        {
            if (usedNodes == null)
                usedNodes = new List<Node>();

            if (usedNodes.Any(x => x == this))
                return 0;

            usedNodes.Add(this);

            return 1 + Connections.Select(x => x.To).Sum(x => x.SumConnectedNodes(usedNodes));
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {1} Requested {2} Available {3} Possible {4} KnowWhatToConnect {5} CanConnectInSomething {6}",
                X,
                Y,
                RequestedLinks,
                AvailableLinks,
                PossibleConnections,
                KnowWhatToConnect,
                CanConnectInSomething);
        }
    }

    public class Connection
    {
        public const int MaxLinkPerConnection = 2;
        public const int MinLinkPerConnection = 1;
        public Node From { get; set; }
        public Node To { get; set; }
        public int Links { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0} {1} - {2} Links",
                To.X,
                To.Y,
                Links);
        }
    }

    public class Board
    {
        private readonly IList<string> boardAsString;
        
        public int Width { get; private set; }
        public int Height { get; private set; }

        public List<Node> Nodes { get; set; }

        public List<Node> UnfinishedNodes
        {
            get { return Nodes.Where(x => x.NeedMoreLinks).ToList(); }
        }

        public Board(int width, int height, IList<string> boardAsString)
        {
            Width = width;
            Height = height;
            this.boardAsString = boardAsString;
            Nodes = new List<Node>();

            for (var i = 0; i < height; i++)
            {
                var line = boardAsString[i];

                for (var j = 0; j < width; j++)
                {
                    if (line[j] == '.')
                        continue;

                    var node = new Node()
                    {
                        Board = this,
                        X = j,
                        Y = i,
                        RequestedLinks = int.Parse(line[j].ToString()),
                    };

                    Nodes.Add(node);
                }
            }

            UpdatePossibleConnectionsForAllNodes();
        }

        public List<Node> GetNodesByX(int x)
        {
            return Nodes.Where(node => node.X == x).ToList();
        }

        public List<Node> GetNodesByY(int y)
        {
            return Nodes.Where(node => node.Y == y).ToList();
        }

        public Node GetNode(int x, int y)
        {
            return Nodes.FirstOrDefault(node => node.X == x && node.Y == y);
        }

        public bool IsFinish()
        {
            return Nodes.All(x => x.AvailableLinks == 0);
        }

        public void ClearAllConnections()
        {
            foreach (var node in Nodes)
            {
                node.Connections.Clear();
            }
        }

        public bool NeedMoreConnections()
        {
            return Nodes.Any(x => x.NeedMoreLinks);
        }

        private void UpdatePossibleConnectionsForAllNodes()
        {
            foreach (var currentNode in Nodes)
            {
                currentNode.UpdatePossibleConnections();
            }
        }

        public bool WillCutAnyConnection(Node node1, Node node2)
        {
            // horizontal cut
            if (node1.X == node2.X)
            {
                Node start;
                Node end;

                if (node1.Y < node2.Y)
                {
                    start = node1;
                    end = node2;
                }
                else
                {
                    start = node2;
                    end = node1;
                }

                for (var i = start.Y + 1; i < end.Y; i++)
                {
                    foreach (var node in GetNodesByY(i).Where(x => x.X < start.X).OrderBy(x => x.X))
                    {
                        if (node.Connections.Any(x => x.To.Y == i && x.To.X > start.X))
                            return true;
                    }
                }
            }
            // vertical cut
            else if (node1.Y == node2.Y)
            {
                Node start;
                Node end;

                if (node1.X < node2.X)
                {
                    start = node1;
                    end = node2;
                }
                else
                {
                    start = node2;
                    end = node1;
                }

                for (var i = start.X + 1; i < end.X; i++)
                {
                    foreach (var node in GetNodesByX(i).Where(x => x.Y < start.Y).OrderBy(x => x.Y))
                    {
                        if (node.Connections.Any(x => x.To.X == i && x.To.Y > start.Y))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool WillCutAnyFutureConnection(Node start, Node end)
        {
            // horizontal cut
            if (start.X == end.X)
            {
                for (var i = start.Y + 1; i < end.Y; i++)
                {
                    var leftNode = GetNodesByY(i).Where(x => x.X < start.X).OrderByDescending(x => x.X).FirstOrDefault();
                    var rightNode = GetNodesByY(i).Where(x => x.X > start.X).OrderBy(x => x.X).FirstOrDefault();

                    if (leftNode == null || rightNode == null)
                        continue;

                    if (leftNode.NeedMoreLinks && rightNode.NeedMoreLinks)
                        return true;
                }
            }
            // vertical cut
            else if (start.Y == end.Y)
            {
                for (var i = start.X + 1; i < end.X; i++)
                {
                    var topNode = GetNodesByX(i).Where(x => x.Y < start.Y).OrderByDescending(x => x.Y).FirstOrDefault();
                    var bottonNode = GetNodesByX(i).Where(x => x.Y > start.Y).OrderBy(x => x.Y).FirstOrDefault();

                    if (topNode == null || bottonNode == null)
                        continue;

                    if (topNode.NeedMoreLinks && bottonNode.NeedMoreLinks)
                        return true;
                }
            }

            return false;
        }

        public void Print()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    var node = GetNode(j, i);
                    if (node == null)
                        Console.Error.Write(".");
                    else
                        Console.Error.Write(node.RequestedLinks.ToString());

                }
                Console.Error.WriteLine();
            }
        }

        public Board Copy()
        {
            var newBoard = new Board(Width, Height, boardAsString);
            foreach (var node in Nodes)
            {
                var newNode = newBoard.GetNode(node.X, node.Y);

                foreach (var connection in node.Connections)
                {
                    var newNodeToConnect = newBoard.GetNode(connection.To.X, connection.To.Y);
                    if (!newNode.HasConnection(newNodeToConnect))
                    {
                        newNode.AddConnection(newNodeToConnect, connection.Links);
                    }
                }
            }

            return newBoard;
        }

        #region Boards

        public static IList<string> Basic = new List<string>()
        {
            "14.3",
            "....",
            ".4.4",
        };

        public static IList<string> Intermediate1 = new List<string>()
        {
            "4.544",
            ".2...",
            "..5.4",
            "332.."
        };

        public static IList<string> Intermediate2 = new List<string>()
        {
            "2..2.1.",
            ".3..5.3",
            ".2.1...",
            "2...2..",
            ".1....2"
        };

        public static IList<string> Intermediate3 = new List<string>()
        {
            "25.1",
            "47.4",
            "..1.",
            "3344",
        };

        public static IList<string> CG = new List<string>()
        {
            "22221",
            "2....",
            "2....",
            "2....",
            "2....",
            "22321",
            ".....",
            ".....",
            "22321",
            "2....",
            "2....",
            "2.131",
            "2..2.",
            "2222.",
        };

        public static IList<string> C = new List<string>()
        {
            "221",
            "2..",
            "221",
        };

        public static IList<string> Simple1 = new List<string>()
        {
            ".1.",
            "131",
        };

        public static IList<string> Simple2 = new List<string>()
        {
            "131",
            ".2.",
            "131",
        };


        public static IList<string> Simple3 = new List<string>()
        {
            "1222",
            "...2",
            ".123",
            "...2",
            "...1",
        };

        public static IList<string> MultipleSolutions2 = new List<string>()
        {
            ".12..",
            ".2421",
            "24442",
            "1242.",
            "..21.",
        };

        public static IList<string> Expert = new List<string>()
        {
            "3..2.2..1....3........4",
            ".2..1....2.6.........2.",
            "..3..6....3............",
            ".......2........1..3.3.",
            "..1.............3..3...",
            ".......3..3............",
            ".3...8.....8.........3.",
            "6.5.1...........1..3...",
            "............2..6.31..2.",
            "..4..4.................",
            "5..........7...7...3.3.",
            ".2..3..3..3............",
            "......2..2...1.6...3...",
            "....2..................",
            ".4....5...3............",
            ".................2.3...",
            ".......3.3..2.44....1..",
            "3...1.3.2.3............",
            ".2.....3...6.........5.",
            "................1......",
            ".1.......3.6.2...2...4.",
            "5...............3.....3",
            "4...................4.2",
        };

        #endregion
    }
}
