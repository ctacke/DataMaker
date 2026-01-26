namespace DataMaker;

using System.Collections;

/// <summary>
/// Represents a query over generated data with deferred execution.
/// Generation occurs when the collection is enumerated, allowing predicates to filter items during generation.
/// </summary>
/// <typeparam name="T">The type of entity being generated.</typeparam>
public class GeneratedQuery<T> : IEnumerable<T> where T : new()
{
    private readonly Generator _generator;
    private readonly int _count;
    private readonly SelectionStrategy _strategy;
    private readonly bool _allowRepeats;
    private readonly List<Func<T, bool>> _predicates = new();

    internal GeneratedQuery(Generator generator, int count,
        SelectionStrategy strategy, bool allowRepeats)
    {
        _generator = generator;
        _count = count;
        _strategy = strategy;
        _allowRepeats = allowRepeats;
    }

    private GeneratedQuery(GeneratedQuery<T> source, Func<T, bool> predicate)
    {
        _generator = source._generator;
        _count = source._count;
        _strategy = source._strategy;
        _allowRepeats = source._allowRepeats;
        _predicates = new List<Func<T, bool>>(source._predicates) { predicate };
    }

    /// <summary>
    /// Ensures items matching the predicate are included first in the results,
    /// then fills the remaining slots with other items to reach the requested count.
    /// Multiple FirstAdd() calls can be chained - items matching any predicate are prioritized.
    /// </summary>
    /// <param name="predicate">A function to identify items that should be included first.</param>
    /// <returns>A new GeneratedQuery with the added predicate.</returns>
    /// <example>
    /// <code>
    /// // Get 10 customers, ensuring the one with ID=5 is included first
    /// var customers = generator
    ///     .Generate&lt;Customer&gt;(10)
    ///     .FirstAdd(c => c.Id == 5)
    ///     .ToList();
    /// </code>
    /// </example>
    public GeneratedQuery<T> FirstAdd(Func<T, bool> predicate)
    {
        return new GeneratedQuery<T>(this, predicate);
    }

    /// <summary>
    /// Returns an enumerator that generates and iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the generated collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _generator.GenerateInternal<T>(_count, _strategy, _allowRepeats, _predicates))
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
