using MediatR;

namespace NextErp.Application.Commands.Stock;

public record SetReorderLevelCommand(int ProductVariantId, decimal? ReorderLevel) : IRequest;
