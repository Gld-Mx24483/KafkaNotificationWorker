using Notification.Worker;
using NotificationWorker;
using NotificationWorker.Service.Implementation;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<KafkaConsumerService>(); 
        services.AddHostedService<Worker>();           
    });


var host = builder.Build();
host.Run();
