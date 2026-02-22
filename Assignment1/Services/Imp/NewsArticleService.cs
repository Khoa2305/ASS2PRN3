using Assignment1.Constant;
using Assignment1.CustomException;
using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Interface;

namespace Assignment1.Services.Imp
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly ITagRepository _tagRepository;

        public NewsArticleService(INewsArticleRepository newsArticleRepository, ITagRepository tagRepository)
        {
            _newsArticleRepository = newsArticleRepository;
            _tagRepository = tagRepository;
        }

        public NewsArticle Add(NewsArticle newNewsArticle, List<int>? tagIds)
        {
            // Assign Auto-Increment ID
            newNewsArticle.NewsArticleId = _newsArticleRepository.GetNextIdAsync().GetAwaiter().GetResult();

            if (tagIds != null)
            {
                // Note: Fetching and adding tags synchronously here because the original method was synchronous.
                // In a real scenario, we should probably make everything async.
                var tags = _tagRepository.GetTagsAsync().GetAwaiter().GetResult();
                var tagsToAdd = tags.Where(t => tagIds.Contains(t.TagId)).ToList();
                foreach (var tag in tagsToAdd)
                {
                    newNewsArticle.Tags.Add(tag);
                }
            }
            return _newsArticleRepository.AddNewsArticleAsync(newNewsArticle).GetAwaiter().GetResult();
        }

        public void Delete(string id)
        {
            var newsArticle = _newsArticleRepository.GetNewsArticleByIdAsync(id).GetAwaiter().GetResult();
            if (newsArticle != null)
            {
                _newsArticleRepository.DeleteNewsArticleAsync(id).GetAwaiter().GetResult();
            }
            else
            {
                throw new AppException(Fail.FAIL_GET_ARTICLE);
            }
        }

        public NewsArticle Find(string id)
        {
            var article = _newsArticleRepository.GetNewsArticleByIdAsync(id).GetAwaiter().GetResult();
            if (article == null)
            {
                throw new AppException(Fail.FAIL_GET_ARTICLE);
            }
            return article;
        }

        public List<NewsArticle> GetRelatedArticles(string id)
        {
            var currentArticle = Find(id);
            var tagIds = currentArticle.Tags.Select(t => t.TagId).ToList();

            var related = _newsArticleRepository.GetNewsArticlesQueryable()
                .Where(a => a.NewsArticleId != id && a.NewsStatus == true &&
                           (a.CategoryId == currentArticle.CategoryId ||
                            a.Tags.Any(t => tagIds.Contains(t.TagId))))
                .Take(3)
                .ToList();

            return related;
        }

        public List<NewsArticle> GetNewsArticles()
        {
            return _newsArticleRepository.GetNewsArticlesAsync().GetAwaiter().GetResult();
        }

        public void Update(NewsArticle newNewsArticle, List<int>? tagIds)
        {
            try
            {
                if (tagIds != null)
                {
                    // Remove tags
                    newNewsArticle.Tags.Clear();
                    
                    // Add new tags
                    var tags = _tagRepository.GetTagsAsync().GetAwaiter().GetResult();
                    var tagsToAdd = tags.Where(t => tagIds.Contains(t.TagId)).ToList();
                    foreach (var tag in tagsToAdd)
                    {
                        newNewsArticle.Tags.Add(tag);
                    }
                }

                _newsArticleRepository.UpdateNewsArticleAsync(newNewsArticle).GetAwaiter().GetResult();
            }
            catch
            {
                throw new AppException(Fail.FAIL_UPDATE);
            }
        }

        public NewsArticle Duplicate(string id, short createdById)
        {
            // Fetch original without tracking to avoid conflicts
            var original = _newsArticleRepository.GetNewsArticleByIdNoTrackingAsync(id).GetAwaiter().GetResult();
            if (original == null) throw new AppException(Fail.FAIL_GET_ARTICLE);

            // Create new instance with NEW ID
            var duplicated = new NewsArticle
            {
                NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20),
                NewsTitle = original.NewsTitle + " (Copy)",
                Headline = original.Headline,
                NewsContent = original.NewsContent,
                NewsSource = original.NewsSource,
                CategoryId = original.CategoryId,
                NewsStatus = original.NewsStatus,
                CreatedDate = DateTime.Now,
                CreatedById = createdById == 999 ? null : createdById,
                ModifiedDate = DateTime.Now,
                UpdatedById = createdById == 999 ? null : createdById,
                Tags = new List<Tag>() // Ensure fresh list
            };

            // Re-fetch tags from repo to ensure they are attached to the current context if needed
            // OR just use the IDs and let EF handle it via the many-to-many relationship
            if (original.Tags != null && original.Tags.Any())
            {
                var tagIds = original.Tags.Select(t => t.TagId).ToList();
                var allTags = _tagRepository.GetTagsAsync().GetAwaiter().GetResult();
                var tagsToAdd = allTags.Where(t => tagIds.Contains(t.TagId)).ToList();
                foreach (var tag in tagsToAdd)
                {
                    duplicated.Tags.Add(tag);
                }
            }

            return _newsArticleRepository.AddNewsArticleAsync(duplicated).GetAwaiter().GetResult();
        }
        public void IncrementViewCount(string id)
        {
            _newsArticleRepository.IncrementViewCountAsync(id).GetAwaiter().GetResult();
        }

        public List<NewsArticle> GetTrendingArticles(int top = 5)
        {
            return _newsArticleRepository.GetTrendingAsync(top).GetAwaiter().GetResult();
        }
    }
}
