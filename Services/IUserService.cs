namespace TaskFlow.Services;

public interface IUserService
{
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser?> GetByIdAsync(int id);
}
