using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Settings;

[RequiresPermission("Settings.System.Manage")]
public record PatchFeatureSettingsCommand(
    string Module,
    IReadOnlyDictionary<string, object?> Values
) : IRequest<Unit>, ITransactionalRequest;
