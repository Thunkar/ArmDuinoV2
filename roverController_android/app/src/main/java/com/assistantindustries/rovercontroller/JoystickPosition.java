package com.assistantindustries.rovercontroller;

/**
 * Created by Asistant on 16/03/2015.
 */
public class JoystickPosition {

    public static final int CODE_LEFT = 0; //Codes must be incremental (they are mapped to array positions)
    public static final int CODE_RIGHT = 1;


    private int angle;
    private int power;

    public JoystickPosition(int angle, int power) {
        this.angle = angle;
        this.power = power;
    }

    public JoystickPosition() {
        this.angle = 0;
        this.power = 0;
    }

    public int getAngle() {
        return angle;
    }

    public int getPower() {
        return power;
    }

    /**
     * Method to obtain if the direction is left, right, forward or backward
     * @return An enum instance indicating the direction
     */
    public JoystickDirection getJoystickDirection() {
        int angle = 90 - this.angle;
        if (angle >= 315 || angle < 45) {
            return JoystickDirection.RIGHT;
        } else {
            if (angle >= 45 && angle < 135) {
                return JoystickDirection.FORWARD;
            } else {
                if (angle >= 135 && angle < 225) {
                    return JoystickDirection.LEFT;
                } else {
                    return JoystickDirection.BACKWARD;
                }
            }
        }
    }

    public enum JoystickDirection {FORWARD, RIGHT, LEFT, BACKWARD}
}
