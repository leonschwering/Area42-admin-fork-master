namespace Area42_1.ApiService.Data.Repositories;

using Area42_1.ApiService.Models.Admin;
using Microsoft.EntityFrameworkCore;

public interface IAdminRepository
{
    Task<AdminUser?> GetByIdAsync(string id);
    Task<AdminUser?> GetByEmailAsync(string email);
    Task<IEnumerable<AdminUser>> GetAllAsync();
    Task<AdminUser> CreateAsync(AdminUser user);
    Task<AdminUser> UpdateAsync(AdminUser user);
    Task<bool> DeleteAsync(string id);
}

public class AdminRepository : IAdminRepository
{
    private readonly Area42Context _context;

    public AdminRepository(Area42Context context)
    {
        _context = context;
    }

    public async Task<AdminUser?> GetByIdAsync(string id)
    {
        return await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == id && u.IsEnabled);
    }

    public async Task<AdminUser?> GetByEmailAsync(string email)
    {
        return await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email == email && u.IsEnabled && !u.IsLocked);
    }

    public async Task<IEnumerable<AdminUser>> GetAllAsync()
    {
        return await _context.AdminUsers.Where(u => u.IsEnabled && !u.IsLocked).ToListAsync();
    }

    public async Task<AdminUser> CreateAsync(AdminUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            user.Id = Guid.NewGuid().ToString();

        user.CreatedAt = DateTime.UtcNow;

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<AdminUser> UpdateAsync(AdminUser user)
    {
        _context.AdminUsers.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        user.IsEnabled = false;
        await UpdateAsync(user);
        return true;
    }
}
