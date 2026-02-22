namespace FUNewsManagement_FE.dto.response
{
    public class SuggestedTag
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class SuggestTagsResponse
    {
        public List<SuggestedTag> SuggestedTags { get; set; } = new();
    }
}
