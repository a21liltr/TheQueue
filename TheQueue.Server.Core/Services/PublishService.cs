using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using TheQueue.Server.Core.Models.BroadcastMessages;
using TheQueue.Server.Core.Options;

namespace TheQueue.Server.Core.Services
{
    public class PublishService : BackgroundService
    {
        private readonly QueueService _queueService;
        private readonly ILogger<PublishService> _logger;
        private readonly IOptions<ConnectionOptions> _options;

        public PublishService(QueueService queueService,ILogger<PublishService> logger, IOptions<ConnectionOptions> options)
        {
            _queueService = queueService;
            _logger = logger;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => RunPublish(stoppingToken), stoppingToken);
        }

        private void RunPublish(CancellationToken stoppingToken)
        {
            using (NetMQPoller poller = new() { _queueService.broadcastQueue })
            using (PublisherSocket publisher = new($"tcp://localhost:{_options.Value.Port}"))
            {
                _logger.LogInformation("PublishService running on port: {port}", _options.Value.Port);
                _queueService.broadcastQueue.ReceiveReady += (sender, args) =>
                {
                    while (args.Queue.TryDequeue(out TopicMessage? message, new(1000)))
                    {
                        try
                        {
                            if (message != null)
                            {
                                publisher.SendMoreFrame(message.Topic);
                                publisher.SendFrame(message.Message);
                                _logger.LogInformation("Published {topic} : {message}", message.Topic, message.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occured: {errorMessage}", ex.Message);
                        }
                    }
                };
                poller.RunAsync();
                while (!stoppingToken.IsCancellationRequested && poller.IsRunning) { }
                poller.StopAsync();
            }
            _logger.LogInformation("PublishService stopped");
        }
    }
}
