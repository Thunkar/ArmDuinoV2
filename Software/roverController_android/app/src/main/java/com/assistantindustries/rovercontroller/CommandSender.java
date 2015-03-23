package com.assistantindustries.rovercontroller;

/**
 * Created by Asistant on 16/03/2015.
 * Interface of commandSenders
 */
public interface CommandSender {

    /**
     * Method to alert the command sender that there is new joystick position data
     *
     * @param code      the position index
     * @param angle     the joystick angle
     * @param power     the joystick power
     * @param direction the joystick direction (unused)
     */
    public void updatePosition(int code, int angle, int power, int direction);

    /**
     * Method to indicate that creation-params have changed
     */
    public void update();

    /**
     * Method to shut down command sender
     */
    public void close();
}
