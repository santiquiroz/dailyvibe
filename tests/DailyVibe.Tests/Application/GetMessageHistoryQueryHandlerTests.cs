using DailyVibe.Application.Common;
using DailyVibe.Application.Interfaces;
using DailyVibe.Application.Messages;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DailyVibe.Tests.Application;

public sealed class GetMessageHistoryQueryHandlerTests
{
    [Fact]
    public async Task Maps_the_requested_page_of_messages_to_dtos()
    {
        var userId = Guid.NewGuid();
        var message = new DailyMessage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Content = "Hola",
            Intent = "x",
            CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        var messages = new Mock<IDailyMessageRepository>();
        messages.Setup(m => m.GetPageAsync(userId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<DailyMessage>([message], 2, 5, 6));

        var result = await new GetMessageHistoryQueryHandler(messages.Object)
            .Handle(new GetMessageHistoryQuery(userId, 2, 5), CancellationToken.None);

        result.Page.Should().Be(2);
        result.Size.Should().Be(5);
        result.TotalCount.Should().Be(6);
        result.Items.Should().Equal(new DailyMessageDto(message.Id, "Hola", "x", message.CreatedAt));
    }
}
