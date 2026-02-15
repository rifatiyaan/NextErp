using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using NextErp.Domain.Repositories;

namespace NextErp.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IApplicationDbContext _dbContext;

        public StockService(
            IStockRepository stockRepository,
            IProductRepository productRepository,
            IApplicationDbContext dbContext)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _dbContext = dbContext;
        }

        public async Task<bool> CheckStockAvailabilityAsync(int productId, decimal requiredQuantity, CancellationToken cancellationToken = default)
        {
            var availableStock = await GetAvailableStockAsync(productId, cancellationToken);
            return availableStock >= requiredQuantity;
        }

        public async Task<decimal> GetAvailableStockAsync(int productId, CancellationToken cancellationToken = default)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId);
            if (stock != null)
            {
                return stock.AvailableQuantity;
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return 0;
            }

            return product.Stock;
        }

        public async Task ReduceStockAsync(int productId, decimal quantity, CancellationToken cancellationToken = default)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId);
            
            if (stock != null)
            {
                if (stock.AvailableQuantity < quantity)
                {
                    var availableProduct = await _productRepository.GetByIdAsync(productId);
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{availableProduct?.Title ?? $"ID {productId}"}'. " +
                        $"Available: {stock.AvailableQuantity}, Required: {quantity}");
                }

                stock.AvailableQuantity -= quantity;
                stock.UpdatedAt = DateTime.UtcNow;
                await _stockRepository.EditAsync(stock);
                
                // Also update Product.Stock for consistency
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    product.Stock -= (int)quantity;
                    if (product.Stock < 0) product.Stock = 0; // Prevent negative
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.EditAsync(product);
                }
            }
            else
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found.");
                }

                if (product.Stock < (int)quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{product.Title}'. " +
                        $"Available: {product.Stock}, Required: {quantity}");
                }

                product.Stock -= (int)quantity;
                if (product.Stock < 0) product.Stock = 0; // Prevent negative
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.EditAsync(product);
            }
        }

        public async Task IncreaseStockAsync(int productId, decimal quantity, CancellationToken cancellationToken = default)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId);
            
            if (stock != null)
            {
                stock.AvailableQuantity += quantity;
                stock.UpdatedAt = DateTime.UtcNow;
                await _stockRepository.EditAsync(stock);
                
                // Also update Product.Stock for consistency
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    product.Stock += (int)quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.EditAsync(product);
                }
            }
            else
            {
                // Stock record should exist after EnsureStockRecordExistsAsync
                // But if it doesn't, create it now
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found.");
                }

                // Create stock record
                var newStock = new Stock
                {
                    Id = productId,
                    ProductId = productId,
                    Title = $"Stock - {product.Title}",
                    AvailableQuantity = quantity,
                    TenantId = product.TenantId,
                    CreatedAt = DateTime.UtcNow
                };
                await _stockRepository.AddAsync(newStock);

                // Update Product.Stock
                product.Stock += (int)quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.EditAsync(product);
            }
        }

        public async Task EnsureStockRecordExistsAsync(int productId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId);
            if (stock != null)
            {
                return;
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found.");
            }

            // Initialize with Product.Stock value (could be 0, that's fine - IncreaseStockAsync will add to it)
            var newStock = new Stock
            {
                Id = productId, // Set Id to ProductId for one-to-one relationship
                ProductId = productId,
                Title = $"Stock - {product.Title}",
                AvailableQuantity = product.Stock > 0 ? (decimal)product.Stock : 0,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            await _stockRepository.AddAsync(newStock);
        }
    }
}

