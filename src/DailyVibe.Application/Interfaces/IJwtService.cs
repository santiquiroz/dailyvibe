using DailyVibe.Domain.Entities;

namespace DailyVibe.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
