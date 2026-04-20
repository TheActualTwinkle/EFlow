namespace EFlow.Common.Domain;

/// <summary>
/// Represents a business rule that can be checked for being broken.
/// If a rule is broken, it means that some invariant of the domain model is violated.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// A message describing the rule that is broken.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Checks if the rule is broken.
    /// </summary>
    /// <returns><c>true</c> if the rule is broken, <c>false</c> otherwise.</returns>
    public bool IsBroken();
}