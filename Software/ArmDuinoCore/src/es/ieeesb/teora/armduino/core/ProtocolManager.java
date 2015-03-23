package es.ieeesb.teora.armduino.core;

import es.ieeesb.teora.armduino.util.Log;
import gnu.io.CommPortIdentifier;
import gnu.io.SerialPort;
import gnu.io.SerialPortEvent;
import gnu.io.SerialPortEventListener;


import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;


/**
 * Class that handles the serial port connection and communication
 * @author Gregorio
 *
 */
public class ProtocolManager implements SerialPortEventListener
{

	private static final int BAUD_RATE = 9600;
	private SerialPort serialPort;
	private OutputStream output;
	private InputStream input;

	/**
	 * Initializes the serial port connection according to the provided port name and baud rate.
	 * @param port
	 * @param baudRate
	 */
	public ProtocolManager(String port, String baudRate)
	{
		try
		{
			Log.LogEvent(Log.SUBTYPE.PORT_CONNECTION, "Opening port connection");
			CommPortIdentifier portId = CommPortIdentifier.getPortIdentifier(port);
			serialPort = (SerialPort) portId.open(port, 2000);
			serialPort.setSerialPortParams(BAUD_RATE, SerialPort.DATABITS_8,SerialPort.STOPBITS_1, SerialPort.PARITY_NONE);
			serialPort.addEventListener(this);
			serialPort.notifyOnDataAvailable(true);
			input = serialPort.getInputStream();
			output = serialPort.getOutputStream();
			Log.LogEvent(Log.SUBTYPE.PORT_CONNECTION, "Port connection established");
		}
		catch (Exception e)
		{
			Log.LogError(Log.SUBTYPE.PORT_CONNECTION, "Couldn't connect to port " + port + ". " + e.getMessage());
			if(Log.DEBUG)
				e.printStackTrace();
			System.exit(-1);
		}
	}

	
	/*
	 * Serial event data received event handler.
	 * @see gnu.io.SerialPortEventListener#serialEvent(gnu.io.SerialPortEvent)
	 */
	@Override
	public synchronized void serialEvent(SerialPortEvent event)
	{
		switch (event.getEventType())
		{
		case SerialPortEvent.DATA_AVAILABLE:
			readPort();
			break;
		}
	}

	/**
	 * Reads the serial port and logs the incoming data.
	 */
	private void readPort()
	{
		try
		{
			int incomingByte = input.read();
			StringBuilder incomingData = new StringBuilder();
			while(incomingByte != -1 && incomingByte != 10 && incomingByte != 13)
			{
				incomingData.append((char)incomingByte);
				incomingByte = input.read();
			}
			String inputLine = incomingData.toString();
			if(inputLine != "" && inputLine.length() != 0)
			{
				Log.LogEvent(Log.SUBTYPE.INCOMING, inputLine);
			}
			
		}
		catch (IOException e)
		{
			Log.LogError(Log.SUBTYPE.PORT_READING,"Serial port reading error: " + e.getMessage());
		}
	}

	
	/**
	 * Writes a message to the serial port.
	 * @param message in raw bytes
	 */
	public void write(char[] message)
	{
		try
		{
			StringBuilder messageStr = new StringBuilder();
			for(int i = 0; i < message.length; i++)
			{
				messageStr.append(message[i]);
				output.write(message[i]);	
			}
			Log.LogDebug(Log.SUBTYPE.PORT_WRITING, "Writing: " + messageStr.toString() + " to port: " + serialPort.getName());
			output.flush();
			output.close();
		}
		catch (Exception e)
		{
			Log.LogError(Log.SUBTYPE.PORT_WRITING, "Serial port writing errror: " + e.getMessage());
		}
	}
	
	
	/**
	 * Helper method that shuts down the serial port input
	 */
	public void closeInput()
	{
		try
		{
			Log.LogEvent(Log.SUBTYPE.PORT_READING, "Serial port input shutting down");
			input.close();
		}
		catch (IOException e)
		{
			if(Log.DEBUG)
				e.printStackTrace();
		}		
	}
	
	/**
	 * Helper method that shuts down the serial port output
	 */
	public void closeOutput()
	{
		try
		{
			Log.LogEvent(Log.SUBTYPE.PORT_WRITING, "Serial port output shutting down");
			output.close();
			serialPort.close();
		}
		catch (IOException e)
		{
			if(Log.DEBUG)
				e.printStackTrace();
		}		
	}
}
