using Assignment1.Models;

namespace Assignment1.Repositories.Interface
{
    public interface ITagRepository
    {
        Task<List<Tag>> GetTagsAsync();
        Task<Tag?> GetTagByIdAsync(int id);
        Task<Tag> AddTagAsync(Tag tag);
        Task UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(int id);
        Task<int> GetMaxIdAsync();
        Task<bool> NameExistsAsync(string name);
    }
}
