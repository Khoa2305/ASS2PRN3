using Assignment1.Models;

namespace Assignment1.Services.Interface
{
    public interface INewsArticleService
    {
        public List<NewsArticle> GetNewsArticles();
        public NewsArticle Add(NewsArticle newNewsArticle, List<int>? tagIds);
        public void Update(NewsArticle newNewsArticle, List<int>? tagIds);
        public NewsArticle Find(string id);
        public List<NewsArticle> GetRelatedArticles(string id);
        public void Delete(string id);
        public NewsArticle Duplicate(string id, short createdById);
    }
}
