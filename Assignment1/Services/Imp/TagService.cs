using Assignment1.Constant;
using Assignment1.CustomException;
using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Interface;

namespace Assignment1.Services.Imp
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<int> GetValidIdAsync()
        {
            int maxId = await _tagRepository.GetMaxIdAsync();
            return maxId + 1;
        }

        public async Task<Tag> AddTagAsync(Tag tag)
        {
            if (await _tagRepository.NameExistsAsync(tag.TagName))
            {
                throw new AppException(Fail.EXISTED_TAG);
            }
            tag.TagId = await GetValidIdAsync();
            return await _tagRepository.AddTagAsync(tag);
        }

        public async Task DeleteTagAsync(int id)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id);
            if (tag == null)
            {
                throw new AppException(Fail.FAIL_GET_TAG);
            }
            if (tag.NewsArticles.Count > 0)
            {
                throw new AppException(Fail.TAG_OWN_ARTICLE);
            }
            await _tagRepository.DeleteTagAsync(id);
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _tagRepository.GetTagsAsync();
        }

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id);
            if (tag == null)
            {
                throw new AppException(Fail.FAIL_GET_TAG);
            }
            return tag;
        }

        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            await _tagRepository.UpdateTagAsync(tag);
            return tag;
        }
    }
}
