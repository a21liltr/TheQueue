package UI;

import javax.swing.*;
import java.awt.*;

public class MainFrame {

    public MainFrame(String[] args, boolean isStudent) {
        String server = "localhost";
        int subPort = 5555;
        int reqPort = subPort+1;
        if (args.length == 3) {
            server = args[0];
            subPort = Integer.parseInt(args[1]);
            reqPort = Integer.parseInt(args[2]);
        }
        else {
            System.out.println("Wrong arguments given, using default values");
        }

        JFrame mainFrame = new JFrame(isStudent ? "Student" : "Supervisor" );
        mainFrame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        mainFrame.setSize(1000, 550);

        BorderLayout bl = new BorderLayout();
        mainFrame.setLayout(bl);

        JPanel studentList = new StudentList("tcp://" + server + ":" + subPort);
        JPanel supervisorList = new SupervisorList("tcp://" + server + ":" + subPort);
        JPanel connectPanel = new ConnectPanel("tcp://" + server + ":" + reqPort,
                "tcp://" + server + ":" + subPort,isStudent);

        JPanel center = new JPanel();
        center.add(studentList);
        center.add(supervisorList);

        mainFrame.getContentPane().add(center, BorderLayout.CENTER);
        mainFrame.getContentPane().add(connectPanel, BorderLayout.SOUTH);

        mainFrame.setVisible(true);
    }
}
