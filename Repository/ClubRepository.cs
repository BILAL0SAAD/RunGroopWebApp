using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;

namespace RunGroopWebApp.Repository
{
    public class ClubRepository : IClubRepository
    {
        private readonly ApplicationDbContext _context;

        public ClubRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Club>> GetAll()
        {
            return await _context.Clubs.Include(c => c.Address).ToListAsync();
        }

        public async Task<Club?> GetByIdAsync(int id) // Changed return type to nullable
        {
            return await _context.Clubs.Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Club?> GetByIdAsyncNoTracking(int id)
        {
            return await _context.Clubs.Include(c => c.Address).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Club>> GetClubByCity(string city)
        {
            return await _context.Clubs.Include(c => c.Address).Where(c => c.Address.City.Contains(city)).ToListAsync();
        }

        public bool Add(Club club)
        {
            _context.Add(club);
            return Save();
        }

        public bool Update(Club club)
        {
            _context.Update(club);
            return Save();
        }

        public bool Delete(Club club)
        {
            _context.Remove(club);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }
    }
}