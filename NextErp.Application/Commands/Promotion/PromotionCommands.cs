using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Promotion;

namespace NextErp.Application.Commands.Promotion;

public record CreatePromotionCommand(PromotionDto.Request.Create Request)
    : IRequest<Guid>, ITransactionalRequest;

public record UpdatePromotionCommand(Guid Id, PromotionDto.Request.Update Request)
    : IRequest<bool>, ITransactionalRequest;

public record DeactivatePromotionCommand(Guid Id)
    : IRequest<bool>, ITransactionalRequest;
