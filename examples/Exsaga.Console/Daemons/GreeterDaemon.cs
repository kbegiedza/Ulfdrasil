using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Exsaga.Console.Daemons;

public class GreeterDaemon : BackgroundService
{
    private readonly ILogger<GreeterDaemon> _logger;

    public GreeterDaemon(ILogger<GreeterDaemon> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Hello! Current time is: {time}", DateTimeOffset.Now);

            await Task.Delay(1000, stoppingToken);
        }
    }
}