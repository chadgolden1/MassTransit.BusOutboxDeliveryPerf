using Components;
using MassTransit;

namespace Publisher;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var context = scope.ServiceProvider.GetRequiredService<MessageDbContext>();

                // publish single message for 1-to-1 outbox state to message
                await endpoint.Publish(new MyMessageType(), stoppingToken);

                await context.SaveChangesAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failure during worker run");
        }
    }
}
