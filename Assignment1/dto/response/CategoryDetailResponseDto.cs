namespace Assignment1.dto.response
{
    public class CategoryDetailResponseDto
    {
        public short CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string CategoryDesciption { get; set; } = null!;

        public bool? IsActive { get; set; }

        public int NumberArticle { get; set; } = 0;
        public bool CanChangeParent { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
