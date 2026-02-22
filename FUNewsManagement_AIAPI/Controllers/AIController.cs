using FUNewsManagement_AIAPI.Dto;
using FUNewsManagement_AIAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_AIAPI.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("suggest-tags")]
        public IActionResult SuggestTags([FromBody] SuggestTagsRequest request)
        {
            var result = _aiService.GetSuggestedTags(request.Content);
            return Ok(result);
        }

        [HttpPost("provide-feedback")]
        public IActionResult ProvideFeedback([FromBody] TagFeedbackRequest request)
        {
            _aiService.LearnTag(request.TagName);
            return Ok(new { message = $"Learning confirmed for tag: {request.TagName}" });
        }
    }
}
