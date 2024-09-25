/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.mycompany.ds.assignment1;

import java.awt.Color;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.BoxLayout;
import javax.swing.JPanel;
import javax.swing.JButton;
import javax.swing.JLabel;
import javax.swing.JTextField;
import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;

/**
 *
 * @author Chris
 */
public class QueueList extends JPanel implements ActionListener{
    
    private JTextField queueList;
    private String msg;
    
    QueueList() {
        
        try (ZContext context = new ZContext()) {

            ZMQ.Socket socket = context.createSocket(SocketType.SUB);
            socket.connect("tcp://ds.iit.his.se:5555");
            socket.subscribe("queue");
            
            String topic = new String(socket.recv(),ZMQ.CHARSET);
            msg = new String(socket.recv(),ZMQ.CHARSET);

        }
        
        JButton queueRefresher = new JButton("Refresh");
        queueRefresher.addActionListener(this);
        JLabel queueLabel = new JLabel("Current queue:");
        queueList = new JTextField();
        
        queueList.setText(msg);
        queueList.setEditable(false);
        
        this.setBounds(50,100,200,300);
        this.setLayout(new BoxLayout(this,BoxLayout.Y_AXIS));
        this.setBackground(Color.red);
        this.setVisible(true);
        this.add(queueLabel);
        this.add(queueRefresher);
        this.add(queueList);
        
        
    }
    
    void refreshQueue() {
    
        try (ZContext context = new ZContext()) {

            ZMQ.Socket socket = context.createSocket(SocketType.SUB);
            socket.connect("tcp://ds.iit.his.se:5555");
            socket.subscribe("queue");
            
            String topic = new String(socket.recv(),ZMQ.CHARSET);
            msg = new String(socket.recv(),ZMQ.CHARSET);

        }
        
        System.out.println(msg);
        queueList.setText(msg);
    }

    @Override
    public void actionPerformed(ActionEvent e) {
        String s = e.getActionCommand();
        if (s.equals("Refresh")) {
            
            refreshQueue();
            this.revalidate();
            
        }
    }
    
}
