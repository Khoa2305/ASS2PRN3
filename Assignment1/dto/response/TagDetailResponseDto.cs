namespace Assignment1.dto.response
{
    public class TagDetailResponseDto
    {
        public int TagId { get; set; }

        public string? TagName { get; set; }

        public string? Note { get; set; }

        public int NumberArticle { get; set; } = 0;
    }
}
