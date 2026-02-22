using Assignment1.dto;
using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Mapper;
using Assignment1.Models;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Assignment1.Constant;
using Assignment1.CustomException;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    [Route("api/category")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetCategoories()
        {
            List<Category> categories =await  _categoryService.GetCategoriesAsync();
            return Ok(new ApiResponse<List<CategoryResponseDto>>
            {
                Success = true,
                Message = "Get categories success",
                Data = categories.Select(s=> CategoryMapHelper.CategoryResponseDtoFromOrigin(s)).ToList()
            });
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDetailResponseDto>>> GetDetail([FromRoute] int id)
        {
            Category category =await _categoryService.GetCategoryByIdAsync(id);
            return Ok(new ApiResponse<CategoryDetailResponseDto>
            {
                Success = true,
                Message = "Get detail category success",
                Data = CategoryMapHelper.DetailFromOrigin(category)
            });
        }
        [HttpPost]
        [Authorize(Roles = "1,Admin")]
        public async Task<ActionResult<ApiResponse<Category>>> CreateCategory([FromBody] CategoryCreateRequestDto requestDto)
        {
            Category category = CategoryMapHelper.OriginFromCreateRequest(requestDto);
            await _categoryService.AddCategoryAysnc(category);
            return Ok(new ApiResponse<Category>
            {
                Message = "Create Category success",
                Success = true,
                Data = category
            });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "1,Admin")]
        public async Task<ActionResult<object>> Delete([FromRoute] int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Delete category success"
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "1,Admin")]
        public async Task<ActionResult<Category>> UpdateCategory([FromBody] UpdateCategoryRequestDto requestDto, int id)
        {
            Category category =await _categoryService.GetCategoryByIdAsync(id);
            CategoryMapHelper.MapUpdateToOrigin(requestDto, category);
            await _categoryService.UpdateAsync(category);
            return Ok(new ApiResponse<Category>
            {
                Success = true,
                Message = "Update category success",
                Data = category
            });
        }
    }
}
