package es.ieeesb.teora.armduino.core;


import java.lang.reflect.Field;
import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.*;

public class Main
{
	public static String PORT;
	public static String BAUD_RATE;
	public static int FIELD_COUNT;
	public static int FIELD_SIZE;
	public static boolean SERVER_ENABLED;
	public static int SERVER_PORT;
	public static CommandDecoder commandDecoder;
	public static TCPServer server;
	
	private static ProtocolManager protocol;
	private static CommandInterpreter commandInterpreter;
	private static CommandSender commandSender;
	private static Thread commandInterpreterThread;
	private static Thread commandSenderThread;
	private static Thread serverThread;
	

	public static void main(String[] args)
	{
		String libPath = System.getProperty("user.dir") + "/lib/";
		System.setProperty("java.library.path", libPath );
		try
		{
			Field fieldSysPath = ClassLoader.class.getDeclaredField( "sys_paths" );
			fieldSysPath.setAccessible( true );
			fieldSysPath.set( null, null );
			PORT = args[0];
			BAUD_RATE = args[1];	
			FIELD_COUNT = Integer.parseInt(args[2]);
			FIELD_SIZE = Integer.parseInt(args[3]);
			SERVER_ENABLED = Boolean.parseBoolean(args[4]);
			SERVER_PORT = Integer.parseInt(args[5]);
			Log.DEBUG = Boolean.parseBoolean(args[6]);
		}
		catch (Exception e)
		{
			Log.LogError(Log.SUBTYPE.SYSTEM, "Invalid args!");
			System.exit(-1);
		}

		LinkedBlockingQueue<char[]> commandsToSend = new LinkedBlockingQueue<char[]>();
		Kattio sysIO = new Kattio(System.in, System.out);
		server = null;
		protocol = new ProtocolManager(PORT, BAUD_RATE);
		

		commandDecoder = new CommandDecoder(commandsToSend);
		commandInterpreter = new CommandInterpreter(sysIO);
		commandSender = new CommandSender(protocol, commandsToSend);
		commandSenderThread = new Thread(commandSender);
		commandInterpreterThread = new Thread(commandInterpreter);
		commandInterpreterThread.start();
		if(SERVER_ENABLED)
		{
			server = new TCPServer(SERVER_PORT);
			serverThread = new Thread(server);
			serverThread.start();
		}
		commandSenderThread.start();
	}
	
	public static void stop()
	{
		Log.LogEvent(Log.SUBTYPE.SYSTEM, "Shutting down");
		commandInterpreter.setActive(false);
		server.setActive(false);
		if(SERVER_ENABLED && server != null)
			server.close();
		commandSender.setActive(false);
		protocol.closeInput();
		protocol.closeOutput();
		Log.LogEvent(Log.SUBTYPE.SYSTEM, "Bye");
		System.exit(0);
	}

}
