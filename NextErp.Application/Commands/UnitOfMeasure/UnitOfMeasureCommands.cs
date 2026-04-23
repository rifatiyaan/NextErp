using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands.UnitOfMeasure;

public record CreateUnitOfMeasureCommand(string Name, string Abbreviation, string? Category = null, bool IsSystem = false) : IRequest<DTOs.UnitOfMeasure.Response.Single>;
public record UpdateUnitOfMeasureCommand(int Id, string Name, string Abbreviation, string? Category, bool IsActive) : IRequest;
public record DeleteUnitOfMeasureCommand(int Id) : IRequest;
