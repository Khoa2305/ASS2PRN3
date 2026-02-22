namespace Assignment1.dto.response
{
    public class ReportResponseDto
    {
        public List<GroupedStatDto> ByCategory { get; set; } = new();
        public List<GroupedStatDto> ByAuthor { get; set; } = new();
        public int TotalActive { get; set; }
        public int TotalInactive { get; set; }
    }

    public class GroupedStatDto
    {
        public string Key { get; set; } = null!;
        public int Count { get; set; }
    }
}
