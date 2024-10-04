package Services;

import Models.ServerMessages.ErrorMessage;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.zeromq.SocketType;
import org.zeromq.ZContext;
import org.zeromq.ZMQ;

public class MessageService {
    public <T> T SendMessage(Object message, String connectionString, Class<T> returnClass) {
        ObjectMapper mapper = new ObjectMapper();
        try (ZContext context = new ZContext();) {
            ZMQ.Socket socket = context.createSocket(SocketType.REQ);
            socket.connect(connectionString);

            String json = mapper.writeValueAsString(message);
            socket.send(json.getBytes(ZMQ.CHARSET), 0);

            var reply = socket.recvStr();
            System.out.println(reply);
            if (reply.isEmpty()){
                return null;
            }
            else if (reply.contains("error")) {
                var error = mapper.readValue(reply, ErrorMessage.class);
                System.out.println(error.Error + " : " + error.Message);
                return null;
            }

            return mapper.readValue(reply, returnClass);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }
}
