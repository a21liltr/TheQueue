package org.example;

import UI.MainFrame;

public class Supervisor {
    public static void main(String[] args) throws Exception {
        System.out.println("Supervisor Client started");

        new MainFrame(args, false);
    }
}