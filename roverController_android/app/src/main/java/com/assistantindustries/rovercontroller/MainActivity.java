package com.assistantindustries.rovercontroller;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.view.Menu;
import android.view.MenuItem;

import com.zerokol.views.JoystickView;

import static com.assistantindustries.rovercontroller.JoystickPosition.CODE_LEFT;
import static com.assistantindustries.rovercontroller.JoystickPosition.CODE_RIGHT;


public class MainActivity extends ActionBarActivity {
    private static final int SETTINGS_REQUEST = 0;
    private JoystickView leftJoystick;
    private JoystickView rightJoystick;
    private CommandSender commandSender;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        leftJoystick = (JoystickView) findViewById(R.id.left_joystick);
        rightJoystick = (JoystickView) findViewById(R.id.right_joystick);
        if (this.commandSender == null) {
            this.commandSender = new TcpSender(this);
        } else {
            this.commandSender.update();
        }
        leftJoystick.setOnJoystickMoveListener(new JoystickView.OnJoystickMoveListener() {
            @Override
            public void onValueChanged(int angle, int power, int direction) {
                commandSender.updatePosition(CODE_LEFT, angle, power, direction);
            }
        }, JoystickView.DEFAULT_LOOP_INTERVAL);
        rightJoystick.setOnJoystickMoveListener(new JoystickView.OnJoystickMoveListener() {
            @Override
            public void onValueChanged(int angle, int power, int direction) {
                commandSender.updatePosition(CODE_RIGHT, angle, power, direction);
            }
        }, JoystickView.DEFAULT_LOOP_INTERVAL);
    }


    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            startActivityForResult(new Intent(this, SettingsActivity.class), SETTINGS_REQUEST);
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        switch (requestCode) {
            case SETTINGS_REQUEST:
                //S this.commandSender.restartSenderThread();
                break;
        }
    }

    @Override
    protected void onDestroy() {
        commandSender.close();
        super.onDestroy();
    }
}
