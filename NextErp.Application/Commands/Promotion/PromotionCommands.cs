using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Promotion;

namespace NextErp.Application.Commands.Promotion;

public record CreatePromotionCommand(CreatePromotionRequest Request)
    : IRequest<Guid>, ITransactionalRequest;

public record UpdatePromotionCommand(Guid Id, UpdatePromotionRequest Request)
    : IRequest<bool>, ITransactionalRequest;

public record DeactivatePromotionCommand(Guid Id)
    : IRequest<bool>, ITransactionalRequest;
