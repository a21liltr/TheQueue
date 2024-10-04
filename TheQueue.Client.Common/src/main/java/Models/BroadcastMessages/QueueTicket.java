package Models.BroadcastMessages;

import com.fasterxml.jackson.annotation.JsonProperty;

public class QueueTicket {
    @JsonProperty
    private String Name;
    @JsonProperty
    private int Ticket;

    public String getName() {
        return Name;
    }

    public void setName(String name) {
        Name = name;
    }

    public int getTicket() {
        return Ticket;
    }

    public void setTicket(int ticket) {
        Ticket = ticket;
    }
}
