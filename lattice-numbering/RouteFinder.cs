namespace LatticeNumbering;

public class RouteFinder
{
    private readonly int _n;
    private readonly bool _verbose;
    
    private bool[] _squares = null!;
    private bool[] _validEndSquares = null!;

    private int[] _cornerSquares = null!;
    private readonly Dictionary<int, int[]> _connections = new();

    private int _remainingCount;
    private int _validEndCount;
    private int[] _remainingRowCounts = null!;
    private int[] _remainingColumnCounts = null!;

    private int _validEndRemainingOptimisationCount;
    private int _deadEndHitOptimisationCount;
    private int _inaccessibleRouteOptimisationCount;
    private int _inaccessibleCornerOptimisationCount;
    private int _deadEndCreatedOptimisationCount;
    private int _routeBlockedOptimisationCount;

    public RouteFinder(int n, bool verbose)
    {
        _n = n;
        _verbose = verbose;
    }
    
    public int Run()
    {
        // Total number of squares, and of those the number of middle squares, that are available across the entire grid
        _remainingCount = _n * _n;
        
        // Create empty arrays for grid data
        _squares = new bool[_remainingCount];
        _validEndSquares = new bool[_remainingCount];
        // Exclude origin corner
        _cornerSquares = new[]{_n - 1, _remainingCount - _n, _remainingCount - 1};
        
        // Populate grid data
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

        // Create empty arrays for row / column tracking data
        _remainingRowCounts = new int[_n];
        _remainingColumnCounts = new int[_n];
        // Populate row / column tracking data
        for (var i = 0; i < _n; i++)
        {
            _remainingRowCounts[i] = _n;
            _remainingColumnCounts[i] = _n;
        }

        if(_verbose)
            Console.WriteLine($"There are {_validEndCount} squares that it is possible for a valid route to end on");
        
        // Only going one direction from the origin corner (downwards, right is disconnected)
        var cornerRouteCount = MoveToSquare(0);

        if (_verbose)
        {
            Console.WriteLine();
            Console.WriteLine("Optimisation methods successfully aborted the following number of routes");
            Console.WriteLine($"(1) Valid end remaining: {_validEndRemainingOptimisationCount}");
            Console.WriteLine($"(2) Dead end hit: {_deadEndHitOptimisationCount}");
            Console.WriteLine($"(3) Inaccessible corner: {_inaccessibleCornerOptimisationCount}");
            Console.WriteLine($"(4) Dead end created: {_deadEndCreatedOptimisationCount}");
            Console.WriteLine($"(5) Route blocked: {_routeBlockedOptimisationCount}");
            Console.WriteLine($"(6) Inaccessible square: {_inaccessibleRouteOptimisationCount}");
        }

        return cornerRouteCount * 8;
    }

    // Once a third of the squares have been visited check at a low frequency
    // Once two thirds of the squares have been visited check at a high frequency
    private bool IsOptimisationThresholdMet() =>
        _remainingCount < 2 * _n / 3 || (_remainingCount < 2 * _n / 3 && _remainingCount % 2 == 0);
    
    private int MoveToSquare(int thisIndex)
    {
        var count = 0;
        
        if (_remainingCount > 1)
        {
            // There are more squares remaining to visit - continue with route
            var nextSquares = GetNextSquares(thisIndex);

            var isRouteValid = _verbose 
                ? IsRouteValidVerbose(thisIndex, nextSquares) 
                : IsRouteValid(thisIndex, nextSquares);
            
            if (isRouteValid)
            {
                // Update grid metrics
                SetSquareLock(thisIndex, true);

                // Investigate each node connected to this one that has not already been visited by this route
                foreach (var nextNode in nextSquares)
                {
                    count += MoveToSquare(nextNode);
                }   
            
                // Allow this node to be visited by other routes
                SetSquareLock(thisIndex, false);
            }
        }
        else
        {
            // Route has reached final square
            // If this node ends on a middle square then it is a valid route
            if(_validEndSquares[thisIndex])
                count++;
        }
        
        return count;
    }

    private bool IsRouteValidVerbose(int index, IReadOnlyCollection<int> nextSquares)
    {
        // Are there any more valid end squares remaining for the route to end on
        if (_validEndCount == 0)
        {
            _validEndRemainingOptimisationCount++;
            return false;
        }
        
        // Route has hit a dead end
        if (nextSquares.Count == 0)
        {
            _deadEndHitOptimisationCount++;
            return false;
        }
        
        // Visiting an edge square carries the risk of isolating a corner square - verify that this is not the case
        if (index.IsEdgeSquare(_n) && !index.IsCornerSquare(_n) && !AreCornersAccessible(index))
        {
            _inaccessibleCornerOptimisationCount++;
            return false;
        }
        
        // Has a row or column been completed, blocking squares on one side from the other
        if (index.IsMiddleSquare(_n) && (CreatesRowBlock(index) || CreatesColumnBlock(index)))
        {
            _routeBlockedOptimisationCount++;
            return false;
        }
        
        // If the route has reached a fork can the two squares be connected?
        // If not then the route is isolating a section of the grid and whichever route is taken will reach a dead end
        if (!CanConnectionsMeet(index, nextSquares))
        {
            _deadEndCreatedOptimisationCount++;
            return false;
        }
        
        // Scan the grid for any squares that have been made inaccessible (no connections available)
        // This scan is expensive so only run when a threshold has been met
        if (IsOptimisationThresholdMet() && !AreRemainingSquaresAccessible())
        {
            _inaccessibleRouteOptimisationCount++;
            return false;
        }

        return true;
    }
    
    private bool IsRouteValid(int index, IReadOnlyCollection<int> nextSquares)
    {
        // Are there any more valid end squares remaining for the route to end on
        if (_validEndCount == 0)
            return false;

        // Route has hit a dead end
        if (nextSquares.Count == 0)
            return false;

        // Visiting an edge square carries the risk of isolating a corner square - verify that this is not the case
        if (index.IsEdgeSquare(_n) && !index.IsCornerSquare(_n) && !AreCornersAccessible(index))
            return false;

        // Has a row or column been completed, blocking squares on one side from the other
        if (index.IsMiddleSquare(_n) && (CreatesRowBlock(index) || CreatesColumnBlock(index)))
            return false;

        // If the route has reached a fork can the two squares be connected?
        // If not then the route is isolating a section of the grid and whichever route is taken will reach a dead end
        if (!CanConnectionsMeet(index, nextSquares))
            return false;

        // Scan the grid for any squares that have been made inaccessible (no connections available)
        // This scan is expensive so only run when a threshold has been met
        if (IsOptimisationThresholdMet() && !AreRemainingSquaresAccessible())
            return false;

        return true;
    }

    private bool AreRemainingSquaresAccessible()
    {
        for (var i = 0; i < _squares.Length; i++)
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

    private bool CreatesRowBlock(int thisIndex)
    {
        var currentRow = thisIndex.GetYCoordinate(_n);
        // This square has not been locked yet so a count of 1 completes the row
        if (_remainingRowCounts[currentRow] <= 1)
        {
            // This square being visited completes the row
            for (var i = 0; i < currentRow; i++)
            {
                if (_remainingRowCounts[i] == 0) continue;

                // Unvisited squares have been identified above this one, no need to continue searching
                i = currentRow;

                for (var j = currentRow + 1; j < _n; j++)
                {
                    if (_remainingRowCounts[j] > 0)
                    {
                        // Unvisited squares have also been identified below this one
                        // This square creates a block and the route is impossible
                        return true;
                    }
                }
            }
        }

        return false;
    }
    private bool CreatesColumnBlock(int thisIndex)
    {
        var currentColumn = thisIndex.GetXCoordinate(_n);
        // This square has not been locked yet so a count of 1 completes the column
        if (_remainingColumnCounts[currentColumn] <= 1)
        {
            // This square being visited completes the column
            for (var i = 0; i < currentColumn; i++)
            {
                if (_remainingColumnCounts[i] == 0) continue;
                
                // Unvisited squares have been identified to the left of this one, no need to continue searching
                i = currentColumn;

                for (var j = currentColumn + 1; j < _n ; j++)
                {
                    if (_remainingColumnCounts[j] > 0)
                    {
                        // Unvisited squares have also been identified to the right of this one
                        // This square creates a block and the route is impossible
                        return true;
                    }
                }
            }
        }
        
        return false;
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

    private bool CanConnectionsMeet(int thisIndex, IReadOnlyCollection<int> nextSquares)
    {
        if (nextSquares.Count != 2)
            return true;
        
        SetTempSquareLock(thisIndex, true);

        var isFound = FindIndexInConnections(new List<int> { nextSquares.First() }, nextSquares.Last(), 3);

        SetTempSquareLock(thisIndex, false);

        return isFound;
    }
    
    private bool FindIndexInConnections(List<int> currentLayerIndexes, int targetIndex, int depth)
    {
        if (depth == 0)
            return false;

        var found = false;
        var nextLayerIndexes = new List<int>();
        
        foreach (var currentIndex in currentLayerIndexes)
        {
            found = found || currentIndex == targetIndex;
            
            // Check SetTempSquareLock hasn't already been called for this index
            // This prevents duplicated calls to nextLayerIndexes (without having to call Distinct)
            if (!_squares[currentIndex] && !found)
            {
                foreach (var nextIndex in GetNextSquares(currentIndex))
                {
                    nextLayerIndexes.Add(nextIndex);
                }
            }
            
            SetTempSquareLock(currentIndex, true);
        }

        found = found || FindIndexInConnections(nextLayerIndexes, targetIndex, depth - 1);

        foreach (var index in currentLayerIndexes)
        {
            SetTempSquareLock(index, false);
        }

        return found;
    }

    private void SetTempSquareLock(int index, bool isLocked)
    {
        _squares[index] = isLocked;
    }
    
    private void SetSquareLock(int index, bool isLocked)
    {
        if (isLocked)
        {
            if (!_squares[index])
            {
                _squares[index] = true;
                _remainingCount--;
            
                if (_validEndSquares[index])
                {
                    _validEndCount--;
                }

                _remainingRowCounts[index.GetYCoordinate(_n)]--;
                _remainingColumnCounts[index.GetXCoordinate(_n)]--;
            }
        }
        else
        {
            if (_squares[index])
            {
                _squares[index] = false;
                _remainingCount++;

                if (_validEndSquares[index])
                {
                    _validEndCount++;
                }
            
                _remainingRowCounts[index.GetYCoordinate(_n)]++;
                _remainingColumnCounts[index.GetXCoordinate(_n)]++;
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