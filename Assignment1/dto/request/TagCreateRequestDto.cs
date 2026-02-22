using System.ComponentModel.DataAnnotations;

namespace Assignment1.dto.request
{
    public class TagCreateRequestDto
    {
        [Required(ErrorMessage = "Tên tag không được để trống")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên tag phải từ 1–100 ký tự")]
        public string TagName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Note { get; set; }
    }
}
