using LatticeNumbering.Extensions;
using LatticeNumbering.Models;

namespace LatticeNumbering.RouteFinders;

public class NodeRouteFinder : IRouteFinder
{
    private readonly int _n;
    private Node[] _nodes = null!;

    public NodeRouteFinder(int n)
    {
        _n = n;
    }

    public int Run()
    {
        CreateNodes();
            
        // Verify nodes are set up as expected
        if(!_nodes[_n - 1].IsCornerNode())
            throw new InvalidDataException("The node array has not been generated as expected");

        var firstNode = _nodes[0];
        if (!firstNode.IsFirstNode())
            throw new InvalidDataException("The node array has not been generated as expected");
            
        return VisitNode(firstNode) * 8;
    }


    private void CreateNodes()
    {
        var length = _n * _n;
        _nodes = new Node[length];
            
        for (var i = 0; i < length; i++)
        {
            var newNode = new Node(i);
            _nodes[i] = newNode;

            newNode.ConnectToPreviousNodes(_nodes, _n);
        }
    }

    private int VisitNode(Node thisNode)
    {
        thisNode.IsVisited = true;

        var count = 0;
            
        // Determine whether this node completes a route
        if (_nodes.IsRouteComplete())
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