using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Services;

public class UserService(ApplicationDbContext db) : IUserService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync() 
    {
       return await _db.Users.OrderBy(u => u.DisplayName).ToListAsync();
    }
    public async Task<ApplicationUser?> GetByIdAsync(int id) =>
        await _db.Users.FindAsync(id);
}
