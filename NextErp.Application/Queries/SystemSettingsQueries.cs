using MediatR;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Queries;

public record GetSystemSettingsQuery() : IRequest<SystemSettingsDto.Response.Single>;
