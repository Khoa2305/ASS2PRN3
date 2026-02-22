using Assignment1.DAO;
using Assignment1.Models;
using Assignment1.Repositories.Interface;

namespace Assignment1.Repositories.Imp
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly FunewsManagementContext _context;

        public CategoryRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await CategoryDAO.Instance.GetCategoriesAsync(_context);
        }

        public async Task<Category?> GetCategoryByIdAsync(short id)
        {
            return await CategoryDAO.Instance.GetCategoryByIdAsync(_context, id);
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            await CategoryDAO.Instance.AddCategoryAsync(_context, category);
            return category;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await CategoryDAO.Instance.UpdateCategoryAsync(_context, category);
        }

        public async Task DeleteCategoryAsync(short id)
        {
            await CategoryDAO.Instance.DeleteCategoryAsync(_context, id);
        }

        public async Task<short> GetMaxIdAsync()
        {
            return await CategoryDAO.Instance.GetMaxIdAsync(_context);
        }
    }
}
