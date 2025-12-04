using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure.Repositories
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(IApplicationDbContext context) : base((DbContext)context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var (users, _, _) = await GetAsync(
                    filter: u => u.Email == email,
                    orderBy: null,
                    include: null,
                    pageIndex: 1,
                    pageSize: 1,
                    isTrackingOff: true
                    );
            return users.FirstOrDefault();

        }

    }
}
