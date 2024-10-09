using TheQueue.Server.Core.Enums;
using TheQueue.Server.Core.Models.BroadcastMessages;
using TheQueue.Server.Core.Models.ClientMessages;
using TheQueue.Server.Core.Models.ServerMessages;
using TheQueue.Server.Core.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace TheQueue.Server.Core.Services
{
    public class SupervisorService
    {
        private StudentService _studentService;
        private QueueService _queueService;
        private ILogger<SupervisorService> _logger;

        public ConcurrentList<Supervisor> _supervisors;

        public SupervisorService(StudentService studentService, QueueService queueService, ILogger<SupervisorService> logger)
        {
            _studentService = studentService;
            _queueService = queueService;
            _logger = logger;
            _supervisors = new();
        }

        public void CreateSupervisorIfNotExists(ClientMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Name))
                throw new Exception(CreateErrorMessage("Supervisor Name not found", ErrorType.Warning));

            if (!_supervisors.Any(x => x.Name == message.Name))
            {
                Supervisor supervisor = new()
                {
                    Name = message.Name,
                    Status = Status.Pending
                };
                _supervisors.Add(supervisor);
            }
        }

        public void SetSupervisorStatus(string supervisorName, Status status)
        {
            Supervisor supervisor = _supervisors.FirstOrDefault(x => x.Name == supervisorName)
                ?? throw new Exception(CreateErrorMessage($"No Supervisor with name {supervisorName}", ErrorType.Warning));

            if (status is not Status.Occupied)
            {
                supervisor.Client = null;
            }
            supervisor.Status = status;
        }

        public string HandleSupervisorEnterQueue(ClientMessage message)
        {

            _logger.LogInformation("Getting student for Supervisor {supervisor}", message.Name);

            QueueTicket? queueTicket = _studentService._queue.FirstOrDefault();
            if (queueTicket is null)
            {
                _logger.LogInformation("No students available for supervision");
                SetSupervisorStatus(message.Name, Status.Available);
                return "{}";
            }
            SetSupervisorStatus(message.Name, Status.Occupied);
            return JsonConvert.SerializeObject(queueTicket);
        }

        public string HandleMessageRequest(ClientMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Message.Body))
                return CreateErrorMessage("Received message with not body content to forward", ErrorType.Warning);

            Supervisor? supervisor = _supervisors.FirstOrDefault(x => x.Name == message.Name);
            if (supervisor is null)
                return CreateErrorMessage("No supervisor found", ErrorType.Critical);

            QueueTicket? student = _studentService._queue.FirstOrDefault(x => x.Name == message.Message.Recipient);
            if (student is null)
                return CreateErrorMessage("Recipient not found", ErrorType.Critical);

            supervisor.Client = student;
            UserMessages userMessage = new()
            {
                Supervisor = message.Name,
                Message = message.Message.Body
            };

            _studentService._queue.Remove(student);

            string topic = message.Message.Recipient;
            _queueService.SendBroadcast(topic, userMessage);
            return "{}";
        }

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
