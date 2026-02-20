using FluentValidation.Results;

namespace ApiTemplate.Application.Validation;

/// <summary>
/// Interface for request/command/query types that carry their own validation logic.
/// Implementors instantiate their specific validator and invoke it inside <see cref="Validate"/>.
/// The validation filter calls this method before the action runs and short-circuits with a
/// ProblemResult (400) when the result is invalid.
/// </summary>
public interface IValidatableRequest
{
    ValidationResult Validate();
}
