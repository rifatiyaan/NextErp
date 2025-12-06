using NextErp.Application;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure
{
    public class ApplicationUnitOfWork : UnitOfWork, IApplicationUnitOfWork
    {
        //public IBookRepository BookRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public ICategoryRepository CategoryRepository { get; private set; }
        public IUserRepository UserRepository { get; private set; }
        public IModuleRepository ModuleRepository { get; private set; }

        public ApplicationUnitOfWork(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IModuleRepository moduleRepository,
            IApplicationDbContext dbContext
        ) : base((DbContext)dbContext)
        {
            ProductRepository = productRepository;
            CategoryRepository = categoryRepository;
            UserRepository = userRepository;
            ModuleRepository = moduleRepository;
        }
    }
}
