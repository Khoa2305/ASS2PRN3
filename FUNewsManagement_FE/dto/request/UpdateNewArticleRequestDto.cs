using System.ComponentModel.DataAnnotations;

namespace UI.dto.request
{
    public class UpdateNewArticleRequestDto
    {
        [StringLength(200, ErrorMessage = "NewsTitle tối đa 200 ký tự")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline là bắt buộc")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Headline phải từ 5 đến 500 ký tự")]
        public string Headline { get; set; } = null!;

        [StringLength(5000, ErrorMessage = "NewsContent tối đa 5000 ký tự")]
        public string? NewsContent { get; set; }

        [StringLength(255, ErrorMessage = "NewsSource tối đa 255 ký tự")]
        public string? NewsSource { get; set; }

        [Required(ErrorMessage = "CategoryId là bắt buộc")]
        [Range(1, short.MaxValue, ErrorMessage = "CategoryId không hợp lệ")]
        public short? CategoryId { get; set; }
        public List<int>? TagIds { get; set; }
    }
}
