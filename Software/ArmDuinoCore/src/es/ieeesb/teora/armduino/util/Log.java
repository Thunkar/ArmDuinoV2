package es.ieeesb.teora.armduino.util;

import java.text.SimpleDateFormat;
import java.util.Calendar;

import es.ieeesb.teora.armduino.core.Main;
import es.ieeesb.teora.armduino.core.TCPServer;

public class Log
{

	public enum TYPE
	{
		ERROR, EVENT, WARNING, HIDDEN, DEBUG
	}

	public enum SUBTYPE
	{
		SYSTEM, PORT_CONNECTION, PORT_WRITING, PORT_READING, DATA_INPUT, DATA_OUTPUT, MOTORS, SENSORS, COMMAND, TCP_SERVER, TCP_CLIENT, NFC, ACK
	}

	public static boolean DEBUG;

	public static void LogEvent(SUBTYPE subtype, String message)
	{

		String timeStamp = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(Calendar
				.getInstance().getTime());
		String sentence = "[" + timeStamp + "]" + "[" + TYPE.EVENT.toString() + "]["
				+ subtype.toString() + "] " + message;
		System.out.println(sentence);
		if (Main.SERVER_ENABLED && Main.server != null && Main.server.isActive())
			TCPServer.write(sentence + '\n');
	}

	public static void LogWarning(SUBTYPE subtype, String message)
	{
		String timeStamp = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(Calendar
				.getInstance().getTime());
		String sentence = "[" + timeStamp + "]" + "[" + TYPE.WARNING.toString() + "]["
				+ subtype.toString() + "] " + message;
		System.out.println(sentence);
		if (Main.SERVER_ENABLED && Main.server != null && Main.server.isActive())
			TCPServer.write(sentence + '\n');

	}

	public static void LogError(SUBTYPE subtype, String message)
	{
		String timeStamp = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(Calendar
				.getInstance().getTime());
		String sentence = "[" + timeStamp + "]" + "[" + TYPE.ERROR.toString() + "]["
				+ subtype.toString() + "] " + message;
		System.out.println(sentence);
		if (Main.SERVER_ENABLED && Main.server != null && Main.server.isActive())
			TCPServer.write(sentence + '\n');
	}

	public static void LogDebug(SUBTYPE subtype, String message)
	{
		if (DEBUG)
		{
			String timeStamp = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(Calendar
					.getInstance().getTime());
			String sentence = "[" + timeStamp + "]" + "[" + TYPE.DEBUG.toString() + "]["
					+ subtype.toString() + "] " + message;
			System.out.println(sentence);
			if (Main.SERVER_ENABLED && Main.server != null && Main.server.isActive())
				TCPServer.write(sentence + '\n');
		}
	}

}
