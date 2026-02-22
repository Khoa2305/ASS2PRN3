using FUNewsManagement_FE.Clients;
using FUNewsManagement_FE.dto.request;
using FUNewsManagement_FE.dto.response;
using Microsoft.AspNetCore.Mvc;

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
            {
                return BadRequest("Content is required for tag suggestion.");
            }

            try
            {
                var response = await _aiClient.SendAndDeserializeAsync<SuggestTagsResponse>(HttpMethod.Post, "api/ai/suggest-tags", request);
                
                if (response != null && response.SuggestedTags != null)
                {
                    return Json(new { success = true, data = response.SuggestedTags });
                }

                return Json(new { success = false, message = "No suggestions returned from AI." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
