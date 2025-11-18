using Microsoft.Extensions.Configuration;

namespace Ulfdrasil.Configuration.Extensions;

/// <summary>
/// Provides extension methods to retrieve typed configuration settings from an <see cref="IConfiguration"/>.
/// </summary>
public static class ConfigurationSettingsExtensions
{
    /// <summary>
    /// Reads a configuration section named after the type <typeparamref name="T"/> and binds it to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The settings type to bind.</typeparam>
    /// <param name="configuration">The configuration to read from.</param>
    /// <returns>An instance of <typeparamref name="T"/> if the section exists; otherwise <c>null</c>.</returns>
    public static T? GetSettings<T>(this IConfiguration configuration)
    {
        var section = configuration.GetSection(typeof(T).Name);

        return section.Get<T>();
    }

    /// <summary>
    /// Reads a child configuration section (parentSection:&lt;T&gt;) and binds it to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The settings type to bind.</typeparam>
    /// <param name="configuration">The configuration to read from.</param>
    /// <param name="parentSection">The parent section name that contains the section for <typeparamref name="T"/>.</param>
    /// <returns>An instance of <typeparamref name="T"/> if the section exists; otherwise <c>null</c>.</returns>
    public static T? GetSettings<T>(this IConfiguration configuration, string parentSection)
    {
        var section = configuration.GetSection(parentSection)
                                   .GetSection(typeof(T).Name);

        return section.Get<T>();
    }

    /// <summary>
    /// Reads a configuration section named after the type <typeparamref name="T"/> and binds it to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The settings type to bind.</typeparam>
    /// <param name="configuration">The configuration to read from.</param>
    /// <returns>An instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the configuration section for <typeparamref name="T"/> is not found.</exception>
    public static T GetRequiredSettings<T>(this IConfiguration configuration)
    {
        var settings = configuration.GetSection(typeof(T).Name)
                                    .Get<T>();

        if (settings != null)
        {
            return settings;
        }

        throw new KeyNotFoundException($"Unable to find {typeof(T).Name} in injected configuration");
    }

    /// <summary>
    /// Reads a child configuration section (parentSection:&lt;T&gt;) and binds it to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The settings type to bind.</typeparam>
    /// <param name="configuration">The configuration to read from.</param>
    /// <param name="parentSection">The parent section name that contains the section for <typeparamref name="T"/>.</param>
    /// <returns>An instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the configuration section for <paramref name="parentSection"/>:&lt;<typeparamref name="T"/>&gt; is not found.</exception>
    public static T GetRequiredSettings<T>(this IConfiguration configuration, string parentSection)
    {
        var settings = configuration.GetSection(parentSection)
                                    .GetSection(typeof(T).Name)
                                    .Get<T>();

        if (settings != null)
        {
            return settings;
        }

        throw new KeyNotFoundException($"Unable to find {parentSection}:{typeof(T).Name} in injected configuration");
    }
}