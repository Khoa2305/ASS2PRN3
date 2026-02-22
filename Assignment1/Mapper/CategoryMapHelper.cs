using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Models;

namespace Assignment1.Mapper
{
    public class CategoryMapHelper
    {
        public static CategoryResponseDto CategoryResponseDtoFromOrigin(Category ca)
        {
            if (ca == null) throw new ArgumentNullException();
            return new CategoryResponseDto
            {
                CategoryDesciption = ca.CategoryDesciption,
                CategoryName = ca.CategoryName,
                CategoryId = ca.CategoryId,
                IsActive = ca.IsActive,
                NumberArticle = ca.NewsArticles.Count,
            };
        }
        public static CategoryDetailResponseDto DetailFromOrigin(Category ca)
        {
            if(ca == null) throw new ArgumentNullException();
            return new CategoryDetailResponseDto
            {
                CanChangeParent = ca.NewsArticles.Count > 0 ? false : true,
                CategoryDesciption = ca.CategoryDesciption,
                CategoryName = ca.CategoryName,
                CategoryId = ca.CategoryId,
                IsActive = ca.IsActive,
                NumberArticle = ca.NewsArticles.Count,
                ParentCategoryId = (int)ca.ParentCategoryId,
            };
        }
        public static Category OriginFromCreateRequest(CategoryCreateRequestDto category)
        {
            if(category == null) throw new ArgumentNullException();
            return new Category
            {
                CategoryName = category.CategoryName,
                CategoryDesciption = category.CategoryDescription,
                ParentCategoryId= category.ParentCategoryId,
                IsActive = true,
            };
        }
        public static void MapUpdateToOrigin(UpdateCategoryRequestDto updateCategoryRequestDto, Category category)
        {
            category.CategoryDesciption = updateCategoryRequestDto.CategoryDescription;
            category.CategoryName = updateCategoryRequestDto.CategoryName;
            category.IsActive = updateCategoryRequestDto.IsActive;
            category.ParentCategoryId = updateCategoryRequestDto.ParentCategoryId;
        }
    }
}
