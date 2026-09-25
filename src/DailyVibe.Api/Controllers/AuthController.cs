using DailyVibe.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DailyVibe.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public Task<AuthResult> Register(RegisterUserCommand command, CancellationToken cancellationToken) =>
        sender.Send(command, cancellationToken);

    [HttpPost("login")]
    public Task<AuthResult> Login(LoginCommand command, CancellationToken cancellationToken) =>
        sender.Send(command, cancellationToken);
}
