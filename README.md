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
use git clone https://github.com/a21liltr/TheQueue.git
```
