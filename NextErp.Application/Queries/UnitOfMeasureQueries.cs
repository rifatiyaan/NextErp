using MediatR;
using NextErp.Application.DTOs;

namespace NextErp.Application.Queries;

public record GetAllUnitOfMeasuresQuery : IRequest<IReadOnlyList<UnitOfMeasure.Response.Single>>;
public record GetUnitOfMeasureByIdQuery(int Id) : IRequest<UnitOfMeasure.Response.Single?>;
