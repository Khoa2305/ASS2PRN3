using Assignment1.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.DAO
{
    public class CategoryDAO
    {
        private static CategoryDAO instance = null!;
        private static readonly object instanceLock = new object();

        private CategoryDAO() { }

        public static CategoryDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CategoryDAO();
                    }
                    return instance;
                }
            }
        }

        public async Task<List<Category>> GetCategoriesAsync(FunewsManagementContext context)
        {
            return await context.Categories.Include(s => s.NewsArticles).ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(FunewsManagementContext context, short id)
        {
            return await context.Categories.Include(s => s.NewsArticles).FirstOrDefaultAsync(s => s.CategoryId == id);
        }

        public async Task AddCategoryAsync(FunewsManagementContext context, Category category)
        {
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(FunewsManagementContext context, Category category)
        {
            context.Categories.Update(category);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(FunewsManagementContext context, short id)
        {
            var category = await context.Categories.FindAsync(id);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }
        
        public async Task<short> GetMaxIdAsync(FunewsManagementContext context)
        {
            var maxId = await context.Categories
                            .Select(x => (short?)x.CategoryId)
                            .MaxAsync();
            return maxId ?? 0;
        }
    }
}
