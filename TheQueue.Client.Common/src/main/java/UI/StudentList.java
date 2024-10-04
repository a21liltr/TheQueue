package UI;

import Services.StudentListService;

import javax.swing.*;

public class StudentList extends JPanel {

    public StudentList(String connectionString) {
        JTable _table = new JTable(new StudentTableModel());
        JScrollPane scrollPane = new JScrollPane(_table);

        StudentListService studentListService = new StudentListService(connectionString, _table);

        this.add(scrollPane);
        studentListService.execute();
    }
}
