package es.ieeesb.teora.armduino.core;

import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.Log;

public class CommandSender implements Runnable
{
	
	private boolean active = true;
	/**
	 * Thread-safe queue shared with the decoder. Every command stored here is set to be sent to the robot
	 */
	private LinkedBlockingQueue<char[]> motorCommandsToSend;
	private ProtocolManager motorsPort;
	private LinkedBlockingQueue<char[]> sensorCommandsToSend;
	private ProtocolManager sensorsPort;
	
	
	public CommandSender(ProtocolManager motorsPort, ProtocolManager sensorsPort, LinkedBlockingQueue<char[]> motorCommandsToSend, LinkedBlockingQueue<char[]> sensorCommandsToSend)
	{
		this.motorCommandsToSend = motorCommandsToSend;
		this.motorsPort = motorsPort;
		this.sensorCommandsToSend = sensorCommandsToSend;
		this.sensorsPort = sensorsPort;
	}

	public boolean isActive()
	{
		return active;
	}

	public void setActive(boolean active)
	{
		this.active = active;
	}

	/* 
	 * Implementation of the run method that checks if there are commands to send. If it is the case, it sends them to the protocol manager (FIFO).
	 * @see java.lang.Runnable#run()
	 */
	@Override
	public void run()
	{
		Log.LogEvent(Log.SUBTYPE.DATA_OUTPUT, "Command sender starting");
		while (active)
		{
			if(!motorCommandsToSend.isEmpty())
			{
				char[] commandToSend = motorCommandsToSend.poll();
				motorsPort.write(commandToSend);
			}
			if(!sensorCommandsToSend.isEmpty())
			{
				char[] commandToSend = sensorCommandsToSend.poll();
				sensorsPort.write(commandToSend);
			}
		}
		Log.LogEvent(Log.SUBTYPE.DATA_OUTPUT, "Command sender shutting down");
	}

}
