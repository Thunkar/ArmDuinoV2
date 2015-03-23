package com.assistantindustries.rovercontroller;

/**
 * Created by Asistant on 16/03/2015.
 */
public interface CommandSender {

    public void updatePosition(int code, int angle, int power, int direction);


    public void update();

    public void close();
}
