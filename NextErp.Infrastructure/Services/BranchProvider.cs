using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NextErp.Application.Interfaces;

namespace NextErp.Infrastructure.Services
{
    public class BranchProvider(IHttpContextAccessor httpContextAccessor) : IBranchProvider
    {
        public bool IsGlobal()
        {
            var raw = httpContextAccessor.HttpContext?.User.FindFirstValue("isGlobal");
            return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
        }

        public Guid? GetBranchId()
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("branchId");
            return Guid.TryParse(value, out var branchId) ? branchId : null;
        }

        public Guid GetRequiredBranchId()
        {
            var branchId = GetBranchId();
            if (branchId.HasValue)
                return branchId.Value;

            throw new InvalidOperationException("Branch claim is missing from the current user token.");
        }
    }
}
