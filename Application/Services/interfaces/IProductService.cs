using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.interfaces
{
    public interface IProductService
    {
     
        Task<ProductResponse> CreateProductAsync(ProductDto request);
        Task<ProductResponse> GetByIdAsync(int id);
        Task<IEnumerable<ProductResponse>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductResponse>> GetProductsByNameAsync(string name);
        Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
        Task<IEnumerable<ProductResponse>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<ProductResponse>> GetAvailableProductsAsync();
        Task<IEnumerable<ProductResponse>> GetVegetarianProductsAsync();

        // Queryable methods for advanced scenarios
        IQueryable<ProductResponse> GetProductsQueryable();
        IQueryable<ProductResponse> FilterProductsQueryable(ProductOrderingEnum orderingEnum, string searchTerm = null);

        Task UpdateProductAsync(int id, UpdateProductRequest request);
        Task DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task<ProductResponse> CheckProductAvailabilityAsync(int productId, int Quantity);

    }
}
