namespace NextErp.Application.Interfaces;

public interface IUserContext
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? PrimaryRoleId { get; }

    bool IsSuperAdmin { get; }
}
