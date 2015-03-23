package es.ieeesb.teora.armduino.core;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.Socket;

import es.ieeesb.teora.armduino.util.Log;

/**
 * Represents a client connected to the TCP server and manages its lifecycle
 * @author Gregorio
 *
 */
public class TCPClientWorker implements Runnable
{
	private Socket clientSocket;
	public TCPClientWorker(Socket clientSocket)
	{
		this.clientSocket = clientSocket;
	}

	/* 
	 * Implementation of the run method that handles client communication and connection/disconnection. 
	 * @see java.lang.Runnable#run()
	 */
	@Override
	public void run()
	{
		while (!clientSocket.isClosed())
		{
			try
			{
				BufferedReader inFromClient = new BufferedReader(new InputStreamReader(
						clientSocket.getInputStream()));
				String[] data = inFromClient.readLine().split(" ");
				String extras = null;
				if (data.length > 1)
				{
					StringBuilder strBuilder = new StringBuilder();
					for (int i = 1; i < Main.FIELD_COUNT + 2; i++)
					{
						try
						{
							strBuilder.append(data[i] + " ");
						}
						catch (Exception e)
						{
							Log.LogError(Log.SUBTYPE.DATA_INPUT, "Invalid field count!");
						}
					}
					extras = strBuilder.toString();
				}
				data[0] = data[0].toUpperCase();
				if (data[0].equals("STOP"))
				{
					clientSocket.close();
					TCPServer.Clients.remove(this);
					Log.LogEvent(Log.SUBTYPE.TCP_CLIENT, "Client disconnected");
				}
				Main.commandDecoder.decode(CommandDecoder.COMMAND_TYPE.fromString(data[0]), extras);
			}
			catch (Exception e)
			{
				TCPServer.Clients.remove(this);
				try
				{
					clientSocket.close();
				}
				catch (Exception ex)
				{
				}
				Log.LogEvent(Log.SUBTYPE.TCP_CLIENT, "Client disconnected");
				Log.LogError(Log.SUBTYPE.TCP_CLIENT, "Client error: " + e.getMessage());
				if (Log.DEBUG)
					e.printStackTrace();
			}
		}

	}

}
