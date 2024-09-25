/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.mycompany.ds.assignment1;

import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;
import java.util.Random;

/**
 *
 * @author Chris
 */
public class EnterQueue {
    
    EnterQueue(String username) {
        
        //JsonObject jo = JsonParser.parseString(json).getAsJsonObject();
        
        /*Scanner scan = new Scanner(System.in);
        System.out.println("Enter your name: ");
        
        String username = scan.nextLine();*/
        
        //String json = "{\"clientId\": \"123qwe\", \"name\": \"Chrille\", \"enterQueue\": true}";
        
        //String message = "{\"clientId\": \"123qwe\", \"enterQueue\": true, \"name\": " + "\"" + username + "\"}";
        
        String message = "{\"clientId\": \"" + username + generateID() +"\", \"enterQueue\": true, \"name\": " + "\"" + username + "\"}";
        
        try (ZContext context = new ZContext()) {
            ZMQ.Socket socket = context.createSocket(SocketType.REQ);
            ZMQ.Socket socket2 = context.createSocket(SocketType.REP);
            socket2.connect("tcp://ds.iit.his.se:5556");
            socket.connect("tcp://ds.iit.his.se:5556");
            
            socket.send(message.getBytes(ZMQ.CHARSET),0);
            
            String msg2 = new String(socket2.recv(),ZMQ.CHARSET);
            
            System.out.println(msg2);
            
        }
    }
    
    private int generateID() {
    
        Random r = new Random();
        int random = r.nextInt(999);
        
        return random;
    }
    
}
