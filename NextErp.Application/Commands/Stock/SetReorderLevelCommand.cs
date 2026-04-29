using MediatR;
using NextErp.Application.Common.Interfaces;

namespace NextErp.Application.Commands.Stock;

public record SetReorderLevelCommand(int ProductVariantId, decimal? ReorderLevel) : IRequest, ITransactionalRequest;
