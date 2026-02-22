using Assignment1.dto;
using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Mapper;
using Assignment1.Models;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Assignment1.Controllers
{
    [Route("api/tag")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TagResponseDto>>>> GetTags()
        {
            List<Tag> tags = await _tagService.GetTagsAsync();
            return Ok(new ApiResponse<List<TagResponseDto>>
            {
                Success = true,
                Message = "Get tags success",
                Data = tags.Select(s => TagMapHelper.TagResponseDtoFromOrigin(s)).ToList()
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TagDetailResponseDto>>> GetDetail([FromRoute] int id)
        {
            Tag tag = await _tagService.GetTagByIdAsync(id);
            return Ok(new ApiResponse<TagDetailResponseDto>
            {
                Success = true,
                Message = "Get detail tag success",
                Data = TagMapHelper.DetailFromOrigin(tag)
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Tag>>> CreateTag([FromBody] TagCreateRequestDto requestDto)
        {
            Tag tag = TagMapHelper.OriginFromCreateRequest(requestDto);
            await _tagService.AddTagAsync(tag);
            return Ok(new ApiResponse<Tag>
            {
                Message = "Create Tag success",
                Success = true,
                Data = tag
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> Delete([FromRoute] int id)
        {
            await _tagService.DeleteTagAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Delete tag success"
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Tag>> UpdateTag([FromBody] UpdateTagRequestDto requestDto, int id)
        {
            Tag tag = await _tagService.GetTagByIdAsync(id);
            TagMapHelper.MapUpdateToOrigin(requestDto, tag);
            await _tagService.UpdateTagAsync(tag);
            return Ok(new ApiResponse<Tag>
            {
                Success = true,
                Message = "Update tag success",
                Data = tag
            });
        }
    }
}
