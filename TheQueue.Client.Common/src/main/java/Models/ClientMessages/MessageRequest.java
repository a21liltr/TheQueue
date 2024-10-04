package Models.ClientMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class MessageRequest extends RequestBase {
    @JsonProperty("name")
    public String Name;
    @JsonProperty("message")
    public Recipient Message;
}


