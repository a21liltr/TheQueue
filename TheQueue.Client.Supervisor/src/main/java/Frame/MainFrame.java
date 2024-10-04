package Frame;

import UI.ConnectPanel;
import UI.StudentList;
import UI.SupervisorList;

import javax.swing.*;
import java.awt.*;

public class MainFrame {

    public MainFrame() {
        JFrame mainFrame = new JFrame("Supervisor");
        mainFrame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        mainFrame.setSize(1000, 550);

        BorderLayout bl = new BorderLayout();
        mainFrame.setLayout(bl);

        JPanel studentList = new StudentList("tcp://localhost:5555");
        JPanel supervisorList = new SupervisorList("tcp://localhost:5555");
        JPanel connectPanel = new ConnectPanel("tcp://localhost:5556", "tcp://localhost:5555",false);

        JPanel center = new JPanel();
        center.add(studentList);
        center.add(supervisorList);

        mainFrame.getContentPane().add(center, BorderLayout.CENTER);
        mainFrame.getContentPane().add(connectPanel, BorderLayout.SOUTH);

        mainFrame.setVisible(true);
    }
}
