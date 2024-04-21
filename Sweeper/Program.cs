using Components;
using MassTransit;
using Serilog.Events;
using Serilog;
using MassTransit.EntityFrameworkCoreIntegration;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddEntityFrameworkOutbox<MessageDbContext>(outboxCfg =>
    {
        outboxCfg.DisableInboxCleanupService();
        outboxCfg.UseSqlServer();
        outboxCfg.QueryDelay = TimeSpan.FromMilliseconds(1000);
        outboxCfg.QueryMessageLimit = 100;
        outboxCfg.UseBusOutbox();
    });

    cfg.UsingInMemory();
});

builder.Services.AddObservability("Sweeper");

//builder.Services.RemoveHostedService<BusOutboxDeliveryService<MessageDbContext>>();

//builder.Services.AddHostedService<CustomBusOutboxDeliveryService<MessageDbContext>>();

builder.Services.AddMessageDbContext(builder.Configuration);

builder.Logging.ClearProviders().AddSerilog(Log.Logger);

var host = builder.Build();

await host.EnsureDbCreated();

host.Run();
