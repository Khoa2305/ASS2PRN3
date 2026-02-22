using FUNewsManagement_AIAPI.Dto;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FUNewsManagement_AIAPI.Services
{
    public interface IAIService
    {
        SuggestTagsResponse GetSuggestedTags(string content);
        void LearnTag(string tagName);
    }

    public class AIService : IAIService
    {
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "with", "is", "are", "of", "it", "that", "this", "from", "by", "as", "be", "was", "were"
        };

        // In-memory Learning Cache: TagName -> SelectionCount
        private static readonly ConcurrentDictionary<string, int> TagLearningCache = new(StringComparer.OrdinalIgnoreCase);

        public SuggestTagsResponse GetSuggestedTags(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new SuggestTagsResponse();

            // 1. Clean and tokenize content
            string cleaned = Regex.Replace(content.ToLower(), @"[^\w\s]", "");
            string[] tokens = cleaned.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // 2. Remove stop words & count frequency
            var wordFrequencies = tokens
                .Where(t => t.Length > 2 && !StopWords.Contains(t))
                .GroupBy(t => t)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToList();

            if (!wordFrequencies.Any())
                return new SuggestTagsResponse();

            int maxFreq = wordFrequencies.First().Count;

            // 3. Map to SuggestedTags with Confidence and apply Learning factor
            var suggestions = wordFrequencies.Select(wf =>
            {
                // Base confidence: frequency / maxFreq
                double baseConfidence = (double)wf.Count / maxFreq;

                // Learning factor: +0.01 per selection, capped at +0.20
                int selections = TagLearningCache.GetValueOrDefault(wf.Word, 0);
                double learningBoost = Math.Min(selections * 0.01, 0.20);

                double finalConfidence = Math.Min(baseConfidence + learningBoost, 1.0);

                return new SuggestedTag
                {
                    Name = wf.Word,
                    Confidence = Math.Round(finalConfidence, 2)
                };
            })
            .OrderByDescending(s => s.Confidence)
            .ToList();

            return new SuggestTagsResponse { SuggestedTags = suggestions };
        }

        public void LearnTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;
            TagLearningCache.AddOrUpdate(tagName.ToLower(), 1, (key, val) => val + 1);
        }
    }
}
