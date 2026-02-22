using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Models;

namespace Assignment1.Mapper
{
    public class MapHelperNewsArticle
    {
        public static NewsArticle FromNewsArticleRequestDto(NewsArticleRequestDto newsArticleRequestDto)
        {
            if (newsArticleRequestDto == null) { throw new ArgumentNullException(); }
            return new NewsArticle
            {
                NewsTitle = newsArticleRequestDto.NewsTitle,
                Headline = newsArticleRequestDto.Headline,
                NewsContent = newsArticleRequestDto.NewsContent,
                NewsSource = newsArticleRequestDto.NewsSource,
                CategoryId = newsArticleRequestDto.CategoryId,

                CreatedDate = DateTime.Now,
                NewsStatus = true,
            };
        }
        public static void MapUpdateDto(NewsArticle entity,UpdateNewArticleRequestDto dto)
        {
            if (entity == null) throw new ArgumentNullException();
            if (dto == null) throw new ArgumentNullException();

            entity.NewsTitle = dto.NewsTitle;
            entity.Headline = dto.Headline;
            entity.NewsContent = dto.NewsContent;
            entity.NewsSource = dto.NewsSource;
            entity.CategoryId = dto.CategoryId;

            entity.UpdatedById =  1;
            entity.ModifiedDate = DateTime.Now;
        }
        public static NewsArticleResponseDto GetResponseFromOrigin(NewsArticle a)
        {
            if(a == null)
            {
                return null;
            }
            return new NewsArticleResponseDto
            {
                NewsArticleId = a.NewsArticleId,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                NewsContent = a.NewsContent,
                NewsSource = a.NewsSource,
                CreatedDate = a.CreatedDate,
                ModifiedDate = a.ModifiedDate,
                NewsStatus = a.NewsStatus,
                CreatedById = a.CreatedById,
                CreatedByName = a.CreatedBy?.AccountName,
                CategoryName = a.Category != null ? a.Category.CategoryName : null,
                Tags = a.Tags.Select(t => t.TagId).ToList()
            };
        }
    }
}
