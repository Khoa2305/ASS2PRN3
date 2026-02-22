using Assignment1.DAO;
using Assignment1.Models;
using Assignment1.Repositories.Interface;

namespace Assignment1.Repositories.Imp
{
    public class TagRepository : ITagRepository
    {
        private readonly FunewsManagementContext _context;

        public TagRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            return await TagDAO.Instance.GetTagsAsync(_context);
        }

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await TagDAO.Instance.GetTagByIdAsync(_context, id);
        }

        public async Task<Tag> AddTagAsync(Tag tag)
        {
            await TagDAO.Instance.AddTagAsync(_context, tag);
            return tag;
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            await TagDAO.Instance.UpdateTagAsync(_context, tag);
        }

        public async Task DeleteTagAsync(int id)
        {
            await TagDAO.Instance.DeleteTagAsync(_context, id);
        }

        public async Task<int> GetMaxIdAsync()
        {
            return await TagDAO.Instance.GetMaxIdAsync(_context);
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await TagDAO.Instance.NameExistsAsync(_context, name);
        }
    }
}
