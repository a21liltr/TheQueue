package Models.BroadcastMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class UserMessage {
    @JsonProperty("supervisor")
    public String Supervisor;
    @JsonProperty("message")
    public String Message;
}
