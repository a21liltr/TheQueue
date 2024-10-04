package Models.ClientMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class SupervisorEnterQueue extends EnterQueue{
    @JsonProperty("supervisor")
    public boolean Supervisor = true;
}
