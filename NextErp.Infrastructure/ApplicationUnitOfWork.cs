using NextErp.Application;
using NextErp.Application.Interfaces;
using NextErp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure
{
    public class ApplicationUnitOfWork(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IModuleRepository moduleRepository,
        IPartyRepository partyRepository,
        IStockRepository stockRepository,
        IPurchaseRepository purchaseRepository,
        ISaleRepository saleRepository,
        IApplicationDbContext dbContext)
        : UnitOfWork((DbContext)dbContext), IApplicationUnitOfWork
    {
        public IProductRepository ProductRepository { get; } = productRepository;
        public ICategoryRepository CategoryRepository { get; } = categoryRepository;
        public IUserRepository UserRepository { get; } = userRepository;
        public IModuleRepository ModuleRepository { get; } = moduleRepository;
        public IPartyRepository PartyRepository { get; } = partyRepository;
        public IStockRepository StockRepository { get; } = stockRepository;
        public IPurchaseRepository PurchaseRepository { get; } = purchaseRepository;
        public ISaleRepository SaleRepository { get; } = saleRepository;
    }
}
