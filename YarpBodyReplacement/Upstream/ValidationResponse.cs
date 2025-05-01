namespace YarpBodyReplacement.Upstream;

/// <summary>
/// Represents the response from the validation endpoint
/// </summary>
public class ValidationResponse
{
    /// <summary>
    /// Indicates whether the validation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// List of error messages, only populated when Success is false
    /// </summary>
    public string[]? Errors { get; set; }
}
