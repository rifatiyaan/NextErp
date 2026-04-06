using NextErp.Domain;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure
{
    public abstract class UnitOfWork(DbContext dbContext) : IUnitOfWork
    {
        public virtual void Dispose() => dbContext?.Dispose();

        public virtual async ValueTask DisposeAsync() => await dbContext.DisposeAsync();

        public virtual void Save() => dbContext?.SaveChanges();
        public virtual async Task SaveAsync() => await dbContext.SaveChangesAsync();
    }
}
