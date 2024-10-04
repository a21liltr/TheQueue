package UI;

import javax.swing.*;

public class Popup {
    public static void ShowMessage(JFrame frame, String message, String title) {
        JOptionPane.showMessageDialog(frame, message, title, JOptionPane.PLAIN_MESSAGE);
    }

    public static String ShowInput(JFrame frame, String message, String title) {
        return JOptionPane.showInputDialog(
            frame,
            message,
            title,
            JOptionPane.PLAIN_MESSAGE
        );
    }

    public static void ShowError(JFrame frame, String errorMessage){
        JOptionPane.showMessageDialog(frame, errorMessage, "Error", JOptionPane.ERROR_MESSAGE);
    }
}
