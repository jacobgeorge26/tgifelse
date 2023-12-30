using System.Diagnostics;

namespace LatticeNumbering
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length is not 1 or 2)
                throw new ArgumentException($"Expected either 1 or 2 arguments, {args.Length} provided");

            if (!int.TryParse(args[0], out var n))
                throw new ArgumentException("Int value expected for n");

            if (n < 3)
                throw new ArgumentOutOfRangeException(nameof(n),"Value >= 3 expected for n");

            var verbose = args.Length == 2 && args[1] == "-v";

            Console.WriteLine($"Finding number of valid routes for a {n} by {n} grid");
            
            var routeFinder = new RouteFinder(n, verbose);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var routeCount = routeFinder.Run();
            
            stopwatch.Stop();
            
            Console.WriteLine();
            Console.WriteLine($"{routeCount} routes possible");
            Console.WriteLine($"Completed in {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)} seconds");

            Console.WriteLine();
        }
    }
}