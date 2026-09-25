using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyVibe.Tests.Authentication;

[ApiController]
[Authorize]
[Route("test/protected")]
public sealed class ProtectedTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
