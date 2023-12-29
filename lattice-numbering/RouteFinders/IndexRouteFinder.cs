using LatticeNumbering.Extensions;

namespace LatticeNumbering.RouteFinders;

public class IndexRouteFinder : IRouteFinder
{
    private readonly int _n;
    
    private bool[] _squares = null!;
    private bool[] _validEndSquares = null!;

    private int[] _cornerSquares = null!;
    private readonly Dictionary<int, int[]> _connections = new();

    private int _remainingCount;
    private int _validEndCount;

    private int _validEndRemainingOptimisationCount;
    private int _deadEndOptimisationCount;
    private int _inaccessibleRouteOptimisationCount;
    private int _inaccessibleCornerOptimisationCount;

    public IndexRouteFinder(int n)
    {
        _n = n;
    }
    
    public int Run()
    {
        // Total number of squares, and of those the number of middle squares, that are available across the entire grid
        _remainingCount = _n * _n;
        
        _squares = new bool[_remainingCount];
        
        _validEndSquares = new bool[_remainingCount];

        // Exclude origin corner
        _cornerSquares = new[]{_n - 1, _remainingCount - _n, _remainingCount - 1};
        
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
        Console.WriteLine($"Valid end remaining optimisation: {_validEndRemainingOptimisationCount}");
        Console.WriteLine($"Dead end optimisation: {_deadEndOptimisationCount}");
        Console.WriteLine("Optimisation methods aborted the following number of routes");
        Console.WriteLine($"Inaccessible square optimisation: {_inaccessibleRouteOptimisationCount}");
        Console.WriteLine($"Inaccessible corner optimisation: {_inaccessibleCornerOptimisationCount}");
        
        return cornerRouteCount * 8;
    }

    // Once a third of the squares have been visited check at a low frequency
    // Once two thirds of the squares have been visited check at a high frequency
    private bool IsOptimisationThresholdMet() =>
        _remainingCount < 2 * _n / 3 || (_remainingCount < 2 * _n / 3 && _remainingCount % 2 == 0);
    
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
                var nextSquares = GetNextSquares(thisIndex);
                // Route has hit a dead end
                if (nextSquares.Count == 0)
                {
                    _deadEndOptimisationCount++;
                }
                // Scan the grid for any squares that have been made inaccessible (no connections available)
                // This scan is expensive so only run when a threshold has been met
                else if (IsOptimisationThresholdMet() && !AreRemainingSquaresAccessible())
                {
                    _inaccessibleRouteOptimisationCount++;
                }
                // Visiting an edge square carries the risk of isolating a corner square - verify that this is not the case
                else if (thisIndex.IsEdgeSquare(_n) && !thisIndex.IsCornerSquare(_n) && !AreCornersAccessible(thisIndex))
                {
                    _inaccessibleCornerOptimisationCount++;
                }
                else
                {
                    // Update grid metrics
                    SetSquareLock(thisIndex, true);

                    // Investigate each node connected to this one that has not already been visited by this route
                    foreach (var nextNode in nextSquares)
                    {
                        //if(_remainingCount < _d || VerifyRouteAhead(nextNode, _d))
                        count += MoveToSquare(nextNode);
                    }   
                
                    // Allow this node to be visited by other routes
                    SetSquareLock(thisIndex, false);
                }
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

    private bool AreRemainingSquaresAccessible()
    {
        for (int i = 0; i < _squares.Length; i++)
        {
            // Find any squares that haven't been visited yet
            if (!_squares[i])
            {
                // No connections to this square - the route has made a square inaccessible
                if (GetNextSquares(i).Count == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool AreCornersAccessible(int currentIndex)
    {
        // All corners must have two connections available
        foreach (var corner in _cornerSquares)
        {
            if (_squares[corner]) continue;
            
            var cornerConnections = GetNextSquares(corner);
            if (cornerConnections.Count == 0)
                return false;

            if (cornerConnections.Count == 1 && cornerConnections.First() != currentIndex)
                return false;
        }

        return true;
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