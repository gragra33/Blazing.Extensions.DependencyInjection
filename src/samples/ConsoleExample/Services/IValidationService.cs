namespace ConsoleExample.Services;

/// <summary>
/// Validation service interface for simple data validation.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates the provided data string.
    /// </summary>
    /// <param name="data">The data to validate.</param>
    /// <returns>True if the data is valid; otherwise false.</returns>
    bool ValidateData(string data);
}