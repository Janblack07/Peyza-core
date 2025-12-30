using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peyza.Core.Infrastructure.Api;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Peyza.Core;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
#if DEBUG
            .WriteTo.Async(c => c.Console())
#endif
            .CreateLogger();

        try
        {
            Log.Information("Starting Peyza.Core.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host
                .UseAutofac()
                .UseSerilog();
            await builder.Services.AddApplicationAsync<CoreHttpApiHostModule>(options =>
            {
                options.Services.ReplaceConfiguration(builder.Configuration);
            });
            var app = builder.Build();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<AbpErrorToApiEnvelopeMiddleware>();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;            
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}