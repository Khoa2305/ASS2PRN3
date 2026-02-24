using Assignment1.Models;
using Assignment1.dto.response;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FUNewsManagement_AIAPI.Services
{
    public interface IAIService
    {
        List<TagResponseDto> GetSuggestedTags(string content);
        void LearnTag(string tagName);
    }

    public class AIService : IAIService
    {
        private readonly FunewsManagementContext _context;

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the","a","an","and","or","but",
            "in","on","at","to","for",
            "with","is","are","of","it",
            "that","this","from","by",
            "as","be","was","were"
        };

        // Learning Cache: TagName -> SelectionCount
        private static readonly ConcurrentDictionary<string, int> TagLearningCache
            = new(StringComparer.OrdinalIgnoreCase);

        public AIService(FunewsManagementContext context)
        {
            _context = context;
        }

        public List<TagResponseDto> GetSuggestedTags(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<TagResponseDto>();

            // Chuẩn hoá content: lowercase, bỏ ký tự đặc biệt
            string normalizedContent = Regex.Replace(content.ToLower(), @"[^\w\s]", " ");

            // Tách content thành tập các token
            var contentTokens = normalizedContent
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Lấy tất cả tag từ DB
            var tagsFromDb = _context.Tags
                .Include(t => t.NewsArticles)
                .Where(t => t.TagName != null)
                .ToList();

            var result = new List<TagResponseDto>();

            foreach (var tag in tagsFromDb)
            {
                string tagLower = tag.TagName!.Trim().ToLower();

                // Tách tên tag thành các từ (hỗ trợ tag nhiều chữ vd: "Artificial Intelligence")
                var tagWords = Regex.Replace(tagLower, @"[^\w\s]", " ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 1 && !StopWords.Contains(w))
                    .ToList();

                if (!tagWords.Any()) continue;

                // --- Tiêu chí khớp ---
                // 1. Substring match: nếu tên tag xuất hiện trong content (nguyên chuỗi)
                bool substringMatch = normalizedContent.Contains(tagLower,
                    StringComparison.OrdinalIgnoreCase);

                // 2. Token match: % các từ của tag xuất hiện trong content
                int tokenHits = tagWords.Count(w => contentTokens.Contains(w));
                double tokenRatio = (double)tokenHits / tagWords.Count;

                // Lấy tag nếu tên xuất hiện nguyên chuỗi HOẶC ≥50% từ của tag khớp
                if (!substringMatch && tokenRatio < 0.5) continue;

                // Tính confidence: ưu tiên substring match, sau đó theo token ratio
                double baseConfidence = substringMatch ? 1.0 : tokenRatio;

                // Learning boost
                int selections = TagLearningCache.GetValueOrDefault(tag.TagName!, 0);
                double learningBoost = Math.Min(selections * 0.01, 0.20);

                double finalConfidence = Math.Min(baseConfidence + learningBoost, 1.0);

                result.Add(new TagResponseDto
                {
                    TagId         = tag.TagId,
                    TagName       = tag.TagName,
                    Note          = tag.Note,
                    // Dùng NumberArticle để truyền confidence (x100 để FE chia lại)
                    // Thực ra FE dùng NumberArticle riêng nên ta để nguyên count bài viết
                    NumberArticle = (int)Math.Round(finalConfidence * 100),
                });
            }

            // Sắp xếp theo confidence (NumberArticle đang lưu confidence*100), trả về top 10
            return result
                .OrderByDescending(t => t.NumberArticle)
                .Take(10)
                .ToList();
        }

        public void LearnTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;
            TagLearningCache.AddOrUpdate(tagName, 1, (_, val) => val + 1);
        }
    }
}