package Services;

import Models.ClientMessages.EnterQueue;
import Models.ServerMessages.QueueTicket;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;

import java.io.IOException;

public class QueueService {
    public QueueTicket SendQueueRequest(EnterQueue request) {
        ObjectMapper mapper = new ObjectMapper();
        try (ZContext context = new ZContext();) {
            ZMQ.Socket socket = context.createSocket(SocketType.REQ);
            socket.connect("tcp://localhost:5555");

            String json = mapper.writeValueAsString(request);

            socket.send(json.getBytes(ZMQ.CHARSET), 0);

            byte[] reply = socket.recv(0);

            return mapper.readValue(new String(reply), QueueTicket.class);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }
}
