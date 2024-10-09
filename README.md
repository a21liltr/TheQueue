## Overview
This project implements a heterogeneous distributed queuing system designed for managing supervision sessions. It consists of three components: a student client, a supervisor client, and a server. Communication between the clients and the server is facilitated through ZeroMQ.
Application developed by Lili Tran (a21liltr).
## Features
- **Student Client** (Java): Allows students to join a queue for supervision.
- **Supervisor Client** (Java): Enables supervisors to provide supervision, send messages and dequeueing the students once attended.
- **Server** (C#): Centralized component that handles all communication between student and supervisor clients.

## Prerequisites
### Programming languages
- C#
- Python
### Dependencies
- **ZeroMQ/NetMQ**
- **JamesNK/NewtonSoft.Json**
- **FasterXML/Jackson**
- **Tinyqueue API**

All communication between clients and the server is handled through JSON objects.

Client and server communicates using a publisher/subscriber pattern where the server sends updates to all connected clients, and a request/reply pattern which allows clients to send requests and receive individual responses from the server. The two patterns use separate network ports, so both the server and client maintain two distinct sockets; one for each communication method.

Following are the types of messages from the Tinyqueue API that the server may receive and process.
####Broadcast messages

Broadcast messages are published by server to all subscribed clients using the ZMQ pub/sub pattern.

####Queue status
Sent to all clients in response to any changes in the queue, for example new clients entering the queue or students receiving supervision. The queue status is an ordered array of Queue tickets, where the first element represent the first student in the queue.

Sent by: server
Topic: queue
```
[ 
    {"ticket": <index>, "name": "<name>"}, ... 
]
```

####Supervisor status
Sent to all clients in response to any changes in the list of supervisors, for example new supervisors connecting or when the status of a supervisor changes.

Sent by: server
Topic: supervisors
```
[ 
    {"name": <name>, "status": "pending"|"available"|"occupied", "client": undefined|{"ticket":<index>,"name":"<name>"}}, ... 
]
```

Note, an undefined key in JSON implies a key that is absent.

####User messages
The server will also publish messages directly to individual users (students). These messages are received by all clients representing that user. These messages may for example indicate that it's the user's turn to receive supervision.

Sent by: server
Topic: <name of user>
```
{
    "supervisor":"<name of supervisor>",
    "message":"<message from supervisor>"
}
```

#### Request/reply messages
Requests are sent by clients with individual responses from the server.

Each request must specify a clientId. The clientId is a universally unique identifier (UUID) chosen by each client. Requests may also comprise a name. The name may be shared between several clients and identifies the queuing user. Two or more clients connected with the same name represent the same user, and thus holds a single shared place in the queue.

#### Enter queue
Indicates that a user with specified name want to enter the queue.

A single user may connect through several clients. If another client with the same name is already connected, both clients hold the same place in the queue.
Sent by: client.
Expected response from server: Queue ticket.
```
{
    "clientId": "<unique id string>",
    "name": "<name>",
    "enterQueue": true
}
```

#### Queue ticket
Indicates that the client with specified name and ticket has entered the queue.
Sent by: server.
```
{
    "name": "<name>",
    "ticket": <index>
}
```

#### Message request
The server accepts message requests from supervisor clients. A message request results in a User message being sent to listening clients. For test purposes, student clients can send messages to themselves, simulating the presence of a supervisor.
Sent by: client.
```
{
    "clientId": "<unique id string>",
    "name": "<name>",
    "message": {"recipient": "<name>", "body": "<message content>"}
}
```

#### Heartbeat
All clients are expected to send a regular messages (heartbeats) to indicate that they want to maintain their plaice in the queue. Clients with a heartbeat interval larger than 4 seconds will be considered inactive, and will be removed from queue.
Sent by: client.
Expected response from server: ```{}```.
```
{
    "clientId": "<unique id string>"
}
```

#### Error message
Sent in response to any client message that does not follow the specified API. The server may also use this message type to indicate other types of errors, for example invalid name strings.
Sent by: server.
```
{
    "error": "<error type>",
    "msg": "<error description>"
}
```

#### Install Server dependencies using NuGet package manager
- NetMQ (.NET port of ZeroMQ) used to communicate with connected clients
- Newtonsoft.Json used for serializing/deserializing objects and handling messages

### Installation
```bash

# Open a terminal (Command Prompt or PowerShell for Windows, Terminal for macOS or Linux)

# Ensure Git is installed
# Visit https://git-scm.com to download and install console Git if not already installed

# Clone the repository
git clone https://github.com/a21liltr/TheQueue.git

# Navigate to the project directory
cd TheQueue

# Check if .NET SDK is installed
dotnet --version  # Check the installed version of .NET SDK
# Visit the official Microsoft website to install or update it if necessary

# Restore dependencies
dotnet restore

# Compile the project
dotnet build

# Run the project
run TheQueue.Server.exe

```
