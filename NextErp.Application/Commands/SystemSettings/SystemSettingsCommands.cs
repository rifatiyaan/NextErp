using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Commands.SystemSettings;

[RequiresPermission("Settings.System.Manage")]
public record UpdateSystemSettingsCommand(UpdateSystemSettingsRequest Dto)
    : IRequest<SystemSettingsResponse>, ITransactionalRequest;

[RequiresPermission("Settings.System.Manage")]
public record ResetSystemSettingsCommand()
    : IRequest<SystemSettingsResponse>, ITransactionalRequest;
