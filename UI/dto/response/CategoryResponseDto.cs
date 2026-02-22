namespace UI.dto.response
{
    public class CategoryResponseDto
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryDesciption { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
