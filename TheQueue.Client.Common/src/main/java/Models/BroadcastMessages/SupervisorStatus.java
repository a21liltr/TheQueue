package Models.BroadcastMessages;

import com.fasterxml.jackson.annotation.JsonProperty;
import Enums.Status;

public class SupervisorStatus {
    @JsonProperty("name")
    public String Name;
    @JsonProperty("status")
    public Status Status ;
    @JsonProperty("client")
    public QueueTicket Client;
}
