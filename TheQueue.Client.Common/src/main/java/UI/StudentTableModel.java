package UI;

import Models.BroadcastMessages.QueueTicket;

import javax.swing.table.AbstractTableModel;
import java.util.ArrayList;
import java.util.List;

public class StudentTableModel extends AbstractTableModel {
    private final String[] columnNames = new String[] {"Name","Ticket"};
    private List<QueueTicket> ticketList = new ArrayList<>();

    @Override
    public int getRowCount() {
        return ticketList.size();
    }

    @Override
    public int getColumnCount() {
        return columnNames.length;
    }

    @Override
    public Object getValueAt(int rowIndex, int columnIndex) {
        QueueTicket ticket = ticketList.get(rowIndex);
        return switch (columnIndex) {
            case 0 -> ticket.getName();
            case 1 -> ticket.getTicket();
            default -> null;
        };
    }

    @Override
    public String getColumnName(int column) {
        return columnNames[column];
    }

    public void addData(List<QueueTicket> tickets){
        ticketList = tickets;
    }
}
