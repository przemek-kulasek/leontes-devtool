namespace Leontes.DevTool.Domain.Common;

/// <summary>Base for violated domain contracts. Use for genuinely exceptional states, never for control flow.</summary>
public abstract class DomainException(string message) : Exception(message);

/// <summary>A requested aggregate could not be found.</summary>
public sealed class NotFoundException(string name, object key)
    : DomainException($"{name} with key '{key}' was not found.");

/// <summary>One or more invariants were violated.</summary>
public sealed class ValidationException(IReadOnlyList<string> errors)
    : DomainException("One or more validation errors occurred.")
{
    public IReadOnlyList<string> Errors { get; } = errors;

    public ValidationException(string error) : this([error]) { }
}
