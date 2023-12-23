namespace LatticeNumbering;

public class Node
{
    public readonly int Id;

    public readonly List<Node> ConnectedNodes = new();

    public bool IsVisited = false;

    public Node(int id)
    {
        Id = id;
    }
}