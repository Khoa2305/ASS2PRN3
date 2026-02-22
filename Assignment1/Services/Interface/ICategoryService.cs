using Assignment1.Models;

namespace Assignment1.Services.Interface
{
    public interface ICategoryService
    {
        public Task<List<Category>> GetCategoriesAsync();
        public Task<Category> GetCategoryByIdAsync(int id);
        public Task<Category> AddCategoryAysnc(Category category);
        public Task<short> GetValidIdAsync();
        public Task DeleteCategoryAsync(int id);
        public Task<Category> UpdateAsync(Category category);
    }
}
