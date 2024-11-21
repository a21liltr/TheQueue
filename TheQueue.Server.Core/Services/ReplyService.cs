using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using TheQueue.Server.Core.Enums;
using TheQueue.Server.Core.Models.ClientMessages;
using TheQueue.Server.Core.Models.ServerMessages;
using TheQueue.Server.Core.Options;

namespace TheQueue.Server.Core.Services
{
    public class ReplyService : BackgroundService
    {
        private ClientService _clientService;
        private StudentService _studentService;
        private SupervisorService _supervisorService;
        private QueueService _queueService;
        private readonly ILogger<ReplyService> _logger;
        private readonly IOptions<ConnectionOptions> _options;

        public ReplyService(ClientService clientService, StudentService studentService, SupervisorService supervisorService, QueueService queueService, ILogger<ReplyService> logger, IOptions<ConnectionOptions> options)
        {
            _clientService = clientService;
            _studentService = studentService;
            _supervisorService = supervisorService;
            _queueService = queueService;
            _logger = logger;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                using (var runtime = new NetMQRuntime())
                {
                    runtime.Run(RunReply(stoppingToken));
                }
            }, stoppingToken);
        }

        private async Task RunReply(CancellationToken stoppingToken)
        {
            using (ResponseSocket responder = new($"tcp://localhost:{_options.Value.RepPort}"))
            {
                _logger.LogInformation("ReplyService running on port: {port}", _options.Value.RepPort);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        (string message, bool anotherFrame) = await responder.ReceiveFrameStringAsync(stoppingToken);
                        _logger.LogInformation("Received message, {message}", message);
                        if (string.IsNullOrEmpty(message))
                        {
                            responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Warning));
                            continue;
                        }

                        var received = JsonConvert.DeserializeObject<ClientMessage>(message);
                        if (received is null)
                        {
                            responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Warning));
                            continue;
                        }
                        _logger.LogDebug("Deserialized message to ClientMessage object");

                        // validation on properties
                        if (string.IsNullOrWhiteSpace(received.ClientId))
                        {
                            responder.SendFrame(CreateErrorMessage("Missing information", ErrorType.Critical));
                        }

                        _clientService.HandleConnect(received);

                        if (received.Supervisor.HasValue && received.Supervisor.Value)
                        {
                            try
                            {
                                _supervisorService.CreateSupervisorIfNotExists(received);
                                if (received.EnterQueue.HasValue && received.EnterQueue.Value)
                                {
                                    responder.SendFrame(_supervisorService.HandleSupervisorEnterQueue(received)); // returns QueueTicket or null
                                }
                                else
                                {
                                    _supervisorService.SetSupervisorStatus(received.Name, Status.pending);
                                    responder.SendFrame("{}");
                                }
                            }
                            catch (Exception ex)
                            {
                                responder.SendFrame(ex.Message); // returns error
                                continue;
                            }
                        }
                        else if (received.EnterQueue.HasValue)
                        {
                            var response = _studentService.CreateStudentAndAddToQueueIfNotExists(received);
                            var responseMessage = JsonConvert.SerializeObject(response);
                            responder.SendFrame(responseMessage);
                        }
                        else if (received.Message is not null)
                        {
                            if (string.IsNullOrWhiteSpace(received.Name))
                            {
                                responder.SendFrame(CreateErrorMessage("Received bad message", ErrorType.Critical));
                                continue;
                            }
                            responder.SendFrame(_supervisorService.HandleMessageRequest(received));
                        }
                        else
                        {
                            if (!_clientService.HandleHeartbeat(received))
                            {
                                responder.SendFrame(
                                CreateErrorMessage("Heartbeat could not be tied to a connected client", ErrorType.Critical));
                            }
                            else
                            {
                                responder.SendFrame("{}");
                            }
                            continue;
                        }
                        _queueService.SendBroadcast("queue", _studentService._queue);
                        _queueService.SendBroadcast("supervisors", _supervisorService._supervisors);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured: {errorMessage}", ex.Message);
                    }
                }
            }
            _logger.LogInformation("ReplyService stopped");
        }

        // TODO: Create one shared CreateErrorMessage function
        private string CreateErrorMessage(string message, ErrorType errorType)
        {
            ErrorMessage errorMessage = new()
            {
                Error = errorType,
                Msg = message
            };
            _logger.LogError("{errorType} An error occured: {errorMessage}", errorType, message);
            return JsonConvert.SerializeObject(errorMessage);
        }
    }
}
