using Assignment1.dto.response;

namespace Assignment1.dto.response
{
    public class NewsArticleDetailResponseDto : NewsArticleResponseDto
    {
        public List<NewsArticleResponseDto> RelatedArticles { get; set; } = new();
    }
}
