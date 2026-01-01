using MediatR;

namespace NextErp.Application.Commands
{
    // Create
    public record CreateSupplierCommand(
        string Title,
        string? ContactPerson,
        string? Phone,
        string? Email,
        string? Address,
        bool IsActive = true
    ) : IRequest<int>; // Returns Id of created supplier

    // Update
    public record UpdateSupplierCommand(
        int Id,
        string Title,
        string? ContactPerson,
        string? Phone,
        string? Email,
        string? Address,
        bool IsActive = true
    ) : IRequest<Unit>; // No return

    // Soft Delete
    public record SoftDeleteSupplierCommand(int Id) : IRequest<Unit>; // No return
}
