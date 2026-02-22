namespace FUNewsManagement_AIAPI.Dto
{
    public class SuggestTagsRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public class SuggestedTag
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class SuggestTagsResponse
    {
        public List<SuggestedTag> SuggestedTags { get; set; } = new();
    }

    public class TagFeedbackRequest
    {
        public string TagName { get; set; } = string.Empty;
    }
}
