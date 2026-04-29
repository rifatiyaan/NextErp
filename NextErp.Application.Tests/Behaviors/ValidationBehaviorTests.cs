using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NextErp.Application.Common.Behaviors;
using NSubstitute;
using ValidationException = NextErp.Application.Common.Exceptions.ValidationException;

namespace NextErp.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public record DummyCommand(string Name) : IRequest<string>;

    private const string SentinelResponse = "OK";

    private static RequestHandlerDelegate<string> NextReturning(string value) =>
        ct => Task.FromResult(value);

    [Fact]
    public async Task No_validators_calls_next_and_returns_response()
    {
        var sut = new ValidationBehavior<DummyCommand, string>(Array.Empty<IValidator<DummyCommand>>());

        var nextCalls = 0;
        Task<string> Next(CancellationToken _)
        {
            nextCalls++;
            return Task.FromResult(SentinelResponse);
        }

        var result = await sut.Handle(new DummyCommand("anything"), Next, CancellationToken.None);

        result.Should().Be(SentinelResponse);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task Validator_passes_calls_next_once()
    {
        var validator = Substitute.For<IValidator<DummyCommand>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var sut = new ValidationBehavior<DummyCommand, string>(new[] { validator });

        var nextCalls = 0;
        Task<string> Next(CancellationToken _)
        {
            nextCalls++;
            return Task.FromResult(SentinelResponse);
        }

        var result = await sut.Handle(new DummyCommand("ok"), Next, CancellationToken.None);

        result.Should().Be(SentinelResponse);
        nextCalls.Should().Be(1);
        await validator.Received(1).ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validator_fails_throws_ValidationException_with_failure()
    {
        var validator = Substitute.For<IValidator<DummyCommand>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(DummyCommand.Name), "Name is required."),
            }));

        var sut = new ValidationBehavior<DummyCommand, string>(new[] { validator });

        var act = async () => await sut.Handle(new DummyCommand(""), NextReturning(SentinelResponse), CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        ex.Errors.Should().ContainKey(nameof(DummyCommand.Name));
        ex.Errors[nameof(DummyCommand.Name)].Should().Contain("Name is required.");
    }

    [Fact]
    public async Task Multiple_validators_all_run_and_failures_aggregate()
    {
        var v1 = Substitute.For<IValidator<DummyCommand>>();
        v1.ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "From v1"),
            }));

        var v2 = Substitute.For<IValidator<DummyCommand>>();
        v2.ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Other", "From v2"),
            }));

        var sut = new ValidationBehavior<DummyCommand, string>(new[] { v1, v2 });

        var act = async () => await sut.Handle(new DummyCommand("x"), NextReturning(SentinelResponse), CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        ex.Errors.Should().ContainKey("Name");
        ex.Errors.Should().ContainKey("Other");
        ex.Errors["Name"].Should().Contain("From v1");
        ex.Errors["Other"].Should().Contain("From v2");

        await v1.Received(1).ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>());
        await v2.Received(1).ValidateAsync(Arg.Any<ValidationContext<DummyCommand>>(), Arg.Any<CancellationToken>());
    }
}
