namespace LatticeNumbering.Extensions;

public static class IndexExtensions
{
    // https://stackoverflow.com/questions/36926608/chess-board-maker-in-javascript-the-code-works-but-how-does-it-work
    public static bool IsEndSquare(this int index, int n) =>
        (index.GetXCoordinate(n) + index.GetYCoordinate(n)) % 2 == (n * n - 1) % 2;

    public static int GetXCoordinate(this int index, int n) => index % n;
    public static int GetYCoordinate(this int index, int n) => (int)Math.Floor((double)index / n);

    public static bool IsCornerSquare(this int index, int n) => index == 0 || index == n - 1
                                                                           || index == n * n - n ||
                                                                           index == n * n - 1;
    
    public static bool IsMiddleSquare(this int index, int n) => !IsEdgeSquare(index, n);

    public static bool IsEdgeSquare(this int index, int n) => IsTopEdge(index, n) || IsBottomEdge(index, n) ||
                                                              IsLeftEdge(index, n) || IsRightEdge(index, n);

    public static bool IsTopEdge(this int index, int n) => index - n <= 0;

    public static bool IsBottomEdge(this int index, int n) => index + n >= n * n;

    public static bool IsLeftEdge(this int index, int n) => index % n == 0;

    public static bool IsRightEdge(this int index, int n) => (index + 1) % n == 0;
}