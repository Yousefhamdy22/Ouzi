using Application.DTOs;
using Application.Helper;
using Domain.Entities;


namespace Application.Services.interfaces
{
    public interface ICategoryServices
    {
        public Task<Response<CategoryDto>> CreateCategory(CategoryDto category);
        public Task<Response<bool>> DeleteAsync(int id);
        Task<Response<CategoryDto>> EditAsync(CategoryDto categoryDto);
        Task<Response<CategoryDto>> GetByIdAsync(int id);

        //Task<Response<CategoryDto>> GetByIdWithIncludeAsync(int id);
        Task<Response<List<CategoryDto>>> GetCategoryListAsync();
        Task<Response<List<CategoryDto>>> GetByNameAsync();


    }
}
