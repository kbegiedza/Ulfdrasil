using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ulfdrasil.Configuration.Extensions;

/// <summary>
/// Provides extension methods to register configuration-backed settings into the host services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds configuration section named after <typeparamref name="T"/> to options of type <typeparamref name="T"/> and registers the bound instance as a singleton.
    /// </summary>
    /// <typeparam name="T">The settings type to bind and register. Must be a reference type.</typeparam>
    /// <param name="builder">The host application builder to extend.</param>
    /// <returns>The same <see cref="IHostApplicationBuilder"/> instance for chaining.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the configuration section for <typeparamref name="T"/> is not found.</exception>
    public static IHostApplicationBuilder AddSettings<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        var configuration = builder.Configuration;

        var config = configuration.GetRequiredSettings<T>();

        builder.Services
               .AddOptions<T>()
               .Bind(configuration.GetSection(typeof(T).Name));

        builder.Services.TryAddSingleton(config);

        return builder;
    }
}