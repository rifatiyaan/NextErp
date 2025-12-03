using EcommerceApplicationWeb.Domain;
using EcommerceApplicationWeb.Domain.Repositories;

namespace EcommerceApplicationWeb.Application
{
    public interface IApplicationUnitOfWork : IUnitOfWork
    {
        //IBookRepository BookRepository { get; }

        IProductRepository ProductRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        IMenuItemRepository MenuItemRepository { get; }
        IModuleRepository ModuleRepository { get; }
    }
}
