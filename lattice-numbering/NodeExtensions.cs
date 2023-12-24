namespace LatticeNumbering;

public static class NodeExtensions
{
    public static bool IsFirstNode(this Node node) => node.ConnectedNodes.Count == 1;
    
    public static bool IsCornerNode(this Node node) => node.ConnectedNodes.Count == 2;
    
    public static bool IsMiddleNode(this Node node) => node.ConnectedNodes.Count == 4;

    // TODO change this to remaining nodes array
    public static bool IsRouteComplete(this Node[] nodes)
    {
        foreach (var node in nodes)
        {
            // If all nodes have been visited then the route is complete
            if (!node.IsVisited)
                return false;
        }
        return true;
    }
    
    public static void ConnectToPreviousNodes(this Node node, Node[] nodes, int n)
    {
        // If a node exists above this one then connect to it
        if (node.Id - n >= 0)
        {
            node.BuildConnection(nodes[node.Id - n]);
        }

        // If a node exists to the left of this one then connect to it
        // Do not connect if this node is on the left edge (multiple of n)
        if (node.Id - 1 >= 0 && node.Id % n != 0)
        {
            // Do not connect the second node to the first
            // Assume any routes taken by starting downwards from the first node can be mirrored onto a route starting by moving right
            if (node.Id != 1)
            {
                node.BuildConnection(nodes[node.Id - 1]);
            }
        }
    }

    private static void BuildConnection(this Node nodeFrom, Node nodeTo)
    {
        nodeFrom.ConnectedNodes.Add(nodeTo);
        nodeTo.ConnectedNodes.Add(nodeFrom);
    }
}