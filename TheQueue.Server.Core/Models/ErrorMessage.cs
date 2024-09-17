using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Models
{
    public class ErrorMessage
    {
        public ErrorType Error { get; set; }
        public string Message { get; set; }

        public ErrorMessage(ErrorType errorType, string errorMessage)
        {
            Error = errorType;
            Message = errorMessage;
        }
    }
}
