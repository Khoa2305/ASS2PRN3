using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Models;

namespace Assignment1.Mapper
{
    public class TagMapHelper
    {
        public static TagResponseDto TagResponseDtoFromOrigin(Tag tag)
        {
            if (tag == null) throw new ArgumentNullException();
            return new TagResponseDto
            {
                TagId = tag.TagId,
                TagName = tag.TagName,
                Note = tag.Note,
                NumberArticle = tag.NewsArticles?.Count ?? 0
            };
        }

        public static TagDetailResponseDto DetailFromOrigin(Tag tag)
        {
            if (tag == null) throw new ArgumentNullException();
            return new TagDetailResponseDto
            {
                TagId = tag.TagId,
                TagName = tag.TagName,
                Note = tag.Note,
                NumberArticle = tag.NewsArticles?.Count ?? 0
            };
        }

        public static Tag OriginFromCreateRequest(TagCreateRequestDto tagDto)
        {
            if (tagDto == null) throw new ArgumentNullException();
            return new Tag
            {
                TagName = tagDto.TagName,
                Note = tagDto.Note
            };
        }

        public static void MapUpdateToOrigin(UpdateTagRequestDto updateDto, Tag tag)
        {
            if (updateDto == null || tag == null) throw new ArgumentNullException();
            tag.TagName = updateDto.TagName;
            tag.Note = updateDto.Note;
        }
    }
}
