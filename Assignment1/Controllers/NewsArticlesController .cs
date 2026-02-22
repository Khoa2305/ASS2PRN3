using Assignment1.dto;
using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Mapper;
using Assignment1.Models;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Assignment1.Controllers
{
    public class NewsArticlesController : ODataController
    {
        private readonly INewsArticleService _articleService;   
        public NewsArticlesController(INewsArticleService newsArticleService)
        {
            _articleService = newsArticleService;
        }
        [EnableQuery]
        public IActionResult Get(
            [FromQuery] string? search,
            [FromQuery] string? categoryName,
            [FromQuery] string? tagName,
            [FromQuery] short? createdById,
            [FromQuery] DateTime? createdDateStart,
            [FromQuery] DateTime? createdDateEnd)
        {
            var query = _articleService.GetNewsArticles().AsQueryable();

            // Global search
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(x => 
                    (x.NewsTitle != null && x.NewsTitle.ToLower().Contains(searchLower)) ||
                    (x.Headline != null && x.Headline.ToLower().Contains(searchLower)) ||
                    (x.NewsContent != null && x.NewsContent.ToLower().Contains(searchLower))
                );
            }

            // Filters
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(x => x.Category != null && x.Category.CategoryName == categoryName);
            }

            if (!string.IsNullOrEmpty(tagName))
            {
                query = query.Where(x => x.Tags.Any(t => t.TagName == tagName));
            }

            if (createdById.HasValue)
            {
                query = query.Where(x => x.CreatedById == createdById);
            }

            if (createdDateStart.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= createdDateStart.Value);
            }
             if (createdDateEnd.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= createdDateEnd.Value);
            }

            var result = query.Select(a => MapHelperNewsArticle.GetResponseFromOrigin(a)).ToList();

            return Ok(new ApiResponse<List<NewsArticleResponseDto>>
            {
                Data = result,
                Message = "Get success",
                Success = true
            });
        }

    }
}
