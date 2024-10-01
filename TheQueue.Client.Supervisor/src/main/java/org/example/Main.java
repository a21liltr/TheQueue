package org.example;

import Models.ClientMessages.EnterQueue;
import Models.ServerMessages.QueueTicket;
import Services.QueueService;
import org.zeromq.SocketType;
import org.zeromq.ZMQ;
import org.zeromq.ZContext;

public class Main {
    public static void main(String[] args) throws Exception {
        System.out.println("Connecting to server");

        QueueService service = new QueueService();
        EnterQueue request = new EnterQueue() {
            {
                ClientId = "aaa";
                Name = "eeeee";
                EnterQueue = true;
            }
        };
        QueueTicket ticket = service.SendQueueRequest(request);
        System.out.println(ticket);
    }
}