package Services;

import Models.BroadcastMessages.SupervisorStatus;
import UI.SupervisorTableModel;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;

import javax.swing.*;
import java.util.List;

public class SupervisorListService extends SwingWorker<Void, List<SupervisorStatus>> {
    private final String _connectionString;
    private List<SupervisorStatus> supervisors;
    private final JTable _table;

    public SupervisorListService(String connectionString, JTable table){
        _connectionString = connectionString;
        _table = table;
    }

    @Override
    protected Void doInBackground() throws Exception {

        try (ZContext context = new ZContext()) {
            ObjectMapper mapper = new ObjectMapper();
            ZMQ.Socket sub = context.createSocket(SocketType.SUB);
            sub.connect(_connectionString);
            sub.subscribe("supervisors".getBytes());
            while(!isCancelled()){
                try {
                    var topic = sub.recvStr();
                    var messageBody = sub.recvStr();

                    System.out.println(messageBody);
                    supervisors = mapper.readValue(messageBody, new TypeReference<List<SupervisorStatus>>(){});

                    SwingUtilities.invokeLater(new Runnable() {
                        public void run() {
                            SupervisorTableModel tableModel = new SupervisorTableModel();
                            tableModel.addData(supervisors);
                            _table.setModel(tableModel);
                        }
                    });
                }
                catch (Exception ex) {
                    System.out.println(ex);
                }
            }
        }
        return null;
    }
}