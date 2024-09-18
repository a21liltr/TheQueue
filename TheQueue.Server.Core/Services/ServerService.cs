using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System.Text.Json;

namespace TheQueue.Server.Core.Services
{
    public class ServerService
    {
        private readonly ILogger<ServerService> _logger;
        private readonly IConfiguration _config;
        private List<string> _clientQueue;

        public ServerService(ILogger<ServerService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _clientQueue = new List<string>();
            var test = _config.GetValue<string>("test");
        }

        public void RunServer()
        {
            using (var responder = new ResponseSocket())
            {
                responder.Bind("tcp://*:5556");

                while (true)
                {
                    string message = responder.ReceiveFrameString();
                    _logger.LogInformation("Received message, {message}", message);

                    if (message is null)
                    {
                        _logger.LogInformation($"Received empty message");
                        continue;
                    }

                    // TODO: Handle properly.
                    

                    Console.WriteLine($"Received message");

                    responder.SendFrame($"Server has received message:\n{message}");
                }
            }
        }
    }
}