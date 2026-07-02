using MediatR;
using NextErp.Application.DTOs.Promotion;
using NextErp.Domain.Entities;

namespace NextErp.Application.Queries.Promotion;

public record GetPromotionByIdQuery(Guid Id)
    : IRequest<PromotionResponse?>;

public record GetPagedPromotionsQuery(
    int PageIndex = 1,
    int PageSize = 50,
    string? SearchText = null,
    PromotionType? Type = null,
    bool? OnlyActive = null)
    : IRequest<PagedPromotionResponse>;
