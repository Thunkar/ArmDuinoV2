package es.ieeesb.teora.armduino.core;

public class Navigation
{
	private Motors motors;
	private Sensors sensors;
	
	private int frontLeftEncoder;
	private int frontRightEncoder;
	private int rearLeftEncoder;
	private int rearRightEncoder;
	
	public Navigation(Motors motors, Sensors sensors)
	{
		this.motors = motors;
		this.sensors = sensors;
	}
	
	public void updateNavigation()
	{
		frontLeftEncoder = motors.getFrontLeftSpeed() > 255 ? frontLeftEncoder + sensors.getFrontLeft() : frontLeftEncoder - sensors.getFrontLeft();
		frontRightEncoder = motors.getFrontRightSpeed() > 255 ? frontRightEncoder + sensors.getFrontRight() : frontRightEncoder - sensors.getFrontRight();
		rearLeftEncoder = motors.getRearLeftSpeed() > 255 ? rearLeftEncoder + sensors.getRearLeft() : rearLeftEncoder - sensors.getRearLeft();
		rearRightEncoder = motors.getRearRightSpeed() > 255 ? rearRightEncoder + sensors.getRearRight() : rearRightEncoder - sensors.getRearRight();
	}
	
	public String getNavigationline()
	{
		String result = frontLeftEncoder + " " + frontRightEncoder + " " + rearLeftEncoder + " " + rearRightEncoder;
		frontLeftEncoder = 0;
		frontRightEncoder = 0;
		rearLeftEncoder = 0;
		rearRightEncoder = 0;
		return result;
	}
}
