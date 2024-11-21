package UI;

import Models.BroadcastMessages.QueueTicket;
import Models.ClientMessages.*;
import Services.MessageService;
import Services.UserMessageService;

import javax.swing.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.UUID;

/*
*
*
*  Has name input and a connect and disconnect button
*  Can't send empty name and error popup is shown
*
*
* */
public class ConnectPanel extends JPanel {
    private final JButton _enterQueueButton;
    private final JButton _leaveQueueButton;
    private final JButton _handleButton;
    private final JTextField _nameField;

    private final String _reqConnectionString;
    private final String _subConnectionString;
    private QueueTicket _ticket;
    private final boolean _isStudent;
    private final UUID _clientId;

    private Thread _heartbeatThread;

    private final MessageService _messageService;
    private UserMessageService _userMessageService;

    public ConnectPanel(String reqConnectionString, String subConnectionString, boolean isStudent) {
        _reqConnectionString = reqConnectionString;
        _subConnectionString = subConnectionString;
        _isStudent = isStudent;
        _clientId = UUID.randomUUID();
        _messageService = new MessageService();

        _nameField = new JTextField("", 10);

        this.add(_nameField);

        _enterQueueButton = new JButton("Enter Queue");
        _enterQueueButton.addActionListener(e -> EnterQueue());
        _enterQueueButton.setVisible(true);
        this.add(_enterQueueButton);

        _leaveQueueButton = new JButton("Leave Queue");
        _leaveQueueButton.addActionListener(e -> LeaveQueue());
        _leaveQueueButton.setVisible(false);
        this.add(_leaveQueueButton);

        _handleButton = new JButton("Handle Next");
        if (!_isStudent) {
            _handleButton.addActionListener(e -> Handle());
            _handleButton.setVisible(false);
            this.add(_handleButton);
        }
    }

    private void EnterQueue() {
        System.out.println("Connect");

        if (_nameField.getText().isEmpty()) {
            Popup.ShowError((JFrame) SwingUtilities.getAncestorOfClass(JFrame.class, this), "Name can't be empty");
            return;
        }

        SendEnterQueue(true);

        if (_heartbeatThread == null || !_heartbeatThread.isAlive()) {
            _heartbeatThread = new Thread(this::Heartbeat);
            _heartbeatThread.start();
        }

        // Start listening for supervisor response
        if (_isStudent) {
            _userMessageService = new UserMessageService(_subConnectionString, _nameField.getText(),(JFrame) SwingUtilities.getAncestorOfClass(JFrame.class, this));
            _userMessageService.execute();
        }
        else {
            _handleButton.setVisible(true);
        }

        _nameField.setEnabled(false);
        _enterQueueButton.setVisible(false);
        _leaveQueueButton.setVisible(true);
    }

    private void LeaveQueue() {
        System.out.println("Leave queue");

        if (_userMessageService != null && !_userMessageService.isCancelled()) {
            _userMessageService.cancel(true);
        }

        SendEnterQueue(false);

        _leaveQueueButton.setVisible(false);
        _enterQueueButton.setVisible(true);
        _handleButton.setVisible(false);
    }

    private void Handle() {
        System.out.println("Handle");

        SendHandle();
    }

    private void SendEnterQueue(boolean connect) {
        EnterQueue request = new EnterQueue() {
            {
                ClientId = _clientId.toString();
                Name = _nameField.getText();
                EnterQueue = _isStudent && connect;
            }
        };

        if (_isStudent) {
            var reply = _messageService.SendMessage(request, _reqConnectionString, QueueTicket.class);
            if (connect && _ticket != null && _ticket.getTicket() != 0) {
                _ticket = reply;
                System.out.println(_ticket);
            }
            else {
                _ticket = null;
            }
        }
        else {
            SupervisorEnterQueue supervisorRequest = new SupervisorEnterQueue() {
                {
                    ClientId = request.ClientId;
                    Name = request.Name;
                    EnterQueue = request.EnterQueue;
                }
            };
            _messageService.SendMessage(supervisorRequest, _reqConnectionString, QueueTicket.class);
        }
    }

    private void SendHandle() {
        SupervisorEnterQueue request = new SupervisorEnterQueue() {
            {
                ClientId = _clientId.toString();
                Name = _nameField.getText();
                EnterQueue = true;
            }
        };

        _ticket = _messageService.SendMessage(request, _reqConnectionString, QueueTicket.class);

        if (_ticket != null && _ticket.getTicket() != 0){
            String popupMessage = "Message to: " + _ticket.getName() + " Ticket: " + _ticket.getTicket();
            String result = Popup.ShowInput((JFrame) SwingUtilities.getAncestorOfClass(JFrame.class, this), popupMessage, "Message to Client");
            if (result == null || result.isEmpty()) {
                return;
            }

            SendSupervisorMessage(new Recipient() {{ Recipient = _ticket.getName(); Body = result; }});
        }
    }

    private void SendSupervisorMessage(Recipient recipient) {
        MessageRequest request = new MessageRequest() {
            {
                ClientId = _clientId.toString();
                Name = _nameField.getText();
                Message = recipient;
            }
        };

        _messageService.SendMessage(request, _reqConnectionString, Object.class);
        _ticket = null;
    }

    private void Heartbeat() {
        Heartbeat request = new Heartbeat() {
            {
                ClientId = _clientId.toString();
            }
        };
        while(!Thread.currentThread().isInterrupted()){
            var reply = _messageService.SendMessage(request, _reqConnectionString, QueueTicket.class);
            try {
                System.out.println("Heartbeat");
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                System.out.println("Heartbeat stopped");
                return;
            }
        }
    }
}
