using DailyVibe.Domain.Entities;

namespace DailyVibe.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<bool> UpdateDefaultIntentAsync(Guid userId, string defaultIntent, CancellationToken cancellationToken);
}
