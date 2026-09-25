using DailyVibe.Api.Authentication;
using DailyVibe.Api.Contracts;
using DailyVibe.Application.Preferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DailyVibe.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/preferences")]
public sealed class PreferencesController(ISender sender) : ControllerBase
{
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdatePreferencesCommand(User.GetUserId(), request.DefaultIntent), cancellationToken);
        return NoContent();
    }
}
