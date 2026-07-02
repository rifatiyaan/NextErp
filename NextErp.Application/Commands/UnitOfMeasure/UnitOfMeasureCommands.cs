using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.UnitOfMeasure;

namespace NextErp.Application.Commands.UnitOfMeasure;

public record CreateUnitOfMeasureCommand(string Name, string Abbreviation, string? Category = null, bool IsSystem = false) : IRequest<UnitOfMeasureResponse>, ITransactionalRequest;
public record UpdateUnitOfMeasureCommand(int Id, string Name, string Abbreviation, string? Category, bool IsActive) : IRequest, ITransactionalRequest;
public record DeleteUnitOfMeasureCommand(int Id) : IRequest, ITransactionalRequest;
