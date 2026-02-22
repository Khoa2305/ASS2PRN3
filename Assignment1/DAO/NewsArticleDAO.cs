using Assignment1.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.DAO
{
    public class NewsArticleDAO
    {
        private static NewsArticleDAO instance = null!;
        private static readonly object instanceLock = new object();

        private NewsArticleDAO() { }

        public static NewsArticleDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new NewsArticleDAO();
                    }
                    return instance;
                }
            }
        }

        public async Task<List<NewsArticle>> GetNewsArticlesAsync(FunewsManagementContext context)
        {
            return await context.NewsArticles
                .Include(s => s.Tags)
                .Include(s => s.Category)
                .Include(s => s.CreatedBy)
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetNewsArticleByIdAsync(FunewsManagementContext context, string id)
        {
            return await context.NewsArticles
                .Include(s => s.Category)
                .Include(s => s.Tags)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.NewsArticleId == id);
        }

        public async Task<NewsArticle?> GetNewsArticleByIdNoTrackingAsync(FunewsManagementContext context, string id)
        {
            return await context.NewsArticles
                .Include(s => s.Category)
                .Include(s => s.Tags)
                .Include(s => s.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.NewsArticleId == id);
        }

        public async Task AddNewsArticleAsync(FunewsManagementContext context, NewsArticle article)
        {
            await context.NewsArticles.AddAsync(article);
            await context.SaveChangesAsync();
        }

        public async Task UpdateNewsArticleAsync(FunewsManagementContext context, NewsArticle article)
        {
            context.NewsArticles.Update(article);
            await context.SaveChangesAsync();
        }

        public async Task DeleteNewsArticleAsync(FunewsManagementContext context, string id)
        {
            var article = await context.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.NewsArticleId == id);

            if (article != null)
            {
                article.Tags.Clear();
                context.NewsArticles.Remove(article);
                await context.SaveChangesAsync();
            }
        }

        public IQueryable<NewsArticle> GetNewsArticlesQueryable(FunewsManagementContext context)
        {
            return context.NewsArticles
                .Include(s => s.Tags)
                .Include(s => s.Category)
                .Include(s => s.CreatedBy);
        }

        public async Task<string> GetNextIdAsync(FunewsManagementContext context)
        {
            var ids = await context.NewsArticles.Select(a => a.NewsArticleId).ToListAsync();
            int maxId = 0;
            foreach (var id in ids)
            {
                if (int.TryParse(id, out int numericId))
                {
                    if (numericId > maxId)
                    {
                        maxId = numericId;
                    }
                }
            }
            return (maxId + 1).ToString();
        }
    }
}
