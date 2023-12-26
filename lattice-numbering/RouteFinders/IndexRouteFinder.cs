using LatticeNumbering.Extensions;

namespace LatticeNumbering.RouteFinders;

public class IndexRouteFinder : IRouteFinder
{
    private readonly int _n;
    
    private bool[] _squares = null!;
    
    private int _remainingCount;
    private int _middleCount;

    public IndexRouteFinder(int n)
    {
        _n = n;
        _remainingCount = _n * _n;
        _middleCount = 4 * n - 4;
    }
    
    public int Run()
    {
        _squares = new bool[_squareCount];
        Array.Fill(_squares, false);

        return VisitNode(0) * 8;
    }
    
    private int VisitNode(int thisIndex)
    {
        _squares[thisIndex] = true;
        _remainingCount--;

        if (thisIndex.IsMiddleSquare(_n))
            _middleCount--;

        var count = 0;
            
        // Determine whether there are any more squares remaining to visit
        if (_remainingCount > 0)
        {
            // Determine whether there are any more middle squares remaining to visit
            // If there are none remaining then the route is already impossible
            if (_middleCount > 0)
            {
                // Investigate each node connected to this one that has not already been visited by this route
                foreach (var nextNode in GetNextSquares(thisIndex))
                {
                    count += VisitNode(nextNode);
                }
            }
        }
        else
        {
            // If this node ends on a middle square then it is a valid route
            if(thisIndex.IsMiddleSquare(_n))
                count++;
        }

        // Allow this node to be visited by other routes
        _squares[thisIndex] = false;
        _remainingCount++;
        _middleCount++;
        
        return count;
    }

    private IEnumerable<int> GetNextSquares(int index)
    {
        var availableSquares = new List<int>();
        
        // Is there is a square above this one that hasn't been visited yet
        if(!index.IsTopEdge(_n) && !_squares[index - _n])
            availableSquares.Add(index - _n);
        
        // Is there is a square below this one that hasn't been visited yet
        if(!index.IsBottomEdge(_n) && !_squares[index + _n])
            availableSquares.Add(index + _n);
        
        // Is there is a square to the left of this one that hasn't been visited yet
        if(!index.IsLeftEdge(_n) && !_squares[index - 1])
            availableSquares.Add(index - 1);
        
        // Is there is a square to the right of this one that hasn't been visited yet
        // Exclude right link for index 0 (all routes taken via the square below index 0 will be mirrored)
        if(!index.IsRightEdge(_n) && !_squares[index + 1] && index != 0)
            availableSquares.Add(index + 1);

        return availableSquares;
    }
}