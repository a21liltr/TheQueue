using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Models.ServerMessages
{
    public class ErrorMessage
    {
        public ErrorType Error { get; set; }
        public string Msg { get; set; }
    }
}
