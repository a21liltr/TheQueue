## Overview
This project implements a heterogeneous distributed queuing system designed for managing supervision sessions. It consists of three components: a student client, a supervisor client, and a server. Communication between the clients and the server is facilitated through ZeroMQ.

## Features
- **Student Client**: Allows students to join a queue for supervision.
- **Supervisor Client**: Enables supervisors (teachers) to manage the queue by providing supervision and removing students once attended.
- **Server**: Centralized component that handles all communication between student and supervisor clients.

This system streamlines the process of organizing and conducting supervision sessions by providing a clear and efficient queuing mechanism.
