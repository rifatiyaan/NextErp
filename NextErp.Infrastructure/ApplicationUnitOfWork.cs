using NextErp.Application;
using NextErp.Application.Interfaces;
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

        // Inventory Module Repositories
        public ISupplierRepository SupplierRepository { get; private set; }
        public ICustomerRepository CustomerRepository { get; private set; }
        public IStockRepository StockRepository { get; private set; }
        public IPurchaseRepository PurchaseRepository { get; private set; }
        public ISaleRepository SaleRepository { get; private set; }

        public ApplicationUnitOfWork(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IModuleRepository moduleRepository,
            ISupplierRepository supplierRepository,
            ICustomerRepository customerRepository,
            IStockRepository stockRepository,
            IPurchaseRepository purchaseRepository,
            ISaleRepository saleRepository,
            IApplicationDbContext dbContext
        ) : base((DbContext)dbContext)
        {
            ProductRepository = productRepository;
            CategoryRepository = categoryRepository;
            UserRepository = userRepository;
            ModuleRepository = moduleRepository;
            SupplierRepository = supplierRepository;
            CustomerRepository = customerRepository;
            StockRepository = stockRepository;
            PurchaseRepository = purchaseRepository;
            SaleRepository = saleRepository;
        }
    }
}
