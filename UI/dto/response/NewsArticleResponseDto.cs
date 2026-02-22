using System.ComponentModel.DataAnnotations;

namespace UI.dto.response
{
    public class NewsArticleResponseDto
    {
        public string NewsArticleId { get; set; } = null!;

        [Required(ErrorMessage = "News title is required")]
        [StringLength(200, ErrorMessage = "News title max 200 characters")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline is required")]
        [StringLength(300, ErrorMessage = "Headline max 300 characters")]
        public string Headline { get; set; } = null!;

        [Required(ErrorMessage = "News content is required")]
        public string? NewsContent { get; set; }

        [StringLength(200, ErrorMessage = "News source max 200 characters")]
        public string? NewsSource { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool? NewsStatus { get; set; }

        public string? CategoryName { get; set; }
        public short? CategoryId { get; set; }
        public List<int> Tags { get; set; } = new();
    }
}
