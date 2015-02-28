package es.ieeesb.teora.armduino.core;

import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.Log;

public class CommandSender implements Runnable
{
	
	private boolean active = true;
	private LinkedBlockingQueue<char[]> commandsToSend;
	private ProtocolManager protocol;
	
	
	public CommandSender(ProtocolManager protocol, LinkedBlockingQueue<char[]> commandsToSend)
	{
		this.commandsToSend = commandsToSend;
		this.protocol = protocol;
	}

	public boolean isActive()
	{
		return active;
	}

	public void setActive(boolean active)
	{
		this.active = active;
	}

	@Override
	public void run()
	{
		Log.LogEvent(Log.SUBTYPE.DATA_OUTPUT, "Command sender starting");
		while (active)
		{
			if(!commandsToSend.isEmpty())
			{
				char[] commandToSend = commandsToSend.poll();
				protocol.write(commandToSend);
			}
		}
		Log.LogEvent(Log.SUBTYPE.DATA_OUTPUT, "Command sender shutting down");
	}

}
