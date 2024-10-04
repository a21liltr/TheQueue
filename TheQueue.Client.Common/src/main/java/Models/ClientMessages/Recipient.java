package Models.ClientMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class Recipient {
    @JsonProperty("recipient")
    public String Recipient;
    @JsonProperty("body")
    public String Body;
}