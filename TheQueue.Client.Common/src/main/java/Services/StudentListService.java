package Services;

import Models.BroadcastMessages.QueueTicket;
import UI.StudentTableModel;
import com.fasterxml.jackson.annotation.JsonAutoDetect;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.introspect.VisibilityChecker;
import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;

import javax.swing.*;
import java.util.List;

public class StudentListService extends SwingWorker<Void, List<QueueTicket>> {
    private final String _connectionString;
    private List<QueueTicket> queueTickets;
    private final JTable _table;

    public StudentListService(String connectionString, JTable table){
        _connectionString = connectionString;
        _table = table;
    }

    @Override
    protected Void doInBackground() throws Exception {

        try (ZContext context = new ZContext()) {
            ObjectMapper mapper = new ObjectMapper();
            mapper.setVisibility(VisibilityChecker.Std.defaultInstance().withFieldVisibility(JsonAutoDetect.Visibility.ANY));
            ZMQ.Socket sub = context.createSocket(SocketType.SUB);
            sub.connect(_connectionString);
            sub.subscribe("queue".getBytes());
            while(!isCancelled()){
                try {
                    var topic = sub.recvStr();
                    // var messageType = sub.recvStr();//sub.recvStr(1);
                    var messageBody = sub.recvStr();//sub.recvStr(2);

                     // System.out.println(messageType);
                    System.out.println(messageBody);
                    // MessageType receivedType = mapper.readValue("\"" + messageType + "\"", MessageType.class);
                    queueTickets = mapper.readValue(messageBody, new TypeReference<List<QueueTicket>>(){});

                    SwingUtilities.invokeLater(new Runnable() {
                        public void run() {
                            StudentTableModel tableModel = new StudentTableModel();
                            tableModel.addData(queueTickets);
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
