using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Common.Settings;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.Settings;

public class GetFeatureSettingsSchemaHandler(ISettingsProvider provider, IApplicationDbContext dbContext)
    : IRequestHandler<GetFeatureSettingsSchemaQuery, SettingsSchema>
{
    public async Task<SettingsSchema> Handle(GetFeatureSettingsSchemaQuery request, CancellationToken cancellationToken = default)
    {
        var schema = provider.GetSchema();

        // Fill dynamic "select" options. Only "branches" is defined today; add
        // more sources here as needed. One DB round-trip, shared across settings.
        var branchBacked = schema.Modules
            .SelectMany(m => m.Settings)
            .Where(s => s.OptionsSource == "branches")
            .ToList();

        if (branchBacked.Count > 0)
        {
            // Materialize the raw Id/Title, then format the value client-side.
            // Guid.ToString() inside the SQL projection is provider-specific
            // (SQLite yields uppercase GUID text, C# yields lowercase), so doing
            // it in memory keeps the option value consistent everywhere.
            var branchRows = await dbContext.Branches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .Select(b => new { b.Id, b.Title })
                .ToListAsync(cancellationToken);

            var choices = branchRows
                .Select(b => new SettingOption { Value = b.Id.ToString(), Label = b.Title })
                .ToList();

            foreach (var setting in branchBacked)
                setting.Choices = choices;
        }

        return schema;
    }
}
