using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Common.Extensions;

/// <summary>
/// Conditional LINQ wrappers for <see cref="IQueryable{T}"/>. Each method either applies the
/// operator or returns the source unchanged. The generated SQL is identical to writing the
/// imperative if/then form by hand — these are pure readability sugar, no runtime overhead.
///
/// Pair with <see cref="EnumerableExtensions"/> for in-memory variants.
/// </summary>
public static class QueryableExtensions
{
    // ===== WHERE =====

    /// <summary>Apply <paramref name="predicate"/> only when <paramref name="condition"/> is true.</summary>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;

    /// <summary>Apply <paramref name="predicate"/> only when <paramref name="value"/> has a value.</summary>
    public static IQueryable<T> WhereIfHasValue<T, TValue>(
        this IQueryable<T> source,
        TValue? value,
        Expression<Func<T, bool>> predicate)
        where TValue : struct
        => value.HasValue ? source.Where(predicate) : source;

    /// <summary>Apply <paramref name="predicate"/> only when <paramref name="value"/> is not null/whitespace.</summary>
    public static IQueryable<T> WhereIfNotEmpty<T>(
        this IQueryable<T> source,
        string? value,
        Expression<Func<T, bool>> predicate)
        => !string.IsNullOrWhiteSpace(value) ? source.Where(predicate) : source;

    /// <summary>
    /// Apply <paramref name="predicate"/> only when <paramref name="value"/> is not null/empty.
    /// Treats whitespace-only strings as a real value (use this when matching legacy
    /// <see cref="string.IsNullOrEmpty(string?)"/> semantics).
    /// </summary>
    public static IQueryable<T> WhereIfNotNullOrEmpty<T>(
        this IQueryable<T> source,
        string? value,
        Expression<Func<T, bool>> predicate)
        => !string.IsNullOrEmpty(value) ? source.Where(predicate) : source;

    /// <summary>Apply <paramref name="predicate"/> only when <paramref name="items"/> has at least one element.</summary>
    public static IQueryable<T> WhereIfAny<T, TItem>(
        this IQueryable<T> source,
        IReadOnlyCollection<TItem>? items,
        Expression<Func<T, bool>> predicate)
        => items is { Count: > 0 } ? source.Where(predicate) : source;

    // ===== INCLUDE (EF Core only) =====

    /// <summary>Apply <c>.Include(path)</c> only when <paramref name="condition"/> is true.</summary>
    public static IQueryable<T> IncludeIf<T, TProperty>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TProperty>> path)
        where T : class
        => condition ? source.Include(path) : source;

    // ===== ORDER BY =====

    /// <summary>Apply <c>.OrderBy(keySelector)</c> only when <paramref name="condition"/> is true.</summary>
    public static IQueryable<T> OrderByIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
        => condition ? source.OrderBy(keySelector) : source;

    /// <summary>Apply <c>.OrderByDescending(keySelector)</c> only when <paramref name="condition"/> is true.</summary>
    public static IQueryable<T> OrderByDescendingIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
        => condition ? source.OrderByDescending(keySelector) : source;

    // ===== PAGING =====

    /// <summary>
    /// Page the query when <paramref name="paged"/> is true, using 1-based <paramref name="pageIndex"/>.
    /// Useful when the same query supports both "give me a page" and "give me everything" modes.
    /// </summary>
    public static IQueryable<T> PageIf<T>(
        this IQueryable<T> source,
        bool paged,
        int pageIndex,
        int pageSize)
        => paged ? source.Skip((pageIndex - 1) * pageSize).Take(pageSize) : source;
}
