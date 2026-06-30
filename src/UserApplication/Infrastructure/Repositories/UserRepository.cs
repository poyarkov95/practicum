using Application.Persistence.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetUserByLogin(string login)
    {
        return await db.Users.FirstOrDefaultAsync(s => s.Login == login);
    }

    public async Task AddUserAsync(User user)
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task<User> GetUser(Guid userId)
    {
        return await db.Users.FirstOrDefaultAsync(s => s.Id == userId);
    }
}