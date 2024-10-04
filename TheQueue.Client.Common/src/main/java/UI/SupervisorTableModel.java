package UI;

import Models.BroadcastMessages.SupervisorStatus;

import javax.swing.table.AbstractTableModel;
import java.util.ArrayList;
import java.util.List;

public class SupervisorTableModel extends AbstractTableModel {
    private final String[] columnNames = new String[] {"Name","Status","Student","Ticket"};
    private List<SupervisorStatus> supervisorList = new ArrayList<>();

    @Override
    public int getRowCount() {
        return supervisorList.size();
    }

    @Override
    public int getColumnCount() {
        return columnNames.length;
    }

    @Override
    public Object getValueAt(int rowIndex, int columnIndex) {
        SupervisorStatus supervisor = supervisorList.get(rowIndex);
        return switch (columnIndex) {
            case 0 -> supervisor.Name;
            case 1 -> supervisor.Status;
            case 2 -> supervisor.Client == null ? "" : supervisor.Client.getName();
            case 3 -> supervisor.Client == null ? "" : supervisor.Client.getTicket();
            default -> null;
        };
    }

    @Override
    public String getColumnName(int column) {
        return columnNames[column];
    }

    public void addData(List<SupervisorStatus> supervisors){
        supervisorList = supervisors;
    }
}