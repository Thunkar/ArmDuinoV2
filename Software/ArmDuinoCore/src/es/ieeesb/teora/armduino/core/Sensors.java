package es.ieeesb.teora.armduino.core;

public class Sensors
{

	private int battery1;
	private int battery2;
	
	private int frontLeft;
	private int frontRight;
	private int rearLeft;
	private int rearRight;
	
	public int getFrontLeft()
	{
		return frontLeft;
	}
	public void setFrontLeft(int frontLeft)
	{
		this.frontLeft = frontLeft;
	}
	public int getFrontRight()
	{
		return frontRight;
	}
	public void setFrontRight(int frontRight)
	{
		this.frontRight = frontRight;
	}
	public int getRearLeft()
	{
		return rearLeft;
	}
	public void setRearLeft(int rearLeft)
	{
		this.rearLeft = rearLeft;
	}
	public int getRearRight()
	{
		return rearRight;
	}
	public void setRearRight(int rearRight)
	{
		this.rearRight = rearRight;
	}
	public int getBattery1()
	{
		return battery1;
	}
	public void setBattery1(int battery1)
	{
		this.battery1 = battery1;
	}
	public int getBattery2()
	{
		return battery2;
	}
	public void setBattery2(int battery2)
	{
		this.battery2 = battery2;
	}
	
	public void parseSensorsLine(String sensorLine)
	{
		String[] splitted = sensorLine.split(" ");
		battery1 = Integer.parseInt(splitted[0]);
		battery2 = Integer.parseInt(splitted[1]);
		frontLeft = Integer.parseInt(splitted[2]);
		frontRight = Integer.parseInt(splitted[3]);
		rearLeft = Integer.parseInt(splitted[4]);
		rearRight = Integer.parseInt(splitted[5]);
	}
	
}
