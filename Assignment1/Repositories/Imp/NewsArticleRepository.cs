using Assignment1.DAO;
using Assignment1.Models;
using Assignment1.Repositories.Interface;

namespace Assignment1.Repositories.Imp
{
    public class NewsArticleRepository : INewsArticleRepository
    {
        private readonly FunewsManagementContext _context;

        public NewsArticleRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task<List<NewsArticle>> GetNewsArticlesAsync()
        {
            return await NewsArticleDAO.Instance.GetNewsArticlesAsync(_context);
        }

        public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id)
        {
            return await NewsArticleDAO.Instance.GetNewsArticleByIdAsync(_context, id);
        }

        public async Task<NewsArticle?> GetNewsArticleByIdNoTrackingAsync(string id)
        {
            return await NewsArticleDAO.Instance.GetNewsArticleByIdNoTrackingAsync(_context, id);
        }

        public async Task<NewsArticle> AddNewsArticleAsync(NewsArticle article)
        {
            await NewsArticleDAO.Instance.AddNewsArticleAsync(_context, article);
            return article;
        }

        public async Task UpdateNewsArticleAsync(NewsArticle article)
        {
            await NewsArticleDAO.Instance.UpdateNewsArticleAsync(_context, article);
        }

        public async Task DeleteNewsArticleAsync(string id)
        {
            await NewsArticleDAO.Instance.DeleteNewsArticleAsync(_context, id);
        }

        public IQueryable<NewsArticle> GetNewsArticlesQueryable()
        {
            return NewsArticleDAO.Instance.GetNewsArticlesQueryable(_context);
        }

        public async Task<string> GetNextIdAsync()
        {
            return await NewsArticleDAO.Instance.GetNextIdAsync(_context);
        }
    }
}
