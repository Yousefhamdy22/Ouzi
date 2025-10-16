using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using Application.Services.interfaces;
using AutoMapper;

using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Interfsces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Application.Services.Implemntation
{
    public class CategoryService : ICategoryServices
    {
        private readonly ICategory _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;
      
        public CategoryService(ICategory categoryRepository,
            IMapper mapper, ILogger<CategoryService> logger , IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<CategoryDto>> CreateCategory(CategoryDto categoryDto)
        {
          
            // 1. Input Validation

            if(categoryDto == null)
            {
                throw new ArgumentNullException(nameof(categoryDto), "Category data cannot be null.");
            }

            // 2. Check for Business Rules/Duplicates

            // 3. Map and Add
            var category = _mapper.Map<Category>(categoryDto);
            await _unitOfWork.Categories.AddAsync(category);
            // 4. Commit with Error Handling
            try
            {
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);
              
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while creating a category.");
                throw new ServiceLayerException("An error occurred while saving the " +
                    "category to the database. Please try again.", ex);
            }
            // 5. Map back to a DTO for the return
            var resultDto = _mapper.Map<CategoryDto>(category);

            // 6. Return a rich response object
            var response = Response<CategoryDto>.Ok(resultDto, "Category created successfully.");

            return response;

                
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid category ID", nameof(id));
            }

            // Get entity first
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {id} was not found." ,id);
            }

            // Delete using the ENTITY (not ID)
            await _categoryRepository.DeleteAsync(category);

            try
            {
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Category with ID: {CategoryId} deleted successfully", id);
                return Response<bool>.Ok(true, "Category deleted successfully.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting category ID: {CategoryId}", id);
                throw new ServiceLayerException("Error deleting category", ex);
            }
        }
        //public async Task<Response<bool>> DeleteAsync(int id)
        //{
        //    if (id <= 0)
        //    {
        //        throw new ArgumentException("Invalid category ID", nameof(id));
        //    }

        //    var category = await _categoryRepository.GetByIdAsync(id);
        //    if (category == null)
        //    {
        //        throw new NotFoundException($"Category with ID  was not found." , id);
        //    }

        //    await _unitOfWork.Categories.DeleteAsync(category);

        //    try
        //    {
        //        await _unitOfWork.CommitAsync();
        //        _logger.LogInformation("Category with ID: {CategoryId} was deleted successfully", id);
        //        return Response<bool>.Ok(true, "Category deleted successfully.");
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        _logger.LogError(ex, "Database error occurred while deleting category with ID: {CategoryId}", id);
        //        throw new ServiceLayerException("An error occurred while deleting the category. Please try again.", ex);
        //    }
        //}

        public async Task<Response<CategoryDto>> EditAsync(CategoryDto categoryDto)
        {
            if (categoryDto == null)
            {
                throw new ArgumentNullException(nameof(categoryDto), "Category data cannot be null.");
            }

            if (categoryDto.Id <= 0)
            {
                throw new ArgumentException("Invalid category ID", nameof(categoryDto.Id));
            }

            // Check if category exists
            var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.Id);
            if (existingCategory == null)
            {
                throw new NotFoundException($"Category with ID {categoryDto} was not found." , categoryDto.Id);
            }

            // Check for duplicate name (excluding current category)
            bool duplicateExists = await _categoryRepository.AnyAsync(c =>
                c.Name.ToLower() == categoryDto.Name.Trim().ToLower() && c.Id != categoryDto.Id);
          
            if (duplicateExists)
            {
                throw new DuplicateException($"A category with the name '{categoryDto}' already exists." , categoryDto.Name);
            }

            // Update the existing category
            _mapper.Map(categoryDto, existingCategory);
            await _unitOfWork.Categories.UpdateAsync(existingCategory);

            try
            {
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Category with ID: {CategoryId} was updated successfully", categoryDto.Id);

                var updatedDto = _mapper.Map<CategoryDto>(existingCategory);
                return Response<CategoryDto>.Ok(updatedDto, "Category updated successfully.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while updating category with ID: {CategoryId}", categoryDto.Id);
                throw new ServiceLayerException("An error occurred while updating the category. Please try again.", ex);
            }
        }

        public async Task<Response<CategoryDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid category ID", nameof(id));
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {id} was not found." , id);
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Response<CategoryDto>.Ok(categoryDto, "Category retrieved successfully.");
        }

        public Task<Response<List<CategoryDto>>> GetByNameAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Response<List<CategoryDto>>> GetCategoryListAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            if (categories == null || !categories.Any())
            {
                return Response<List<CategoryDto>>.Ok(new List<CategoryDto>(), "No categories found.");
            }

            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return Response<List<CategoryDto>>.Ok(categoryDtos, "Categories retrieved successfully.");
        }

      
    }
}
