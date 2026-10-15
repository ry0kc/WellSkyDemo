using FluentValidation;
using MediatR;
using Xunit;

namespace ReferralDemo.Tests;

public class ValidationBehaviorTests
{
    public class Ping : IRequest<string>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(p => p.Name).NotEmpty();
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<Ping, string>([]);

        var result = await behavior.Handle(new Ping(), _ => Task.FromResult("pong"), default);

        Assert.Equal("pong", result);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsAndSkipsHandler()
    {
        var behavior = new ValidationBehavior<Ping, string>([new PingValidator()]);
        var handlerCalled = false;

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new Ping(), _ => { handlerCalled = true; return Task.FromResult("pong"); }, default));

        Assert.False(handlerCalled);
        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(Ping.Name));
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var behavior = new ValidationBehavior<Ping, string>([new PingValidator()]);

        var result = await behavior.Handle(new Ping { Name = "a" }, _ => Task.FromResult("pong"), default);

        Assert.Equal("pong", result);
    }
}
