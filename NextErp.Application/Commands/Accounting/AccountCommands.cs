using MediatR;
using NextErp.Application.Common.Interfaces;
using NextErp.Application.DTOs.Accounting;

namespace NextErp.Application.Commands.Accounting;

public record CreateAccountCommand(CreateAccountRequest Request)
    : IRequest<Guid>, ITransactionalRequest;

public record UpdateAccountCommand(Guid Id, UpdateAccountRequest Request)
    : IRequest<bool>, ITransactionalRequest;

public record DeactivateAccountCommand(Guid Id)
    : IRequest<bool>, ITransactionalRequest;
