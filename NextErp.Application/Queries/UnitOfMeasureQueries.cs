using MediatR;
using NextErp.Application.DTOs.UnitOfMeasure;

namespace NextErp.Application.Queries;

public record GetAllUnitOfMeasuresQuery : IRequest<IReadOnlyList<UnitOfMeasureResponse>>;
public record GetUnitOfMeasureByIdQuery(int Id) : IRequest<UnitOfMeasureResponse?>;
