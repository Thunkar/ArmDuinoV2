package es.ieeesb.teora.armduino.core;


public class Motors
{
	private int frontLeftSpeed;
	private int frontRightSpeed;
	private int rearLeftSpeed;
	private int rearRightSpeed;
	
	private int frontLeftTwist;
	private int frontRightTwist;
	private int rearLeftTwist;
	private int rearRightTwist;
	
	public void parseMotorsLine(String sensorLine)
	{
		String[] splitted = sensorLine.trim().split(" ");
		
		if(splitted[0].equals("IDLE")) 
		{
			frontLeftTwist = 0;
			frontRightTwist = 0;
			rearLeftTwist = 0;
			rearRightTwist = 0;
			
			frontLeftSpeed = 0;
			frontRightSpeed = 0;
			rearLeftSpeed = 0;
			rearRightSpeed = 0;
			return;
		}
		
		if(!splitted[0].equals("POSITIONS:")) return;
		
		frontLeftTwist = Integer.parseInt(splitted[24]);
		frontRightTwist = Integer.parseInt(splitted[25]);
		rearLeftTwist = Integer.parseInt(splitted[26]);
		rearRightTwist = Integer.parseInt(splitted[27]);
		
		frontLeftSpeed = Integer.parseInt(splitted[28]);
		frontRightSpeed = Integer.parseInt(splitted[29]);
		rearLeftSpeed = Integer.parseInt(splitted[30]);
		rearRightSpeed = Integer.parseInt(splitted[31]);
	}
	
	public int getFrontLeftSpeed()
	{
		return frontLeftSpeed;
	}

	public void setFrontLeftSpeed(int frontLeftSpeed)
	{
		this.frontLeftSpeed = frontLeftSpeed;
	}

	public int getFrontRightSpeed()
	{
		return frontRightSpeed;
	}

	public void setFrontRightSpeed(int frontRightSpeed)
	{
		this.frontRightSpeed = frontRightSpeed;
	}

	public int getRearLeftSpeed()
	{
		return rearLeftSpeed;
	}

	public void setRearLeftSpeed(int rearLeftSpeed)
	{
		this.rearLeftSpeed = rearLeftSpeed;
	}

	public int getRearRightSpeed()
	{
		return rearRightSpeed;
	}

	public void setRearRightSpeed(int rearRightSpeed)
	{
		this.rearRightSpeed = rearRightSpeed;
	}

	public int getFrontLeftTwist()
	{
		return frontLeftTwist;
	}

	public void setFrontLeftTwist(int frontLeftTwist)
	{
		this.frontLeftTwist = frontLeftTwist;
	}

	public int getFrontRightTwist()
	{
		return frontRightTwist;
	}

	public void setFrontRightTwist(int frontRightTwist)
	{
		this.frontRightTwist = frontRightTwist;
	}

	public int getRearLeftTwist()
	{
		return rearLeftTwist;
	}

	public void setRearLeftTwist(int rearLeftTwist)
	{
		this.rearLeftTwist = rearLeftTwist;
	}

	public int getRearRightTwist()
	{
		return rearRightTwist;
	}

	public void setRearRightTwist(int rearRightTwist)
	{
		this.rearRightTwist = rearRightTwist;
	}

	public String getMotorsLine()
	{
		return frontLeftTwist + " " + frontRightTwist + " " + rearLeftTwist + " " + rearRightTwist + " " + frontLeftSpeed + " " + frontRightSpeed + " " + rearLeftSpeed + " " + rearRightSpeed;
	}
}
