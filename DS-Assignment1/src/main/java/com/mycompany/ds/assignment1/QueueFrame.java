/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.mycompany.ds.assignment1;

/**
 *
 * @author Chris
 */
import java.awt.Color;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import javax.swing.BoxLayout;
import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JPanel;
import javax.swing.JTextField;
import javax.swing.JLabel;
/**
 *
 * @author Chris
 */
public class QueueFrame extends JFrame implements ActionListener {
    
    private EnterQueue enterQueue;
    private JPanel namePanel;
    private JTextField nameTextField;
    private String enteredName;
    
    QueueFrame() {
        
        namePanel = new JPanel();
        nameTextField = new JTextField();
        JLabel nameLabel = new JLabel("Enter your name: ");
        JButton submitName = new JButton("Submit");
        submitName.addActionListener(this);
        
        namePanel.setVisible(true);
        namePanel.setBackground(Color.green);
        namePanel.setBounds(5,10,150,80);
        namePanel.setLayout(new BoxLayout(namePanel, BoxLayout.Y_AXIS));
        namePanel.add(nameLabel);
        namePanel.add(nameTextField);
        namePanel.add(submitName);
        
        QueueList queueList = new QueueList();
        
        this.add(namePanel);
        this.add(queueList);
        
        
        this.setBounds(0,0,450,500);
        this.setLayout(null);
        this.setTitle("The Queue");
        this.setVisible(true);
        this.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        
    }

    @Override
    public void actionPerformed(ActionEvent e) {
        String s = e.getActionCommand();
        if(s.equals("Submit")) {
            
            enteredName = nameTextField.getText();
            
            enterQueue = new EnterQueue(enteredName);
            
        }
        
    }
    
}