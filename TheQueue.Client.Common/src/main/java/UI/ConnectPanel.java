package UI;

import Models.BroadcastMessages.QueueTicket;
import Models.ClientMessages.*;
import Services.MessageService;
import Services.UserMessageService;

import javax.swing.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
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
    private final JButton _connectButton;
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

    private MessageService _messageService;
    private UserMessageService _userMessageService;

    public ConnectPanel(String reqConnectionString, String subConnectionString, boolean isStudent) {
        _reqConnectionString = reqConnectionString;
        _subConnectionString = subConnectionString;
        _isStudent = isStudent;
        _clientId = UUID.randomUUID();
        _messageService = new MessageService();

        _nameField = new JTextField("", 10);

        _connectButton = new JButton("Connect");
        _connectButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                Connect();
            }
        });

        this.add(_nameField);
        this.add(_connectButton);

        _enterQueueButton = new JButton("Enter Queue");
        _enterQueueButton.addActionListener(e -> EnterQueue());
        _enterQueueButton.setVisible(false);
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

    private void Connect() {
        System.out.println("Connect");

        if (_nameField.getText().isEmpty()) {
            Popup.ShowError((JFrame) SwingUtilities.getAncestorOfClass(JFrame.class, this), "Name can't be empty");
            return;
        }

        SendEnterQueue(false);

        if (_heartbeatThread == null || !_heartbeatThread.isAlive()) {
            _heartbeatThread = new Thread(this::Heartbeat);
            _heartbeatThread.start();
        }

        _nameField.setEnabled(false);
        _connectButton.setVisible(false);
        _enterQueueButton.setVisible(true);
        _handleButton.setVisible(true);
    }

    private void EnterQueue() {
        SendEnterQueue(true);

        // Start listening for supervisor response
        if (_isStudent) {
            _userMessageService = new UserMessageService(_subConnectionString, _nameField.getText(),(JFrame) SwingUtilities.getAncestorOfClass(JFrame.class, this));
            // TODO: Make service go to "Done" state. Listen to event and make it possible to reenter queue???
            //_userMessageService.addPropertyChangeListener();
            _userMessageService.execute();
        }

        _enterQueueButton.setVisible(false);
        _leaveQueueButton.setVisible(true);
    }

    private void LeaveQueue() {
        System.out.println("Leave queue");

        /*if(_heartbeatThread != null) {
            _heartbeatThread.interrupt();
        }*/

        if (_userMessageService != null && !_userMessageService.isCancelled()) {
            _userMessageService.cancel(true);
        }

        SendEnterQueue(false);

        _leaveQueueButton.setVisible(false);
        _enterQueueButton.setVisible(true);
        _handleButton.setVisible(false);
        // _nameField.setEnabled(true);
        //_disconnectButton.setVisible(false);
        //_connectButton.setVisible(true);
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
                EnterQueue = connect;
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
            //_messageService.SendMessage(supervisorRequest, _reqConnectionString, Object.class);
            _ticket = _messageService.SendMessage(supervisorRequest, _reqConnectionString, QueueTicket.class);

            if (_ticket != null && _ticket.getTicket() != 0){
                String popupMessage = "Message to: " + _ticket.getName() + " Ticket: " + _ticket.getTicket();
                String result = Popup.ShowInput((JFrame) SwingUtilities.getAncestorOfClass(JFrame.class, this), popupMessage, "Message to Client");
                if (result == null || result.isEmpty()) {
                    //request
                    // _messageService.SendMessage()
                    return;
                }

                SendSupervisorMessage(new Recipient() {{ Recipient = _ticket.getName(); Body = result; }});
            }
        }
    }

    private void SendHandle() {
        /*HandleClient request = new HandleClient() {
            {
                ClientId = _clientId.toString();
                NewClient = true;
            }
        };*/

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
                //request
                // _messageService.SendMessage()
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
