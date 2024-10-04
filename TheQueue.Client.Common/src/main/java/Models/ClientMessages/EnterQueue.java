package Models.ClientMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class EnterQueue extends RequestBase {
    @JsonProperty("name")
    public String Name;
    @JsonProperty("enterQueue")
    public boolean EnterQueue;
}
