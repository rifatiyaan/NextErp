namespace NextErp.Application.Common.Interfaces;

/// <summary>
/// Marker for MediatR requests that should execute inside a database transaction.
/// </summary>
public interface ITransactionalRequest { }
