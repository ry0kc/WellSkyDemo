using FluentValidation;
using MediatR;

namespace ReferralDemo;

// Runs every IValidator<TRequest> before the handler. Any failure throws a
// ValidationException, which ValidationExceptionHandler turns into a 400,
// so handlers only ever see requests that passed validation.
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();

        using var activity = Telemetry.ActivitySource.StartActivity("Validate");
        activity?.SetTag("request.type", typeof(TRequest).Name);

        var context = new ValidationContext<TRequest>(request);
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, ct);
            failures.AddRange(result.Errors);
        }

        activity?.SetTag("validation.failed", failures.Count > 0);
        if (failures.Count > 0)
        {
            activity?.SetTag("validation.error_count", failures.Count);
            throw new ValidationException(failures);
        }

        return await next();
    }
}
