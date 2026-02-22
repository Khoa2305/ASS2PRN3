using Assignment1.dto.request;
using Assignment1.dto;
using Assignment1.Mapper;
using Assignment1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Assignment1.Services.Interface;
using Assignment1.dto.response;
using Assignment1.Constant;
using Assignment1.CustomException;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment1.Hubs;

namespace Assignment1.Controllers
{
    [Route("api/newsarticles")]
    [ApiController]
    [Authorize]
    public class NewsArticleApiController : ControllerBase
    {
        private readonly INewsArticleService _articleService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NewsArticleApiController(INewsArticleService newsArticleService, IHubContext<NotificationHub> hubContext)
        {
            _articleService = newsArticleService;
            _hubContext = hubContext;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<List<NewsArticleResponseDto>>> GetPublicArticles([FromQuery] DateTime? date, [FromQuery] string? title)
        {
            var query = _articleService.GetNewsArticles()
                            .Where(a => a.NewsStatus == true);

            if (date.HasValue)
            {
                query = query.Where(a => a.CreatedDate.HasValue && a.CreatedDate.Value.Date == date.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                var keyword = title.Trim().ToLower();

                query = query.Where(a =>
                    a.NewsTitle != null &&
                    a.NewsTitle.ToLower().Contains(keyword)
                );
            }


            var articles = query.OrderByDescending(a => a.CreatedDate)
                            .Select(a => MapHelperNewsArticle.GetResponseFromOrigin(a))
                            .ToList();
            
            return Ok(new ApiResponse<List<NewsArticleResponseDto>>
            {
                Success = true,
                Message = "Get public articles success",
                Data = articles
            });
        }

        [HttpGet("public/{id}")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<NewsArticleDetailResponseDto>> GetPublicDetail([FromRoute] string id)
        {
            return GetDetail(id);
        }
        [HttpPost]
        [Authorize(Roles = "1,Admin")]
        public ActionResult<ApiResponse<NewsArticle>> Add([FromBody] NewsArticleRequestDto article)
        {
            var entity = MapHelperNewsArticle.FromNewsArticleRequestDto(article);
            
            // Set CreatedById from current user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId) && userId != 999)
            {
                entity.CreatedById = userId;
            }

            var created = _articleService.Add(entity, article.TagIds);

            // SignalR: Broadcast new article notification
            var msg = new { 
                Title = entity.NewsTitle, 
                Author = entity.CreatedBy?.AccountName ?? "Someone", 
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
            };
            _hubContext.Clients.All.SendAsync("ReceiveNotification", msg);

            return Created(
                $"/api/newsarticle",
                new ApiResponse<NewsArticle>
                {
                    Success = true,
                    Message = "Add success",
                    Data = created
                }
            );
        }

        [HttpPost("{id}/duplicate")]
        [Authorize(Roles = "1,Admin")]
        public ActionResult<ApiResponse<NewsArticle>> Duplicate([FromRoute] string id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                var duplicated = _articleService.Duplicate(id, userId);
                return Ok(new ApiResponse<NewsArticle>
                {
                    Success = true,
                    Message = "Duplicate success",
                    Data = duplicated
                });
            }
            return Unauthorized();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "1,Admin")]
        public ActionResult<ApiResponse<NewsArticleResponseDto>> Add([FromBody] UpdateNewArticleRequestDto requestDto,[FromRoute]string id)
        {
            NewsArticle a = _articleService.Find(id);
            MapHelperNewsArticle.MapUpdateDto(a, requestDto);

            // Set UpdatedById and ModifiedDate for Staff or Admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                if (userId != 999)
                {
                    a.UpdatedById = userId;
                }
                else
                {
                    a.UpdatedById = null;
                }
                a.ModifiedDate = DateTime.Now;
            }

            _articleService.Update(a, requestDto.TagIds);
            return Created(
                $"/api/newsarticle/${a.NewsArticleId}",
                new ApiResponse<NewsArticleResponseDto>
                {
                    Success = true,
                    Message = "Update success",
                    Data = MapHelperNewsArticle.GetResponseFromOrigin(a)
                }
            );
        }
        [HttpGet("{id}")]
        public ActionResult<ApiResponse<NewsArticleDetailResponseDto>> GetDetail([FromRoute] string id)
        {
            NewsArticle a = _articleService.Find(id);
            var related = _articleService.GetRelatedArticles(id);

            // Increment view count (fire-and-forget style — non-blocking)
            _articleService.IncrementViewCount(id);

            var response = new NewsArticleDetailResponseDto
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
                Tags = a.Tags.Select(t => t.TagId).ToList(),
                RelatedArticles = related.Select(r => MapHelperNewsArticle.GetResponseFromOrigin(r)).ToList()
            };

            return Ok(
                new ApiResponse<NewsArticleDetailResponseDto>
                {
                    Success = true,
                    Message = "Get detail success",
                    Data = response
                }
            );
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "1,Admin")]
        public ActionResult<ApiResponse<NewsArticle>> DeleteAritcle([FromRoute] string id)
        {
            _articleService.Delete(id);
            return Ok(
                new ApiResponse<NewsArticle>
                {
                    Success = true,
                    Message = "Delete success",
                }
            );
        }

        /// <summary>Get top trending articles ordered by ViewCount descending.</summary>
        [HttpGet("trending")]
        [AllowAnonymous]
        public ActionResult<ApiResponse<List<NewsArticleResponseDto>>> GetTrending([FromQuery] int top = 5)
        {
            if (top < 1 || top > 100) top = 5;
            var articles = _articleService.GetTrendingArticles(top)
                .Select(a => MapHelperNewsArticle.GetResponseFromOrigin(a))
                .ToList();

            return Ok(new ApiResponse<List<NewsArticleResponseDto>>
            {
                Success = true,
                Message = $"Top {top} trending articles",
                Data = articles
            });
        }
    }
}
