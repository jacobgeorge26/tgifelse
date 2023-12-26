using LatticeNumbering.Extensions;

namespace LatticeNumbering.RouteFinders;

public class IndexRouteFinder : IRouteFinder
{
    private readonly int _n;
    private readonly int _d;
    
    private bool[] _squares = null!;
    private bool[] _middleSquares = null!;

    private int _remainingCount;
    private int _middleCount;

    private int _middleRemainingOptimisationCount;
    private int _breakingMoveOptimisationCount;
    
    public IndexRouteFinder(int n, int d)
    {
        _n = n;
        _d = d;
    }
    
    public int Run()
    {
        // Total number of squares, and of those the number of middle squares, that are available across the entire grid
        _remainingCount = _n * _n;
        _middleCount = (_n - 2) * (_n - 2);
        
        _squares = new bool[_remainingCount];
        _middleSquares = new bool[_remainingCount];
        
        for (var i = 0; i < _remainingCount; i++)
        {
            _squares[i] = false;
            _middleSquares[i] = i.IsMiddleSquare(_n);
        }
        
        // Only going one direction from the origin corner (downwards, right is disconnected)
        var cornerRouteCount = MoveToSquare(0);
        Console.WriteLine("Optimisation method aborted the following number of routes");
        Console.WriteLine($"Middle square remaining optimisation: {_middleRemainingOptimisationCount}");
        Console.WriteLine($"Breaking move optimisation: {_breakingMoveOptimisationCount}");
        
        return cornerRouteCount * 8;
    }
    
    private int MoveToSquare(int thisIndex)
    {
        var count = 0;
        
        // Update grid metrics
        SetSquareLock(thisIndex, true);

        // Determine whether there are any more squares remaining to visit
        if (_remainingCount > 0)
        {
            // Investigate each node connected to this one that has not already been visited by this route
            foreach (var nextNode in GetNextSquares(thisIndex))
            {
                if(VerifyRouteAhead(nextNode, _d))
                    count += MoveToSquare(nextNode);
            }
        }
        else if (_remainingCount > 0)
        {
            // Investigate each node connected to this one that has not already been visited by this route
            foreach (var nextNode in GetNextSquares(thisIndex))
            {
                count += MoveToSquare(nextNode);
            }
        }
        else
        {
            // If this node ends on a middle square then it is a valid route
            if(_middleSquares[thisIndex])
                count++;
        }

        // Allow this node to be visited by other routes
        SetSquareLock(thisIndex, false);
        
        return count;
    }

    private void SetSquareLock(int index, bool isLocked)
    {
        if (isLocked)
        {
            _squares[index] = true;
            _remainingCount--;
            if (_middleSquares[index])
            {
                _middleCount--;
            }
        }
        else
        {
            _squares[index] = false;
            _remainingCount++;
            if (_middleSquares[index])
            {
                _middleCount++;
            }
        }
    }

    // Verify a square has at least one available route remaining
    private bool VerifyRouteAhead(int index, int remainingDepth)
    {
        if (remainingDepth <= 0)
            return true;
        
        SetSquareLock(index, true);

        // Determine whether there are any more squares remaining to visit
        if (_remainingCount > 0)
        {
            // If there are no middle squares in the remaining squares then route is invalid
            if (_middleCount == 0)
            {
                _middleRemainingOptimisationCount++;
                SetSquareLock(index, false);
                return false;
            }

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