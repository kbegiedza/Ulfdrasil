using System.Security.Claims;

using Microsoft.Extensions.Logging;

namespace Ulfdrasil.Web.Mvc;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Base controller for API endpoints. Provides a logger instance for derived controllers.
/// </summary>
[ApiController]
[Route(RoutePrefix)]
public abstract class ApiController : ControllerBase
{
    private const string RoutePrefix = "/api/v{version:apiVersion}/[controller]";

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to be used by the controller.</param>
    protected ApiController(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger instance for the controller.
    /// </summary>
    protected ILogger Logger { get; private set; }
}