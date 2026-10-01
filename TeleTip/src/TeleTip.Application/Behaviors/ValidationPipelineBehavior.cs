using FluentValidation;
using MediatR;
using MyProject.Domain.Abstractions;

namespace MyProject.Application.Behaviors
{
    // AOP: Validation Pipeline Behavior
    public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Result
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var error = Error.Validation("Validation", string.Join(", ", failures.Select(f => f.ErrorMessage)));

                var resultType = typeof(TResponse);

                if (resultType == typeof(Result))
                    return (TResponse)(object)Result.Failure(error);

                var valueType = resultType.GetGenericArguments()[0];
                var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
                    .MakeGenericMethod(valueType);

                return (TResponse)failureMethod.Invoke(null, [error])!;
            }

            return await next();
        }
    }
}
