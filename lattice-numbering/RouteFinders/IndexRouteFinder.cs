using LatticeNumbering.Extensions;

namespace LatticeNumbering.RouteFinders;

public class IndexRouteFinder : IRouteFinder
{
    private readonly int _n;
    private readonly int _d;
    
    private bool[] _squares = null!;
    private bool[] _validEndSquares = null!;

    private Dictionary<int, int[]> _connections = new();

    private int _remainingCount;
    private int _validEndCount;

    private int _breakingMoveOptimisationCount;
    private int _validEndRemainingOptimisationCount;
    
    public IndexRouteFinder(int n, int d)
    {
        _n = n;
        _d = d;
    }
    
    public int Run()
    {
        // Total number of squares, and of those the number of middle squares, that are available across the entire grid
        _remainingCount = _n * _n;
        
        _squares = new bool[_remainingCount];
        
        _validEndSquares = new bool[_remainingCount];
        
        for (var i = 0; i < _remainingCount; i++)
        {
            // Set squares as not visited by default
            _squares[i] = false;
            
            // Get all possible connections for the square
            _connections[i] = GetConnections(i);
            
            if (i.IsMiddleSquare(_n) && i.IsEndSquare(_n))
            {
                _validEndSquares[i] = true;
                _validEndCount++;
            }
            else
            {
                _validEndSquares[i] = false;
            }
        }

        Console.WriteLine($"There are {_validEndCount} squares that it is possible for a valid route to end on");
        
        // Only going one direction from the origin corner (downwards, right is disconnected)
        var cornerRouteCount = MoveToSquare(0);
        Console.WriteLine("Optimisation methods aborted the following number of routes");
        Console.WriteLine($"Valid end remaining optimisation: {_validEndRemainingOptimisationCount}");
        Console.WriteLine($"Breaking move optimisation: {_breakingMoveOptimisationCount}");
        
        return cornerRouteCount * 8;
    }
    
    private int MoveToSquare(int thisIndex)
    {
        var count = 0;
        
        // Determine whether there are any more squares remaining to visit
        if (_remainingCount > 1)
        {
            if (_validEndCount == 0)
            {
                // No more valid end squares remaining
                _validEndRemainingOptimisationCount++;
            }
            else
            {
                // Update grid metrics
                SetSquareLock(thisIndex, true);

                // Investigate each node connected to this one that has not already been visited by this route
                foreach (var nextNode in GetNextSquares(thisIndex))
                {
                    //if(_remainingCount < _d || VerifyRouteAhead(nextNode, _d))
                    count += MoveToSquare(nextNode);
                }   
                
                // Allow this node to be visited by other routes
                SetSquareLock(thisIndex, false);
            }
        }
        else
        {
            // If this node ends on a middle square then it is a valid route
            if(_validEndSquares[thisIndex])
                count++;
        }
        
        return count;
    }

    private void SetSquareLock(int index, bool isLocked)
    {
        if (isLocked)
        {
            _squares[index] = true;
            _remainingCount--;
            
            if (_validEndSquares[index])
            {
                _validEndCount--;
            }
        }
        else
        {
            _squares[index] = false;
            _remainingCount++;

            if (_validEndSquares[index])
            {
                _validEndCount++;
            }
        }
    }
    
    // TODO rethink this using 'net cast' method versus iterating through connected nodes
    // Verify a square has at least one available route remaining
    private bool VerifyRouteAhead(int index, int remainingDepth)
    {
        if (remainingDepth <= 0)
            return true;
        
        SetSquareLock(index, true);

        // Determine whether there are any more squares remaining to visit
        if (_remainingCount > 0)
        {
            // If this move is a dead end then the route is invalid
            var nextSquares = GetNextSquares(index);
            if (nextSquares.Count == 0)
            {
                _breakingMoveOptimisationCount++;
                SetSquareLock(index, false);
                return false;
            }

            // Verify whether there are any possible future routes that make this route worth exploring
            var isFutureRoutePossible = false;
            foreach (var nextSquare in nextSquares)
            {
                if(!isFutureRoutePossible)
                    isFutureRoutePossible = isFutureRoutePossible || VerifyRouteAhead(nextSquare, remainingDepth - 1);
            }
            SetSquareLock(index, false);
            return isFutureRoutePossible;
        }
        
        // Allow this node to be visited by other routes
        SetSquareLock(index, false);
        return true;
    }

    private int[] GetConnections(int index)
    {
        var connections = new List<int>();
        
        // Is there is a square above this one
        if(!index.IsTopEdge(_n))
            connections.Add(index - _n);
        
        // Is there is a square below this one
        if(!index.IsBottomEdge(_n))
            connections.Add(index + _n);
        
        // Is there is a square to the left of this one
        if(!index.IsLeftEdge(_n))
            connections.Add(index - 1);
        
        // Is there is a square to the right of this one
        // Exclude right link for index 0 (all routes taken via the square below index 0 will be mirrored)
        if(!index.IsRightEdge(_n) && index != 0)
            connections.Add(index + 1);

        return connections.ToArray();
    }
    
    private List<int> GetNextSquares(int index)
    {
        var availableSquares = new List<int>();

        // Get all connected indexes that haven't been visited yet
        foreach (var connectedIndex in _connections[index])
        {
            if(!_squares[connectedIndex])
                availableSquares.Add(connectedIndex);
        }
        
        return availableSquares;
    }
}