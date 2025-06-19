using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;

namespace RunGroopWebApp.Repository
{
    public class RaceRepository : IRaceRepository
    {
        private readonly ApplicationDbContext _context; // Changed from ApplicationDBContext

        public RaceRepository(ApplicationDbContext context) // Changed from ApplicationDBContext
        {
            _context = context;
        }

        public bool Add(Race race)
        {
            _context.Add(race);
            return Save();
        }

        public bool Delete(Race race)
        {
            _context.Remove(race);
            return Save();
        }

        public async Task<IEnumerable<Race>> GetAll()
        {
            return await _context.Races.Include(r => r.Address).ToListAsync();
        }

        public async Task<IEnumerable<Race>> GetAllRacesByCity(string city)
        {
            return await _context.Races.Include(r => r.Address).Where(c => c.Address.City.Contains(city)).ToListAsync();
        }

        public async Task<Race?> GetByIdAsync(int id)
        {
            return await _context.Races.Include(r => r.Address).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Race?> GetByIdAsyncNoTracking(int id)
        {
            return await _context.Races.Include(r => r.Address).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id); // Fixed - added WHERE clause
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Races.CountAsync();
        }

        public async Task<int> GetCountByCategoryAsync(RaceCategory category)
        {
            return await _context.Races.CountAsync(r => r.RaceCategory == category);
        }

        public async Task<IEnumerable<Race>> GetRacesByCategoryAndSliceAsync(RaceCategory category, int offset, int size)
        {
            return await _context.Races.Include(r => r.Address)
                .Where(r => r.RaceCategory == category)
                .Skip(offset)
                .Take(size)
                .ToListAsync();
        }

        public async Task<IEnumerable<Race>> GetSliceAsync(int offset, int size)
        {
            return await _context.Races.Include(r => r.Address)
                .Skip(offset)
                .Take(size)
                .ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        public bool Update(Race race)
        {
            _context.Update(race);
            return Save();
        }
    }
}