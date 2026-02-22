using Assignment1.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.DAO
{
    public class TagDAO
    {
        private static TagDAO instance = null!;
        private static readonly object instanceLock = new object();

        private TagDAO() { }

        public static TagDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new TagDAO();
                    }
                    return instance;
                }
            }
        }

        public async Task<List<Tag>> GetTagsAsync(FunewsManagementContext context)
        {
            return await context.Tags.Include(s => s.NewsArticles).ToListAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(FunewsManagementContext context, int id)
        {
            return await context.Tags.Include(s => s.NewsArticles).FirstOrDefaultAsync(s => s.TagId == id);
        }

        public async Task AddTagAsync(FunewsManagementContext context, Tag tag)
        {
            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTagAsync(FunewsManagementContext context, Tag tag)
        {
            context.Tags.Update(tag);
            await context.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(FunewsManagementContext context, int id)
        {
            var tag = await context.Tags.FindAsync(id);
            if (tag != null)
            {
                context.Tags.Remove(tag);
                await context.SaveChangesAsync();
            }
        }

        public async Task<int> GetMaxIdAsync(FunewsManagementContext context)
        {
            var maxId = await context.Tags
                            .Select(x => (int?)x.TagId)
                            .MaxAsync();
            return maxId ?? 0;
        }

        public async Task<bool> NameExistsAsync(FunewsManagementContext context, string name)
        {
            return await context.Tags.AnyAsync(t => t.TagName == name);
        }
    }
}
