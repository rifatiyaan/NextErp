namespace NextErp.Domain.Repositories
{
    public interface IUserRepository : IRepositoryBase<User, Guid>
    {
        Task<User?> GetByEmailAsync(string email); // needed for login/auth
    }
}
