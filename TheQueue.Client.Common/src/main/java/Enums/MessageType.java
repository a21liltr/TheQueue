package Enums;

public enum MessageType {
    // Used by Student and Supervisor
    Connect,
    Disconnect,
    Heartbeat,

    // Used only by Student
    Queue,
    Dequeue,

    // Used only by Supervisor
    DequeueStudent,
    NextUp
}
