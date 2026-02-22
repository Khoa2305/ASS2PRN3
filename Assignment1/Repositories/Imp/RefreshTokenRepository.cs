using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Repositories.Imp
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly FunewsManagementContext _context;

        public RefreshTokenRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task RevokeAsync(RefreshToken token)
        {
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
