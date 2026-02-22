namespace Assignment1.dto.response
{
    public class NewsArticleResponseDto
    {
        public string NewsArticleId { get; set; } = null!;

        public string? NewsTitle { get; set; }

        public string Headline { get; set; } = null!;

        public string? NewsContent { get; set; }

        public string? NewsSource { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool? NewsStatus { get; set; }

        public short? CreatedById { get; set; }

        public string? CategoryName { get; set; }
        public short? CategoryId { get; set; }
        public string? CreatedByName { get; set; }
        public List<int> Tags { get; set; } = new();
    }
}
