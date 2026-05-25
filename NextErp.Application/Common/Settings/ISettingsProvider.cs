namespace NextErp.Application.Common.Settings;

public interface ISettingsProvider
{
    Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : class, new();

    Task<T> UpdateAsync<T>(T settings, CancellationToken cancellationToken = default) where T : class, new();

    Task PatchAsync(string moduleName, IReadOnlyDictionary<string, object?> values, CancellationToken cancellationToken = default);

    SettingsSchema GetSchema();

    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>> GetAllValuesAsync(CancellationToken cancellationToken = default);
}
