using MediatR;
using NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Queries;

public record GetSystemSettingsQuery() : IRequest<SystemSettingsResponse>;
