using NextErp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

public class User : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParentId { get; set; } = Guid.Empty;   // Guid.Empty = no parent

    [NotMapped]
    public IEntity<Guid>? Child { get; set; }

    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Title { get; set; }
}
