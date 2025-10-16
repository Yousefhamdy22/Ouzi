using Application.DTOs;
using Application.Exceptions;
using Application.Services.interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Interfsces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implemntation
{
    class ProductService : IProductService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenaricRepository<Product> _productRepository;
        private readonly IGenaricRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IUnitOfWork unitOfWork,
        IGenaricRepository<Product> productRepository,
            IGenaricRepository<Category> categoryRepository,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductResponse> CreateProductAsync(ProductDto request)
        {
            try
            {
               
                var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId);
                if (categoryExists == null)
                {
                    throw new ArgumentException($"Category with ID {request.CategoryId} does not exist");
                }

                var product = _mapper.Map<Product>(request);
                product.CreatedDate = DateTime.UtcNow;

                await _productRepository.AddAsync(product);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Product created successfully: {ProductId}", product.Id);

                return await GetProductResponseById(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                throw;
            }
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            try
            {
                var product = await GetProductWithIncludes()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                return _mapper.Map<ProductResponse>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await GetProductWithIncludes()
                    .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} products for category {CategoryId}", 
                    products.Count, categoryId);
               
                var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
                return productsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching products for category {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return await GetAllProductsAsync();
                }

                var products = await GetProductWithIncludes()
                    .Where(p => p.Name.Contains(name) && p.IsAvailable)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
                return productsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching products by name: {Name}", name);
                throw;
            }
        }

        public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAsNoTracking()
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
                return productsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all products");
                throw;
            }
        }

        public async Task<IEnumerable<ProductResponse>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllProductsAsync();
                }

                var normalizedSearchTerm = searchTerm.Trim().ToLower();

                var products = await GetProductWithIncludes()
                    .Where(p => p.IsAvailable &&
                               (p.Name.ToLower().Contains(normalizedSearchTerm) ||
                                p.Description.ToLower().Contains(normalizedSearchTerm) ||
                                p.Category.Name.ToLower().Contains(normalizedSearchTerm)))
                    .OrderBy(p => p.Name)
                 
                    .ToListAsync();

                _logger.LogInformation("Found {Count} products for search term: {SearchTerm}",
                    searchTerm);

                var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
                return productsResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching products with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<ProductResponse>> GetAvailableProductsAsync()
        {
         var product =  await GetProductWithIncludes()
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(product);
            return productsResponse;
        }

        public async Task<IEnumerable<ProductResponse>> GetVegetarianProductsAsync()
        {
            var Product = await GetProductWithIncludes()
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Name)
               
                .ToListAsync();
            var productsResponse = _mapper.Map<IEnumerable<ProductResponse>>(Product);
            return productsResponse;

        }

        public IQueryable<ProductResponse> GetProductsQueryable()
        {
            var product = GetProductWithIncludes()
               .Where(p => p.IsAvailable)
               .ProjectTo<ProductResponse>(_mapper.ConfigurationProvider);
           
            return product;

        }

        public IQueryable<ProductResponse> FilterProductsQueryable(ProductOrderingEnum orderingEnum, string searchTerm = null)
        {
            var query = GetProductWithIncludes().Where(p => p.IsAvailable);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(normalizedSearchTerm) ||
                    p.Description.ToLower().Contains(normalizedSearchTerm) ||
                    p.Category.Name.ToLower().Contains(normalizedSearchTerm));
            }

           
            var orderedQuery = orderingEnum switch
            {
                ProductOrderingEnum.Name => query.OrderBy(p => p.Name),
                ProductOrderingEnum.Price => query.OrderBy(p => p.Price),
                ProductOrderingEnum.Category => query.OrderBy(p => p.Category.Name),
                ProductOrderingEnum.CreatedDate => query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderBy(p => p.Id)
            };

            return orderedQuery.ProjectTo<ProductResponse>(_mapper.ConfigurationProvider);
        }

        public async Task UpdateProductAsync(int id, UpdateProductRequest request)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                // Update properties if provided
                if (!string.IsNullOrWhiteSpace(request.Name))
                    product.Name = request.Name;

                if (!string.IsNullOrWhiteSpace(request.Description))
                    product.Description = request.Description;

                if (request.Price.HasValue)
                    product.Price = request.Price.Value;

                if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                    product.ImageUrl = request.ImageUrl;

                if (request.IsAvailable.HasValue)
                    product.IsAvailable = request.IsAvailable.Value;              

                if (request.CategoryId.HasValue)
                {
                    var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                    if (categoryExists == null)
                    {
                        throw new ArgumentException($"Category with ID {request.CategoryId} does not exist");
                    }
                    product.CategoryId = request.CategoryId.Value;
                }

                product.UpdatedDate = DateTime.UtcNow;

              await _productRepository.UpdateAsync(product);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Product updated successfully: {ProductId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                await _productRepository.DeleteAsync(product);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id) != null;
        }

        private IQueryable<Product> GetProductWithIncludes()
        {
            return _productRepository.GetQueryable()
                .Include(p => p.Category);
                //.Include(p => p.ImageUrl);
        }

        private async Task<ProductResponse> GetProductResponseById(int id)
        {
            var product = await GetProductWithIncludes()
                .FirstOrDefaultAsync(p => p.Id == id);

            return _mapper.Map<ProductResponse>(product);
        }

        public Task<ProductResponse> CheckProductAvailabilityAsync(int productId, int Quantity)
        {
            var product  = _productRepository.GetQueryable()
                .FirstOrDefault(p => p.Id == productId && p.IsAvailable && p.Stock >= Quantity);

            if (product == null)
                throw new 
                    NotFoundException($"Product with ID {productId} is not available in the requested quantity of {Quantity}.", productId );
            return
                Task.FromResult(_mapper.Map<ProductResponse>(product));
        }
    }
}

