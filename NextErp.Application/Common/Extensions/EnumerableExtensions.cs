namespace NextErp.Application.Common.Extensions;

/// <summary>
/// Conditional LINQ wrappers for in-memory <see cref="IEnumerable{T}"/>. Mirror the
/// <see cref="QueryableExtensions"/> shape but take <see cref="Func{T, TResult}"/> delegates
/// (in-memory predicates) instead of <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
/// </summary>
public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, bool> predicate)
        => condition ? source.Where(predicate) : source;

    public static IEnumerable<T> WhereIfHasValue<T, TValue>(
        this IEnumerable<T> source,
        TValue? value,
        Func<T, bool> predicate)
        where TValue : struct
        => value.HasValue ? source.Where(predicate) : source;

    public static IEnumerable<T> WhereIfNotEmpty<T>(
        this IEnumerable<T> source,
        string? value,
        Func<T, bool> predicate)
        => !string.IsNullOrWhiteSpace(value) ? source.Where(predicate) : source;

    public static IEnumerable<T> WhereIfNotNullOrEmpty<T>(
        this IEnumerable<T> source,
        string? value,
        Func<T, bool> predicate)
        => !string.IsNullOrEmpty(value) ? source.Where(predicate) : source;

    public static IEnumerable<T> WhereIfAny<T, TItem>(
        this IEnumerable<T> source,
        IReadOnlyCollection<TItem>? items,
        Func<T, bool> predicate)
        => items is { Count: > 0 } ? source.Where(predicate) : source;
}
