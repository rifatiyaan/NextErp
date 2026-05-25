using MediatR;
using NextErp.Application.Common.Settings;

namespace NextErp.Application.Queries;

public record GetFeatureSettingsSchemaQuery() : IRequest<SettingsSchema>;

public record GetFeatureSettingsValuesQuery() : IRequest<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>;
