using System.Diagnostics;
using LatticeNumbering.Models;
using LatticeNumbering.RouteFinders;

namespace LatticeNumbering
{
    public static class Program
    {
        private static IRouteFinder _routeFinder = null!;
        private static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException($"Two arguments expected, {args.Length} provided");

            if (!int.TryParse(args[0], out var n))
                throw new ArgumentException("Int value expected for n");

            if (n < 3)
                throw new ArgumentOutOfRangeException(nameof(n),"Value >= 3 expected for n");

            if (!Enum.TryParse<Method>(args[1], out var method))
                throw new ArgumentException("Invalid value for method");
            
            _routeFinder = method switch
            {
                Method.Node => new NodeRouteFinder(n),
                Method.Index => new IndexRouteFinder(n),
                _ => throw new ArgumentOutOfRangeException()
            };

            Console.WriteLine($"Finding number of valid routes for an {n} by {n} grid, using the {method.ToString()} method");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var routeCount = _routeFinder.Run();
            
            stopwatch.Stop();
            
            Console.WriteLine($"{routeCount} routes possible");

            Console.WriteLine($"Completed in {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)} seconds");
        }
    }
}