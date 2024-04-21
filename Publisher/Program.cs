using Components;
using MassTransit;
using Publisher;
using Serilog.Events;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
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
        outboxCfg.UseBusOutbox(busOutboxCfg =>
        {
            busOutboxCfg.DisableDeliveryService();
        });
    });

    cfg.UsingInMemory();
});

builder.Services.AddObservability("Publisher");

builder.Services.AddMessageDbContext(builder.Configuration);

// Add lots of publisher workers because why not
foreach (int i in Enumerable.Range(0, Environment.ProcessorCount))
    builder.Services.AddSingleton<IHostedService, Worker>();

builder.Logging.ClearProviders().AddSerilog(Log.Logger);

var host = builder.Build();

await host.EnsureDbCreated();

host.Run();
