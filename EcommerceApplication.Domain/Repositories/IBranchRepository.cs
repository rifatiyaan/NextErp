using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Domain.Repositories;

public interface IBranchRepository : IRepositoryBase<Branch, Guid>
{
    Task<IList<Branch>> GetBranchesByTenantAsync(Guid tenantId);
    Task<Branch?> GetBranchByTenantAsync(Guid tenantId, Guid branchId);
}
