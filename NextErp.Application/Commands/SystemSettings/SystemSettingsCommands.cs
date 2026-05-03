using MediatR;
using NextErp.Application.Common.Attributes;
using NextErp.Application.Common.Interfaces;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Commands.SystemSettings;

[RequiresPermission("Settings.System.Manage")]
public record UpdateSystemSettingsCommand(SystemSettingsDto.Request.Update Dto)
    : IRequest<SystemSettingsDto.Response.Single>, ITransactionalRequest;

[RequiresPermission("Settings.System.Manage")]
public record ResetSystemSettingsCommand()
    : IRequest<SystemSettingsDto.Response.Single>, ITransactionalRequest;
