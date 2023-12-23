using System.Diagnostics;

namespace LatticeNumbering
{
    public static class Program
    {
        private static int _n;
        private static Node[] _nodes = null!;
        private static void Main(string[] args)
        {
            if (!int.TryParse(args[0], out _n))
                throw new ArgumentException("Int value expected for n");

            if (_n < 3)
                throw new ArgumentOutOfRangeException(nameof(_n),"Value >= 3 expected for n");

            Console.WriteLine($"Finding number of valid routes for an {_n} by {_n} grid");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            CreateNodes();
            
            // Verify nodes are set up as expected
            if(!_nodes[_n - 1].IsCornerNode())
                throw new InvalidDataException("The node array has not been generated as expected");

            var firstNode = _nodes[0];
            if (!firstNode.IsFirstNode())
                throw new InvalidDataException("The node array has not been generated as expected");
            
            var totalCount = VisitNode(firstNode);
            
            stopwatch.Stop();
            
            Console.WriteLine($"{totalCount * 8} routes possible");

            Console.WriteLine($"Completed in {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)} seconds");
        }

        private static void CreateNodes()
        {
            _nodes = new Node[_n * _n];

            for (var i = 0; i < _n * _n; i++)
            {
                _nodes[i] = new Node(i);
                
                _nodes[i].ConnectToPreviousNodes(_nodes, _n);
            }
        }

        private static int VisitNode(Node thisNode)
        {
            thisNode.IsVisited = true;

            var count = 0;
            
            // Determine whether this node was the end of the route
            if (_nodes.AllNodesVisited())
            {
                // If this node ends a valid route then update the count
                if (thisNode.IsMiddleNode())
                    count++;
            }
            else
            {
                // Investigate each node connected to this one that has not already been visited by this route
                foreach (var nextNode in thisNode.ConnectedNodes)
                {
                    if(nextNode.IsVisited)
                        continue;

                    count += VisitNode(nextNode);
                }
            }

            // Allow this node to be visited by other routes
            thisNode.IsVisited = false;

            return count;
        }
    }
}