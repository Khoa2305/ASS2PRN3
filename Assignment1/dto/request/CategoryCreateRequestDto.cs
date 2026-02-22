using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class CategoryCreateRequestDto
    {

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2–100 ký tự")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "Mô tả danh mục không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string CategoryDescription { get; set; } = null!;

        [Range(1, short.MaxValue, ErrorMessage = "ParentCategoryId không hợp lệ")]
        public short? ParentCategoryId { get; set; }
    }
}
