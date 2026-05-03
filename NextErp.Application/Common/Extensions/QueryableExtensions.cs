using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Application.Common.Extensions;

public static class QueryableExtensions
{
    // ===== WHERE =====

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;

    public static IQueryable<T> WhereIfHasValue<T, TValue>(
        this IQueryable<T> source,
        TValue? value,
        Expression<Func<T, bool>> predicate)
        where TValue : struct
        => value.HasValue ? source.Where(predicate) : source;

    public static IQueryable<T> WhereIfNotEmpty<T>(
        this IQueryable<T> source,
        string? value,
        Expression<Func<T, bool>> predicate)
        => !string.IsNullOrWhiteSpace(value) ? source.Where(predicate) : source;

    public static IQueryable<T> WhereIfNotNullOrEmpty<T>(
        this IQueryable<T> source,
        string? value,
        Expression<Func<T, bool>> predicate)
        => !string.IsNullOrEmpty(value) ? source.Where(predicate) : source;

    public static IQueryable<T> WhereIfAny<T, TItem>(
        this IQueryable<T> source,
        IReadOnlyCollection<TItem>? items,
        Expression<Func<T, bool>> predicate)
        => items is { Count: > 0 } ? source.Where(predicate) : source;

    // ===== INCLUDE (EF Core only) =====

    public static IQueryable<T> IncludeIf<T, TProperty>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TProperty>> path)
        where T : class
        => condition ? source.Include(path) : source;

    // ===== ORDER BY =====

    public static IQueryable<T> OrderByIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
        => condition ? source.OrderBy(keySelector) : source;

    public static IQueryable<T> OrderByDescendingIf<T, TKey>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, TKey>> keySelector)
        => condition ? source.OrderByDescending(keySelector) : source;

    // ===== PAGING =====

    public static IQueryable<T> PageIf<T>(
        this IQueryable<T> source,
        bool paged,
        int pageIndex,
        int pageSize)
        => paged ? source.Skip((pageIndex - 1) * pageSize).Take(pageSize) : source;
}

