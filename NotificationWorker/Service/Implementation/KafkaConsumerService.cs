using Confluent.Kafka;
using Newtonsoft.Json;
using NotificationWorker.Data.Dtos.Requests;

namespace NotificationWorker.Service.Implementation
{
    public class KafkaConsumerService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly string _bootstrapServers = Environment.GetEnvironmentVariable("KafkaBootstrapServers")!;
        private readonly string _saslUsername     = Environment.GetEnvironmentVariable("SaslUsername")!;
        private readonly string _saslPassword     = Environment.GetEnvironmentVariable("SaslPassword")!;
        private readonly string _topic            = Environment.GetEnvironmentVariable("KafkaTopicName")!;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
        {
            _logger = logger;
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism    = SaslMechanism.Plain,
                SaslUsername     = _saslUsername,
                SaslPassword     = _saslPassword,
                GroupId          = "notification-worker-group",
                AutoOffsetReset  = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            try
            {
                consumer.Subscribe(_topic);
                _logger.LogInformation("Kafka consumer started. Listening to topic: {Topic}", _topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(cancellationToken);
                        var evt    = JsonConvert.DeserializeObject<NotificationEvent>(result.Message.Value);

                        if (evt is null)
                        {
                            consumer.Commit(result);
                            continue;
                        }

                        var emailMessage = EmailTemplateBuilder.Build(evt);

                        _logger.LogInformation("────────────────────────────────────────────");
                        _logger.LogInformation("[KAFKA] Module     : {Module}",        evt.Module);
                        _logger.LogInformation("[KAFKA] EventType  : {EventType}",     evt.EventType);
                        _logger.LogInformation("[EMAIL] To         : {To}",            emailMessage.To);
                        _logger.LogInformation("[EMAIL] Subject    : {Subject}",       emailMessage.Subject);
                        _logger.LogInformation("[EMAIL] Body       : {Body}",          emailMessage.Body);
                        _logger.LogInformation("────────────────────────────────────────────");

                        consumer.Commit(result); 
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Kafka consumer stopping due to cancellation.");
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogWarning("Kafka consumer handle was destroyed. Shutting down.");
                        break;
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka. Retrying in 5s...");
                        await Task.Delay(5000, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error during message processing. Retrying in 2s...");
                        await Task.Delay(2000, cancellationToken);
                    }
                }
            }
            finally
            {
                consumer.Close();
                consumer.Dispose();
                _logger.LogInformation("Kafka consumer closed and disposed.");
            }
        }
    }
}