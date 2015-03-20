package es.ieeesb.teora.armduino.core;

import es.ieeesb.teora.armduino.util.Kattio;
import es.ieeesb.teora.armduino.util.Log;


public class CommandInterpreter implements Runnable
{

	private Kattio IO;
	private boolean active = true;
	
	public boolean isActive()
	{
		return active;
	}
	
	public void setActive(boolean active)
	{
		this.active = active;
	}
	
	public Kattio getIO()
	{
		return IO;
	}

	
	public CommandInterpreter(Kattio IO)
	{
		this.IO = IO;
		active = true;
	}
	
	

	@Override
	public void run()
	{
		Log.LogEvent(Log.SUBTYPE.COMMAND, "Command interpreter starting");
		while(active)
		{
			String incoming = null;
			String extra = null;
			if((incoming = IO.getWord()) != null)
			{
				try
				{
					incoming = incoming.toUpperCase();
					if(incoming.equals("STOP"))
					{
						active = false;
						Log.LogEvent(Log.SUBTYPE.COMMAND, "Command interpreter shutting down");
					}
					if(incoming.equals("MOVE"))
					{
						StringBuilder strBldr = new StringBuilder();
						for(int i = 0; i < Main.FIELD_COUNT + 1; i++)
						{
							strBldr.append(IO.getWord()+" ");
						}
						extra = strBldr.toString();
					}
					Main.commandDecoder.decode(CommandDecoder.COMMAND_TYPE.fromString(incoming), extra);
				}
				catch(Exception e)
				{
					Log.LogError(Log.SUBTYPE.DATA_INPUT, "Error decoding data: " + e.getMessage());
				}
			}
		}
	}
}
