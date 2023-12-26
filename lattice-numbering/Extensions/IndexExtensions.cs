namespace LatticeNumbering.Extensions;

public static class IndexExtensions
{
    public static bool IsMiddleSquare(this int index, int n) => !IsEdgeSquare(index, n);

    public static bool IsEdgeSquare(this int index, int n) => IsTopEdge(index, n) || IsBottomEdge(index, n) ||
                                                              IsLeftEdge(index, n) || IsRightEdge(index, n);

    public static bool IsTopEdge(this int index, int n) => index - n <= 0;

    public static bool IsBottomEdge(this int index, int n) => index + n >= n * n;

    public static bool IsLeftEdge(this int index, int n) => index % n == 0;

    public static bool IsRightEdge(this int index, int n) => (index + 1) % n == 0;
}