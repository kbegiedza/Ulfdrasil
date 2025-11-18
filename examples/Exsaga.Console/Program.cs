using Exsaga.Console.Daemons;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<GreeterDaemon>();

var host = builder.Build();

await host.RunAsync();