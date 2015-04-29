package es.ieeesb.teora.armduino.core;

import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.Log;

public class CommandDecoder
{
	
	/**
	 * Thread-safe queue that holds the commands that have not been sent yet
	 */
	private LinkedBlockingQueue<char[]> motorsCommandQueue;
	private LinkedBlockingQueue<char[]> sensorsCommandQueue;
	
	/** 
	 * Constructor
	 * @param motorsCommandQueue
	 */
	public CommandDecoder(LinkedBlockingQueue<char[]> motorsCommandQueue, LinkedBlockingQueue<char[]> sensorsCommandQueue)
	{
		this.motorsCommandQueue = motorsCommandQueue;
		this.sensorsCommandQueue = sensorsCommandQueue;
	}
	
	/**
	 * Describes the different supported commands types.
	 * @author Gregorio
	 *
	 */
	public enum COMMAND_TYPE 
	{ 
		READSENSORS, MOVE, CONNECT, RESET, INVALID, STOP, QUEUE, READMOTORS; 
		
		public static COMMAND_TYPE fromString(String str)
		{
			switch(str)
			{
			case "READSENSORS":
				return READSENSORS;
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
			case "READMOTORS":
				return READMOTORS;
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
		case READMOTORS:
			motorsCommandQueue.put(MessageConformer.getReadMotorsRequest());
			return;
		case READSENSORS:
			Log.LogEvent(Log.SUBTYPE.SENSORS, Main.Sensors.getBattery1() + " " + Main.Sensors.getBattery2());
			return;
		case MOVE:
			Log.LogEvent(Log.SUBTYPE.ACK, "ACK");
			motorsCommandQueue.put(MessageConformer.getMoveMessage(extra));
			return;
		case CONNECT:
			Log.LogEvent(Log.SUBTYPE.ACK, "ACK");
			char[] commandToSend = MessageConformer.getConnectRequest();
			motorsCommandQueue.put(commandToSend);
			return;
		case RESET:
			Log.LogEvent(Log.SUBTYPE.ACK, "ACK");
			motorsCommandQueue.put(MessageConformer.getResetRequest());
			return;
		case QUEUE:
			Log.LogWarning(Log.SUBTYPE.SYSTEM, "Command queue size: " + motorsCommandQueue.size());
			return;
		case STOP:
			Log.LogEvent(Log.SUBTYPE.ACK, "ACK");
			motorsCommandQueue.put(MessageConformer.getResetRequest());
			Main.stop();
			return;
		}
	}
}
