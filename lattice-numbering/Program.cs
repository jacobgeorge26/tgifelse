using System.Diagnostics;
using LatticeNumbering.Models;
using LatticeNumbering.RouteFinders;

namespace LatticeNumbering
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException(nameof(args), "Missing method argument");

            var routeFinder = GetRouteFinder(args);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var routeCount = routeFinder.Run();
            
            stopwatch.Stop();
            
            Console.WriteLine($"{routeCount} routes possible");

            Console.WriteLine($"Completed in {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)} seconds");
        }
        
        private static IRouteFinder GetRouteFinder(string[] args)
        {
            if (!Enum.TryParse<Method>(args[0], out var method))
                throw new ArgumentException("Invalid value for method");

            var expectedArgs = method switch
            {
                Method.Node => 2,
                Method.Index => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (args.Length != expectedArgs)
                throw new ArgumentException($"{expectedArgs} arguments expected, {args.Length} provided");

            if (!int.TryParse(args[1], out var n))
                throw new ArgumentException("Int value expected for n");

            if (n < 3)
                throw new ArgumentOutOfRangeException(nameof(n),"Value >= 3 expected for n");
            
            if (method == Method.Node)
            {
                Console.WriteLine($"Finding number of valid routes for a {n} by {n} grid, using the {method.ToString()} method");
                return new NodeRouteFinder(n);
            }

            if (!int.TryParse(args[2], out var d))
                throw new ArgumentException("Int value expected for d");

            var maxD = Math.Floor(n / 2f);
            if (d < 0 || d > maxD)
                throw new ArgumentException($"Invalid value {d} for d. Value must be in range 0 - {maxD}");

            Console.WriteLine($"Finding number of valid routes for a {n} by {n} grid, using the {method.ToString()} method");

            return new IndexRouteFinder(n, d);
        }
        
    }
}