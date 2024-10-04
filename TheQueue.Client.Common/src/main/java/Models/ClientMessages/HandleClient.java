package Models.ClientMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class HandleClient extends RequestBase {
    @JsonProperty("newClient")
    public boolean NewClient;
}
