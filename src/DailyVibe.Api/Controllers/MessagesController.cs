using DailyVibe.Api.Authentication;
using DailyVibe.Api.Contracts;
using DailyVibe.Application.Common;
using DailyVibe.Application.Messages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DailyVibe.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public sealed class MessagesController(ISender sender) : ControllerBase
{
    [HttpGet("today")]
    public Task<DailyMessageDto> Today(CancellationToken cancellationToken) =>
        sender.Send(new GetTodayMessageQuery(User.GetUserId()), cancellationToken);

    [HttpPost("generate")]
    public Task<DailyMessageDto> Generate(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] GenerateMessageRequest? request,
        CancellationToken cancellationToken) =>
        sender.Send(new GenerateMessageCommand(User.GetUserId(), request?.Intent), cancellationToken);

    [HttpGet("history")]
    public Task<PagedResult<DailyMessageDto>> History(
        int page = 1,
        int size = MessageRules.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        sender.Send(new GetMessageHistoryQuery(User.GetUserId(), page, size), cancellationToken);
}
