using Assignment1.Models;

namespace Assignment1.Services.Interface
{
    public interface ITagService
    {
        public Task<List<Tag>> GetTagsAsync();
        public Task<Tag> GetTagByIdAsync(int id);
        public Task<Tag> AddTagAsync(Tag tag);
        public Task DeleteTagAsync(int id);
        public Task<Tag> UpdateTagAsync(Tag tag);
        public Task<int> GetValidIdAsync();
    }
}
