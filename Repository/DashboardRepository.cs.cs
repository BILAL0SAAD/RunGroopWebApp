using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;


namespace RunGroopWebApp.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Club>> GetAllUserClubs()
        {
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

            if (string.IsNullOrEmpty(curUserId))
            {
                return new List<Club>();
            }

            var userClubs = await _context.Clubs
                .Include(c => c.Address)
                .Where(c => c.AppUserId == curUserId)
                .ToListAsync();

            return userClubs;
        }

        public async Task<List<Race>> GetAllUserRaces()
        {
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

            if (string.IsNullOrEmpty(curUserId))
            {
                return new List<Race>();
            }

            var userRaces = await _context.Races
                .Include(r => r.Address)
                .Where(r => r.AppUserId == curUserId)
                .ToListAsync();

            return userRaces;
        }

        public async Task<AppUser> GetUserById(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetByIdNoTracking(string id)
        {
            return await _context.Users.Where(u => u.Id == id).AsNoTracking().FirstOrDefaultAsync();
        }

        public bool Update(AppUser user)
        {
            _context.Users.Update(user);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}