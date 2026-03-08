using NotificationWorker.Service.Implementation;

namespace Notification.Worker;

public class Worker(ILogger<Worker> logger, KafkaConsumerService kafkaConsumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

        await kafkaConsumer.ConsumeAsync(stoppingToken);
    }
}