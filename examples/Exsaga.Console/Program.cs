using Exsaga.Console.EmbeddingsLoad;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Ulfdrasil.Configuration.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddCommandLine(args, EmbeddingsLoadSettings.CommandLineMappings);

builder.AddSettings<EmbeddingsLoadSettings>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<EmbeddingsLoadDaemon>();

var host = builder.Build();

await host.RunAsync();
