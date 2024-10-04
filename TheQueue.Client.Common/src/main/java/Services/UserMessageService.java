package Services;

import Models.BroadcastMessages.UserMessage;
import UI.Popup;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;

import javax.swing.*;

public class UserMessageService extends SwingWorker<Void, Void> {
    private final String _connectionString;
    private final String _topic;
    private final JFrame _frame;

    public UserMessageService(String connectionString, String topic, JFrame frame) {
        _connectionString = connectionString;
        _frame = frame;
        _topic = topic;
    }

    @Override
    protected Void doInBackground() throws Exception {

        try (ZContext context = new ZContext()) {
            ObjectMapper mapper = new ObjectMapper();
            ZMQ.Socket sub = context.createSocket(SocketType.SUB);
            sub.connect(_connectionString);
            sub.subscribe(_topic.getBytes());
            while(!isCancelled()){
                try {
                    var topic = sub.recvStr();
                    var messageBody = sub.recvStr();

                    System.out.println(messageBody);

                    var message = mapper.readValue(messageBody, UserMessage.class);

                    SwingUtilities.invokeLater(new Runnable() {
                        public void run() {
                            Popup.ShowMessage(_frame, message.Supervisor + ": " + message.Message, "Supervisor Message");
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
