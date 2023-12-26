using LatticeNumbering.Extensions;

namespace LatticeNumbering.RouteFinders;

public class IndexRouteFinder : IRouteFinder
{
    private readonly int _n;
    
    private bool[] _squares = null!;
    private int[] _verificationSquareIndices = null!;
    
    private int _remainingCount;
    private int _middleCount;

    private int _skippedMiddleCount = 0;
    private int _skippedSampleCount = 0;
    
    public IndexRouteFinder(int n)
    {
        _n = n;
    }
    
    public int Run()
    {
        // Total number of squares, and of those middle squares, are available across the entire grid
        _remainingCount = _n * _n;
        _middleCount = (_n - 2) * (_n - 2);
        
        _squares = new bool[_remainingCount];
        Array.Fill(_squares, false);
        
        // Get indices of corner squares (excluding the origin corner)
        _verificationSquareIndices = new []{_n - 1, _remainingCount - _n, _remainingCount - 1};

        // Only going one direction from the origin corner (downwards, right is disconnected)
        var cornerRouteCount = VisitNode(0);
        Console.WriteLine("Optimisation method aborted the following number of routes");
        Console.WriteLine($"Middle square optimisation: {_skippedMiddleCount}");
        Console.WriteLine($"Sample square optimisation: {_skippedSampleCount}");
        
        return cornerRouteCount * 8;
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
            if (IsRoutePossible())
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

    private bool IsRoutePossible()
    {
        // Determine whether there are any more middle squares remaining to visit
        // If there are none remaining then the route is already impossible
        if (_middleCount == 0)
        {
            _skippedMiddleCount++;
            return false;
        }

        // Verify that all the squares in the sample pool are still accessible
        foreach (var verificationSquareIndex in _verificationSquareIndices)
        {
            if (!VerifySquare(verificationSquareIndex))
                return false;
        }

        return true;
    }

    // Return true if the square is 'valid'
    // Valid = is already visited, or has at least one available route
    private bool VerifySquare(int index)
    {
        // Square is considered valid if it has already been visited
        if (_squares[index])
            return true;

        _squares[index] = true;

        var nextInRoute = GetNextSquares(index);

        var isPossible = nextInRoute.Count > 0;

        if (!isPossible)
            _skippedSampleCount++;
        
        // TODO Recursively run VerifySquare here to verify that the next square is accessible to (to d depth)

        _squares[index] = false;

        return isPossible;
    }
    
    private List<int> GetNextSquares(int index)
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