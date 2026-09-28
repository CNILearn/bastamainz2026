namespace ExtensionBlocks.Models;

/// <summary>
/// C# 14 Extension Members for IEnumerable&lt;T&gt; demonstrating collection extension methods.
/// Shows how extension methods can enhance collection operations.
/// 
/// Uses the actual C# 14 extension syntax that compiles and runs in .NET 10 RC 1.
/// This supports generic type parameters and provides better organization for
/// related collection operations.
/// </summary>
public static class CollectionExtensions
{
    extension<T> (IEnumerable<T> source)
    {
        /// <summary>
        /// Extension method that checks if the collection is null or empty.
        /// Utility method for safe collection checking.
        /// </summary>
        public bool IsNullOrEmpty()
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// Extension method that returns a random element from the collection.
        /// Demonstrates random selection with proper null checking.
        /// </summary>
        public T? GetRandomElement()
        {
            if (source.IsNullOrEmpty())
                return default;

            List<T> list = [.. source];
            return list[Random.Shared.Next(list.Count)];
        }

        /// <summary>
        /// Extension method that returns multiple random elements from the collection.
        /// More complex random selection with count parameter.
        /// </summary>
        public IEnumerable<T> GetRandomElements(int count)
        {
            if (source.IsNullOrEmpty() || count <= 0)
                return [];

            List<T> list = [.. source];
            
            return Enumerable.Range(0, Math.Min(count, list.Count))
                            .Select(_ => list[Random.Shared.Next(list.Count)])
                            .Distinct();
        }

        /// <summary>
        /// Extension method that chunks the collection into batches of specified size.
        /// Useful for processing large collections in smaller batches.
        /// </summary>
        public IEnumerable<IEnumerable<T>> ChunkBy(int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));

            if (source.IsNullOrEmpty())
                yield break;

            var chunk = new List<T>(chunkSize);
            
            foreach (var item in source)
            {
                chunk.Add(item);
                
                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                }
            }
            
            if (chunk.Count > 0)
                yield return chunk;
        }
    }
}

/// <summary>
/// C# 14 Extension Block for IEnumerable&lt;int&gt; demonstrating numeric collection extensions.
/// Specialized extension methods for numeric collections.
/// 
/// Uses the actual C# 14 extension syntax that compiles and runs in .NET 10 RC 1.
/// </summary>
public static class NumericCollectionExtensions
{
    extension (IEnumerable<int> source)
    {
        /// <summary>
        /// Extension method that calculates the median value of the collection.
        /// Statistical calculation extension method for numeric collections.
        /// </summary>
        public double Median()
        {
            if (source.IsNullOrEmpty())
                return 0;

            var sorted = source.OrderBy(x => x).ToList();
            var count = sorted.Count;
            
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        /// <summary>
        /// Extension method that finds the mode (most frequent value) in the collection.
        /// More complex statistical calculation with frequency analysis.
        /// </summary>
        public int Mode()
        {
            if (source.IsNullOrEmpty())
                return 0;

            return source.GroupBy(x => x)
                      .OrderByDescending(g => g.Count())
                      .First()
                      .Key;
        }

        /// <summary>
        /// Extension method that calculates the standard deviation of the collection.
        /// Advanced statistical calculation demonstrating mathematical operations.
        /// </summary>
        public double StandardDeviation()
        {
            if (source.IsNullOrEmpty())
                return 0;

            var mean = source.Average();
            var sumOfSquaredDifferences = source.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumOfSquaredDifferences / source.Count());
        }

        /// <summary>
        /// Extension method that returns statistical summary of the collection.
        /// Combines multiple statistical calculations into a single result.
        /// </summary>
        public (int Count, double Mean, double Median, int Mode, double StdDev, int Min, int Max) GetStatistics()
        {
            if (source.IsNullOrEmpty())
                return (0, 0, 0, 0, 0, 0, 0);

            return (
                Count: source.Count(),
                Mean: source.Average(),
                Median: source.Median(),
                Mode: source.Mode(),
                StdDev: source.StandardDeviation(),
                Min: source.Min(),
                Max: source.Max()
            );
        }
    }
}