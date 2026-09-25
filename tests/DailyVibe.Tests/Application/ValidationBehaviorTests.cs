using DailyVibe.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;

namespace DailyVibe.Tests.Application;

public sealed class ValidationBehaviorTests
{
    public sealed record Ping(string Name);

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(p => p.Name).NotEmpty();
    }

    private sealed class PingLengthValidator : AbstractValidator<Ping>
    {
        public PingLengthValidator() => RuleFor(p => p.Name).MaximumLength(3);
    }

    private sealed class PingPrefixValidator : AbstractValidator<Ping>
    {
        public PingPrefixValidator() => RuleFor(p => p.Name).Must(name => name.StartsWith('p'));
    }

    [Fact]
    public async Task Throws_validation_exception_before_invoking_the_handler()
    {
        var handlerInvoked = false;
        var behavior = new ValidationBehavior<Ping, string>([new PingValidator()]);

        var act = () => behavior.Handle(new Ping(""), _ => { handlerInvoked = true; return Task.FromResult("pong"); }, CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainSingle(e => e.PropertyName == nameof(Ping.Name));
        handlerInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task Aggregates_failures_from_every_validator()
    {
        var behavior = new ValidationBehavior<Ping, string>([new PingLengthValidator(), new PingPrefixValidator()]);

        var act = () => behavior.Handle(new Ping("toolong"), _ => Task.FromResult("pong"), CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>()).Which.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Invokes_the_handler_when_the_request_is_valid()
    {
        var behavior = new ValidationBehavior<Ping, string>([new PingValidator(), new PingLengthValidator(), new PingPrefixValidator()]);

        var result = await behavior.Handle(new Ping("pin"), _ => Task.FromResult("pong"), CancellationToken.None);

        result.Should().Be("pong");
    }

    [Fact]
    public async Task Invokes_the_handler_when_no_validator_is_registered()
    {
        var behavior = new ValidationBehavior<Ping, string>([]);

        var result = await behavior.Handle(new Ping(""), _ => Task.FromResult("pong"), CancellationToken.None);

        result.Should().Be("pong");
    }
}
