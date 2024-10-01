## Overview
This project implements a heterogeneous distributed queuing system designed for managing supervision sessions. It consists of three components: a student client, a supervisor client, and a server. Communication between the clients and the server is facilitated through ZeroMQ.
Application developed by Lili Tran (a21liltr) and Christian Mourad (b22chrmo).

## Features
- **Student Client** (Python): Allows students to join a queue for supervision.
- **Supervisor Client** (Python): Enables supervisors to provide supervision, send messages and dequeueing the students once attended.
- **Server** (C#): Centralized component that handles all communication between student and supervisor clients.

## Prerequisites
### Programming languages
- C#
- Python
### Dependencies
#### Install Server dependencies using NuGet package manager
- NetMQ (.NET port of ZeroMQ) used to communicate with connected clients
- Newtonsoft.Json used for serializing/deserializing objects and handling messages
- Serilog for logging purposes

#### Install Client(s) dependencies using pip
- tkinter

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

```
