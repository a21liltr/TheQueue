namespace TheQueue.Server.Core.Enums
{
    public enum MessageType
    {
        // Used by Student and Supervisor
        Connect = 0,
        Disconnect = 1,
        Heartbeat = 2,

        // Used only by Student
        Queue = 3,
        Dequeue = 4,

        // Used only by Supervisor
        DequeueStudent = 5,
        NextUp = 6
    }
}
