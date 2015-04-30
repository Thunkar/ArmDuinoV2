package es.ieeesb.teora.armduino.core;

import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.Kattio;
import es.ieeesb.teora.armduino.util.Log;

public class Main
{
	public static String MOTORS_PORT;
	public static String SENSORS_PORT;
	public static int MOTORS_BAUD_RATE;
	public static int SENSORS_BAUD_RATE;
	public static int FIELD_COUNT;
	public static int FIELD_SIZE;
	public static boolean SERVER_ENABLED;
	public static int SERVER_PORT;
	public static boolean NFC_ENABLED;
	public static CommandDecoder commandDecoder;
	public static TCPServer server;
	public static Sensors Sensors;

	private static ProtocolManager motorsPort;
	private static ProtocolManager sensorsPort;
	private static CommandLineReader commandLineReader;
	private static CommandSender commandSender;
	private static NFC NFC;
	private static Thread commandInterpreterThread;
	private static Thread commandSenderThread;
	private static Thread serverThread;
	private static Thread nfcThread;

	/**
	 * Main method. Parses the arguments and initializes all the worker threads
	 * in the correct order.
	 * 
	 * @param args
	 */
	public static void main(String[] args)
	{
		try
		{
			MOTORS_PORT = args[0];
			SENSORS_PORT = args[1];
			MOTORS_BAUD_RATE = Integer.parseInt(args[2]);
			SENSORS_BAUD_RATE = Integer.parseInt(args[3]);
			FIELD_COUNT = Integer.parseInt(args[4]);
			FIELD_SIZE = Integer.parseInt(args[5]);
			SERVER_ENABLED = Boolean.parseBoolean(args[6]);
			SERVER_PORT = Integer.parseInt(args[7]);
			NFC_ENABLED = Boolean.parseBoolean(args[8]);
			Log.DEBUG = Boolean.parseBoolean(args[9]);
		}
		catch (Exception e)
		{
			Log.LogError(Log.SUBTYPE.SYSTEM, "Invalid args!");
			System.exit(-1);
		}

		LinkedBlockingQueue<char[]> motorCommandsToSend = new LinkedBlockingQueue<char[]>();
		LinkedBlockingQueue<char[]> sensorCommandsToSend = new LinkedBlockingQueue<char[]>();
		Kattio sysIO = new Kattio(System.in, System.out);
		Sensors = new Sensors();
		server = null;
		motorsPort = new ProtocolManager(MOTORS_PORT, MOTORS_BAUD_RATE, Log.SUBTYPE.MOTORS);
		sensorsPort = new ProtocolManager(SENSORS_PORT, SENSORS_BAUD_RATE, Log.SUBTYPE.SENSORS);
		commandDecoder = new CommandDecoder(motorCommandsToSend, sensorCommandsToSend);
		commandLineReader = new CommandLineReader(sysIO);
		commandSender = new CommandSender(motorsPort, sensorsPort, motorCommandsToSend, sensorCommandsToSend);
		commandSenderThread = new Thread(commandSender);
		commandInterpreterThread = new Thread(commandLineReader);
		commandInterpreterThread.start();
		if (SERVER_ENABLED)
		{
			server = new TCPServer(SERVER_PORT);
			serverThread = new Thread(server);
			serverThread.start();
		}
		if (NFC_ENABLED)
		{
			NFC = new NFC();
			nfcThread = new Thread(NFC);
			nfcThread.start();
		}
		commandSenderThread.start();
	}

	/**
	 * Stops everything and shuts the CLI down.
	 */
	public static void stop()
	{
		Log.LogEvent(Log.SUBTYPE.SYSTEM, "Shutting down");
		if (SERVER_ENABLED && server != null)
		{
			server.setActive(false);
			server.close();
		}
		if (NFC_ENABLED)
		{
			NFC.setActive(false);
		}
		commandSender.setActive(false);
		motorsPort.closeInput();
		motorsPort.closeOutput();
		sensorsPort.closeInput();
		sensorsPort.closeOutput();
		Log.LogEvent(Log.SUBTYPE.SYSTEM, "Bye");
		System.exit(0);
	}

}
