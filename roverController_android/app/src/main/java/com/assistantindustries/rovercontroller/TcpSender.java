package com.assistantindustries.rovercontroller;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.preference.PreferenceManager;
import android.util.Log;

import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;

import static com.assistantindustries.rovercontroller.JoystickPosition.CODE_LEFT;
import static com.assistantindustries.rovercontroller.JoystickPosition.CODE_RIGHT;

/**
 * Created by Asistant on 16/03/2015.
 * This class is a command sender that works over TCP sending commands to the CLI running in a server.
 * It uses the tcp address and port preferences.
 */
public class TcpSender extends AbstractCommandSender {

    private static final String ENABLED_SERVO_STRING = "000000011111111"; //This string is appended to the message and indicates which servos are enabled.
    private SendThread sendThread;
    private Context ctx; //context is used to get application preferences

    public TcpSender(Context ctx) {
        super();
        this.ctx = ctx;
        initSenderThread(); // On initialization, init sender thread
    }

    private void initSenderThread() {
        this.sendThread = new SendThread(getTcpAddress(), getPort());
        this.sendThread.start();
    }

    public String getTcpAddress() {
        SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(ctx);
        return prefs.getString("tcp_address", "192.168.1.2");
    }

    public int getPort() {
        SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(ctx);
        return Integer.parseInt(prefs.getString("tcp_port", "3000"));
    }

    //See abstract command sender javadoc
    @Override
    public void updatePosition(int code, int angle, int power, int direction) {
        this.positions[code] = new JoystickPosition(angle, power); //update joystick position
        sendPositions(); //send to CLI
    }

    @Override
    public void close() {
        this.sendThread.getHandler().sendEmptyMessage(QUIT_CONNECTION);
    }

    /**
     * method used to send a cli formatted command through sender thread
     * @param command the command in raw string
     */
    private void sendCommand(String command) {
        Handler handler = sendThread.getHandler();
        if (handler != null) {
            Message msg = handler.obtainMessage(SEND_COMMAND);
            msg.getData().putString(COMMAND_KEY, command);
            handler.sendMessage(msg);
        }
    }

    /**
     * This method formats data to a cli-compliant string
     * @return A string that can be sended to CLI
     */
    private String buildPositionsString() {
        StringBuilder sb = new StringBuilder();
        sb.append("MOVE");
        for (int i = 0; i < 6; i++) {
            sb.append(" 90 ");
        }
        sb.append(" 170 ");
        int angle = positions[CODE_RIGHT].getAngle();
        if (angle > 90) angle = 90 - angle;
        if (angle < -90) angle = Math.abs(angle) - 90;
        angle = angle + 90;
        sb.append(" " + angle + " ");
        sb.append(" " + angle + " ");
        sb.append(" " + angle + " ");
        sb.append(" " + angle + " ");
        switch (positions[CODE_LEFT].getJoystickDirection()) {
            case RIGHT:
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                break;
            case FORWARD:
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                break;
            case LEFT:
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + positions[CODE_LEFT].getPower() + " ");
                break;
            case BACKWARD:
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                sb.append(" " + -positions[CODE_LEFT].getPower() + " ");
                break;
        }
        sb.append(ENABLED_SERVO_STRING);
        String join = sb.toString().replace("  ", " "); //Remove double whitespaces
        Log.d("Rover", join);
        return join;
    }


    public void sendPositions() {
        if (positions[0] != null && positions[1] != null)
            sendCommand(buildPositionsString());
    }

    public void update() {
        updateSocket();
    }

    /**
     * Sends an update command to the sender thread
     */
    private void updateSocket() {
        Message msg = sendThread.getHandler().obtainMessage(UPDATE_SOCKET);
        msg.getData().putString(CODE_ADDRESS, getTcpAddress());
        msg.getData().putInt(CODE_PORT, getPort());
        sendThread.getHandler().sendMessage(msg);
    }

    /**
     * This class is a thread that accepts orders from the ui thread
     */
    private class SendThread extends Thread {

        private Handler handler;
        private Socket socket;
        private PrintWriter out;
        private String address;
        private int port;


        public SendThread(String address, int port) {
            this.address = address;
            this.port = port;
        }


        public Handler getHandler() {
            return handler;
        }


        @Override
        public void run() {
            super.run();
            Looper.prepare();
            try {
                socket = new Socket(address, port);
                out = new PrintWriter(socket.getOutputStream(), true);
                out.println(CONNECT_COMMAND); //First sends the connect command to the CLI
            } catch (IOException e) {
                e.printStackTrace();
            }
            this.handler = new Handler() {
                @Override
                public void handleMessage(Message msg) {
                    super.handleMessage(msg);
                    switch (msg.what) {
                        case SEND_COMMAND: //Sends a command directly to the CLI
                            if (out != null) {
                                String command = msg.getData().getString(COMMAND_KEY);
                                out.println(command);
                            }
                            break;
                        case QUIT_CONNECTION: //Gently destroys connection
                            if (out != null) {
                                out.println(CLOSE_COMMAND);
                            }
                            if (socket != null && !socket.isClosed()) {
                                try {
                                    socket.close();
                                } catch (IOException e) {
                                    e.printStackTrace();
                                }
                            }

                            break;
                        case UPDATE_SOCKET: //Closes sockets and opens it again
                            String newAddress = msg.getData().getString(CODE_ADDRESS);
                            int newPort = msg.getData().getInt(CODE_PORT);
                            if (!address.equals(newAddress) || newPort != newPort)
                                closeAndReopen();
                            break;
                    }

                }
            };
            Looper.loop();
        }

        private void closeAndReopen() {
            try {
                if (socket != null && !socket.isClosed()) {
                    socket.close();
                }
                socket = new Socket(address, port);
                out = new PrintWriter(socket.getOutputStream());
                out.println(CONNECT_COMMAND);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}
