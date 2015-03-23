package com.assistantindustries.rovercontroller;

/**
 * Created by Asistant on 3/17/2015.
 * Abstract class for the variables and methods that command senders have in common
 */
public abstract class AbstractCommandSender implements CommandSender {
    public static final int JOYSTICK_AMOUNT = 2;

    public static final String COMMAND_KEY = "command"; //Key to send through handler bundle

    public static final String CONNECT_COMMAND = "CONNECT";
    public static final String CLOSE_COMMAND = "CLOSE";

    public static final int SEND_COMMAND = 0; //"what" keys
    public static final int QUIT_CONNECTION = 1;
    public static final int UPDATE_SOCKET = 2;

    public static final String CODE_ADDRESS = "Address"; //Codes for sharedpreferences
    public static final String CODE_PORT = "Port";

    protected JoystickPosition[] positions;


    public AbstractCommandSender() { //default constructor
        positions = new JoystickPosition[JOYSTICK_AMOUNT]; //init positions
        for(int i=0;i<JOYSTICK_AMOUNT;i++){
            positions[i]=new JoystickPosition();
        }
    }

    @Override
    public abstract void updatePosition(int code, int angle, int power, int direction); //Implementation-dependant

}
