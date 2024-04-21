using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Components;

public static class HostingExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r =>
            {
                r.AddService(serviceName);
            })
            .WithMetrics(m =>
            {
                m.AddMeter(DiagnosticHeaders.DefaultListenerName);
                m.AddView("messaging.masstransit.outbox.delivery", DiagnosticHeaders.DefaultListenerName);
                m.AddConsoleExporter((consoleCfg, metricCfg) =>
                {
                    metricCfg.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
                    {
                        ExportIntervalMilliseconds = 1000
                    };
                    metricCfg.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                });
            });
        return services;
    }

    public static IServiceCollection AddMessageDbContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MessageDbContext>(x =>
        {
            var connectionString = config.GetConnectionString("Db")!;
            x.UseSqlServer(connectionString);
        });
        return services;
    }

    public static async Task EnsureDbCreated(this IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MessageDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
