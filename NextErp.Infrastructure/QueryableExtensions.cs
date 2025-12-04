using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace NextErp.Infrastructure
{
    public static class QueryableExtensions
    {
        // Apply Where clause only when condition is true
        public static IQueryable<T> WhereWhen<T>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }

        // Conditional Include for reference navigation
        public static IQueryable<T> IncludeWhen<T, TProperty>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, TProperty>> includeExpression)
            where T : class
        {
            return condition ? query.Include(includeExpression) : query;
        }

        // Conditional ThenInclude for collection navigation
        public static IQueryable<T> ThenIncludeWhen<T, TPreviousProperty, TProperty>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where T : class
        {
            // Normally ThenInclude must be chained after Include, so you can use manually in your queries
            return query;
        }
    }
}
