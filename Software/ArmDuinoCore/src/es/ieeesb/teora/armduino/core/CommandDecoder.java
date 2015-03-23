package es.ieeesb.teora.armduino.core;

import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.Log;

public class CommandDecoder
{
	
	/**
	 * Thread-safe queue that holds the commands that have not been sent yet
	 */
	private LinkedBlockingQueue<char[]> commandQueue;
	
	/** 
	 * Constructor
	 * @param commandQueue
	 */
	public CommandDecoder(LinkedBlockingQueue<char[]> commandQueue)
	{
		this.commandQueue = commandQueue;
	}
	
	/**
	 * Describes the different supported commands types.
	 * @author Gregorio
	 *
	 */
	public enum COMMAND_TYPE 
	{ 
		STATUS, MOVE, CONNECT, RESET, INVALID, STOP, QUEUE; 
		
		public static COMMAND_TYPE fromString(String str)
		{
			switch(str)
			{
			case "STATUS":
				return STATUS;
			case "MOVE":
				return MOVE;
			case "CONNECT":
				return CONNECT;
			case "RESET":
				return RESET;
			case "STOP":
				return STOP;
			case "QUEUE":
				return QUEUE;
			default:
				return INVALID;
			}
		}
		
	}
	
	/**
	 * Takes a command and calls the message conformer in order to put it into the send queue
	 * @param command
	 * @param extra
	 * @throws InterruptedException
	 */
	public synchronized void decode(COMMAND_TYPE command, String extra) throws InterruptedException
	{
		switch(command)
		{
		case INVALID:
			Log.LogError(Log.SUBTYPE.COMMAND, "Command not recognized");
			return;
		case STATUS:
			commandQueue.put(MessageConformer.getStatusRequest());
			return;
		case MOVE:
			commandQueue.put(MessageConformer.getMoveMessage(extra));
			return;
		case CONNECT:
			char[] commandToSend = MessageConformer.getConnectRequest();
			commandQueue.put(commandToSend);
			return;
		case RESET:
			commandQueue.put(MessageConformer.getResetRequest());
			return;
		case QUEUE:
			Log.LogWarning(Log.SUBTYPE.SYSTEM, "Command queue size: " + commandQueue.size());
			return;
		case STOP:
			commandQueue.put(MessageConformer.getResetRequest());
			Main.stop();
			return;
		}
	}
}
