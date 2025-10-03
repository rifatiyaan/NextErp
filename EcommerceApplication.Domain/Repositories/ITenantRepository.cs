using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Domain.Repositories;

public interface ITenantRepository : IRepositoryBase<Tenant, Guid>
{
    Task<Tenant?> GetByIdWithBranchesAsync(Guid id);
    Task<bool> IsNameUniqueAsync(string name);
}

