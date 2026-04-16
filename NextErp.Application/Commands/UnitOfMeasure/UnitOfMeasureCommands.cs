using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Commands.UnitOfMeasure;

public record CreateUnitOfMeasureCommand(string Name, string Abbreviation) : IRequest<DTOs.UnitOfMeasure.Response.Single>;
public record UpdateUnitOfMeasureCommand(int Id, string Name, string Abbreviation, bool IsActive) : IRequest;
public record DeleteUnitOfMeasureCommand(int Id) : IRequest;
