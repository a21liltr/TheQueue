package Models.ClientMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class RequestBase {
    @JsonProperty("clientId")
    public String ClientId;
}