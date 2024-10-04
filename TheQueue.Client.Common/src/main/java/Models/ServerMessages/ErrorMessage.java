package Models.ServerMessages;

import Enums.ErrorType;
import com.fasterxml.jackson.annotation.JsonProperty;

public class ErrorMessage {

    @JsonProperty("error")
    public ErrorType Error;
    @JsonProperty("msg")
    public String Message;
}