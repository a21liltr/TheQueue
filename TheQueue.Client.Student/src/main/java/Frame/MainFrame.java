package Frame;

import javax.swing.*;
import java.awt.*;

import UI.ConnectPanel;
import UI.StudentList;
import UI.SupervisorList;

public class MainFrame {

    public MainFrame() {
        JFrame mainFrame = new JFrame("Student");
        mainFrame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        mainFrame.setSize(1000, 550);

        BorderLayout bl = new BorderLayout();
        mainFrame.setLayout(bl);

//        JPanel studentList = new StudentList("tcp://ds.iit.his.se:5555");
//        JPanel supervisorList = new SupervisorList("tcp://ds.iit.his.se:5555");
//        JPanel connectPanel = new ConnectPanel("tcp://ds.iit.his.se:5556", "tcp://localhost:5555",true);

        JPanel studentList = new StudentList("tcp://localhost:5555");
        JPanel supervisorList = new SupervisorList("tcp://localhost:5555");
        JPanel connectPanel = new ConnectPanel("tcp://localhost:5556", "tcp://localhost:5555",true);

        JPanel center = new JPanel();
        center.add(studentList);
        center.add(supervisorList);

        mainFrame.getContentPane().add(center, BorderLayout.CENTER);
        mainFrame.getContentPane().add(connectPanel, BorderLayout.SOUTH);

        mainFrame.setVisible(true);
    }
}
