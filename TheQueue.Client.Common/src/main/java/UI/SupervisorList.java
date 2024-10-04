package UI;

import Services.SupervisorListService;

import javax.swing.*;

public class SupervisorList extends JPanel {

    public SupervisorList(String connectionString) {
        JTable _table = new JTable(new SupervisorTableModel());
        JScrollPane scrollPane = new JScrollPane(_table);

        SupervisorListService supervisorListService = new SupervisorListService(connectionString, _table);

        this.add(scrollPane);
        supervisorListService.execute();
    }
}
