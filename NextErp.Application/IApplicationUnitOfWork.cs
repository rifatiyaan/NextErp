using NextErp.Domain;
using NextErp.Domain.Repositories;

namespace NextErp.Application
{
    public interface IApplicationUnitOfWork : IUnitOfWork
    {
        //IBookRepository BookRepository { get; }

        IProductRepository ProductRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        IModuleRepository ModuleRepository { get; }
        
        // Inventory Module Repositories
        ISupplierRepository SupplierRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IStockRepository StockRepository { get; }
        IPurchaseRepository PurchaseRepository { get; }
        ISaleRepository SaleRepository { get; }
    }
}
