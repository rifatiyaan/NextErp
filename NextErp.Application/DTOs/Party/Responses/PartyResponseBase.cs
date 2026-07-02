using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Party;

public abstract record PartyResponseBase
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? LoyaltyCode { get; set; }
    public string? NationalId { get; set; }
    public string? VatNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }
    public PartyType PartyType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
}
