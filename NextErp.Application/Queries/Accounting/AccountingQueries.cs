using MediatR;
using NextErp.Application.DTOs.Accounting;
using NextErp.Domain.Entities;

namespace NextErp.Application.Queries.Accounting;

public record GetAccountByIdQuery(Guid Id)
    : IRequest<AccountDto.Response.Single?>;

public record GetPagedAccountsQuery(
    int PageIndex = 1,
    int PageSize = 50,
    string? SearchText = null,
    AccountType? Type = null,
    bool? OnlyPostable = null)
    : IRequest<AccountDto.Response.Paged>;

public record GetJournalEntryByIdQuery(Guid Id)
    : IRequest<JournalDto.Response.Single?>;

public record GetPagedJournalEntriesQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchText = null,
    JournalEntryReferenceType? ReferenceType = null)
    : IRequest<JournalDto.Response.Paged>;
