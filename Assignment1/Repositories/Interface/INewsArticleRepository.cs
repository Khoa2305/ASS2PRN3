using Assignment1.Models;

namespace Assignment1.Repositories.Interface
{
    public interface INewsArticleRepository
    {
        Task<List<NewsArticle>> GetNewsArticlesAsync();
        Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
        Task<NewsArticle?> GetNewsArticleByIdNoTrackingAsync(string id);
        Task<NewsArticle> AddNewsArticleAsync(NewsArticle article);
        Task UpdateNewsArticleAsync(NewsArticle article);
        Task DeleteNewsArticleAsync(string id);
        IQueryable<NewsArticle> GetNewsArticlesQueryable();
        Task<string> GetNextIdAsync();
    }
}
