package es.ieeesb.teora.armduino.core;

public class Sensors
{

	private int battery1;
	private int battery2;
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
	}
	
}
