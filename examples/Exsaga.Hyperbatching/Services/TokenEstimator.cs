namespace Exsaga.Hyperbatching.Services;

/// <summary>
/// Provides a simple token estimation utility for examples.
/// </summary>
public static class TokenEstimator
{
    /// <summary>
    /// Estimates tokens from input length.
    /// </summary>
    /// <param name="input">The input text.</param>
    /// <returns>An estimated token count.</returns>
    public static int Estimate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return 0;
        }

        var estimated = (int)Math.Ceiling(input.Length / 4d);
        return Math.Max(1, estimated);
    }
}
