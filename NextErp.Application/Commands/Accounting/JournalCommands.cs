using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Accounting;

namespace NextErp.Application.Commands.Accounting;

/// <summary>
/// Posts a balanced journal entry representing money moving from one account
/// to another. The handler validates both accounts exist + allow postings,
/// builds a 2-line entry (debit destination / credit source), and saves it
/// transactionally so the ledger never sees a half-posted transfer.
/// </summary>
public record CreateAccountTransferCommand(TransferJournalRequest Request)
    : IRequest<Guid>, ITransactionalRequest;
