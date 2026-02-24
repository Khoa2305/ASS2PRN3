using FUNewsManagement_FE.Clients;
using FUNewsManagement_FE.dto.request;
using Microsoft.AspNetCore.Mvc;
using UI.dto.response;

namespace FUNewsManagement_FE.Controllers
{
    [Route("AIProxy")]
    public class AIProxyController : Controller
    {
        private readonly IAIApiClient _aiClient;

        public AIProxyController(IAIApiClient aiClient)
        {
            _aiClient = aiClient;
        }

        [HttpPost("SuggestTags")]
        public async Task<IActionResult> SuggestTags([FromBody] SuggestTagsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Content is required for tag suggestion.");

            try
            {
                // AIAPI trả về List<TagResponseDto> (array JSON)
                var tags = await _aiClient
                    .SendAndDeserializeAsync<List<TagResponseDto>>(
                        HttpMethod.Post,
                        "api/ai/suggest-tags",
                        request);

                if (tags != null && tags.Any())
                {
                    // AIService lưu confidence*100 vào NumberArticle
                    var data = tags.Select(t => new
                    {
                        name       = t.TagName ?? string.Empty,
                        confidence = Math.Round(t.NumberArticle / 100.0, 2)
                    });

                    return Json(new { success = true, data });
                }

                return Json(new { success = false, message = "No tags matched the content." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
