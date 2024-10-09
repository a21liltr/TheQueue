# Overview
The application integrates the **Tinyqueue API** and uses a *publisher/subscriber* pattern as well as a *request/reply* pattern for all communication between clients and the server. These two patterns use separate network ports, so both the server and clients maintain two distinct sockets; one for each communication method. The communication uses *JSON objects* for the data exchange. The Tinyqueue API is extended to enable functionality of a Supervisor client.

Developed by Lili Tran (a21liltr).
# Features
- **Student Client** (Java): Allows students to join a queue for supervision.
- **Supervisor Client** (Java): Enables supervisors to provide supervision, send messages and dequeueing the students once attended.
- **Server** (C#): Handles all communication between student and supervisor clients.

# Prerequisites
## Programming languages
- C#
- Java

## Dependencies
- **JDK 23**
- **.NET SDK 6.0**
- **FasterXML/Jackson**
- **JamesNK/NewtonSoft.Json**
- **Tinyqueue API**
- **ZeroMQ/JeroMQ**
- **ZeroMQ/NetMQ**

## Tinyqueue API
The server complies with the **Tinyqueue API** and therefore may receive and process the message types defined here: https://ds.iit.his.se/#api

## Extension of the Tinyqueue API
To connect to the server as a Supervisor, an extension of the Tinyqueue API has been implemented to handle supervisor functionality, such as attending the student first in queue, removing him or her from the queue as well as sending notification messages to the student that is to receive supervision.

### Supervisor connect:
Indicates that a Supervisor with a specified name wants to connect to the server.

Sent by: Supervisor client

Expected response: ```{}```
```
{
    "clientId": <unique id string>,
    "name": "<name>",
    "enterQueue": false,
    "supervisor": true
}
```

### Supervisor Enter Queue:
Indicates that a Supervisor with a specified name is ready to give supervision. If any students are in the queue to receive supervision then the server will respond with a *Queue ticket*. If there are no students to give supervision to, then the server will respond with ```{}```.

Sent by: Supervisor client

Expected response: *Queue ticket* or ```{}```
```
{
    "clientId": <unique id string>,
    "name": "<name>",
    "enterQueue": true,
    "supervisor": true
}
```
## Compilation and installation
```bash
# Open a terminal (Command Prompt or PowerShell for Windows, Terminal for macOS or Linux)

# Ensure Git is installed
# Visit https://git-scm.com to download and install console Git if not already installed

# Clone the repository
git clone https://github.com/a21liltr/TheQueue.git

# Navigate to the project directory
cd TheQueue

# For the server:
# Check if .NET SDK is installed
dotnet --version  # Check the installed version of .NET SDK
# Visit the official Microsoft website to install or update it if necessary

# Restore dependencies
dotnet restore

# Compile the project
dotnet build

# Run the project
run TheQueue.Server.exe

# For the clients:
# Check if JDK 23 is installed
javac --version  # Check the installed version of JDK

# Perform clean when doing an install 
mvn clean install -U

# Compile the project
javac TheQueue.Client.Common
javac TheQueue.Client.Student
javac TheQueue.Client.Supervisor

# Run the server
# Run the clients
```
