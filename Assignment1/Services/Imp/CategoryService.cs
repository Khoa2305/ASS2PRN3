using Assignment1.Constant;
using Assignment1.CustomException;
using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Interface;

namespace Assignment1.Services.Imp
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> AddCategoryAysnc(Category category)
        {
            return await _categoryRepository.AddCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync((short)id);
            if (category == null) throw new AppException(Fail.FAIL_GET_CATEGORY);

            if (category.NewsArticles.Count > 0)
            {
                throw new AppException(Fail.CATEGORY_OWN_ARTICLE);
            }
            await _categoryRepository.DeleteCategoryAsync((short)id);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _categoryRepository.GetCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync((short)id);
            if (category == null)
            {
                throw new AppException(Fail.FAIL_GET_CATEGORY);
            }
            return category;
        }

        public async Task<short> GetValidIdAsync()
        {
            short maxId = await _categoryRepository.GetMaxIdAsync();
            var nextId = maxId + 1;

            if (nextId > short.MaxValue)
                throw new InvalidOperationException("Id đã vượt quá giới hạn của short");

            return (short)nextId;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            await _categoryRepository.UpdateCategoryAsync(category);
            return category;
        }
    }
}
