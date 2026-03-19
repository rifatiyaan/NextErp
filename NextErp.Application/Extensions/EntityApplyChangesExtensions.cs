using System.Reflection;

namespace NextErp.Application.Extensions
{
    public static class EntityApplyChangesExtensions
    {
        public static T ApplyChanges<T>(
            this T entity,
            IReadOnlyDictionary<string, object?> changes,
            IReadOnlySet<string>? allowedProperties = null) where T : class
        {
            if (entity == null || changes == null || changes.Count == 0)
                return entity;

            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.SetMethod != null)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var kv in changes)
            {
                var key = kv.Key;
                if (allowedProperties != null && !allowedProperties.Contains(key, StringComparer.OrdinalIgnoreCase))
                    continue;
                if (!props.TryGetValue(key, out var prop))
                    continue;

                var value = kv.Value;
                if (value == null)
                {
                    if (!prop.PropertyType.IsClass && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                        continue;
                    prop.SetValue(entity, null);
                    continue;
                }

                var targetType = prop.PropertyType;
                var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
                try
                {
                    if (underlying.IsAssignableFrom(value.GetType()))
                    {
                        prop.SetValue(entity, value);
                        continue;
                    }
                    if (value is string s)
                    {
                        if (underlying == typeof(bool) && bool.TryParse(s, out var b)) { prop.SetValue(entity, b); continue; }
                        if (underlying == typeof(int) && int.TryParse(s, out var i)) { prop.SetValue(entity, i); continue; }
                        if (underlying == typeof(decimal) && decimal.TryParse(s, out var d)) { prop.SetValue(entity, d); continue; }
                        if (underlying == typeof(DateTime) && DateTime.TryParse(s, out var dt)) { prop.SetValue(entity, dt); continue; }
                        if (underlying == typeof(Guid) && Guid.TryParse(s, out var g)) { prop.SetValue(entity, g); continue; }
                    }
                    if (underlying.IsInstanceOfType(value))
                    {
                        prop.SetValue(entity, value);
                        continue;
                    }
                    prop.SetValue(entity, Convert.ChangeType(value, underlying));
                }
                catch
                {
                    // Skip incompatible values
                }
            }

            return entity!;
        }

        public static IEnumerable<T> WhereHasMatchingProperties<T>(
            this IEnumerable<T> source,
            IReadOnlyDictionary<string, object?> changes,
            IReadOnlySet<string>? allowedProperties = null) where T : class
        {
            if (changes == null || changes.Count == 0)
                return source;

            var type = typeof(T);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.SetMethod != null)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            return source.Where(_ =>
            {
                foreach (var key in changes.Keys)
                {
                    if (allowedProperties != null && !allowedProperties.Contains(key, StringComparer.OrdinalIgnoreCase))
                        continue;
                    if (props.ContainsKey(key))
                        return true;
                }
                return false;
            });
        }
    }
}
