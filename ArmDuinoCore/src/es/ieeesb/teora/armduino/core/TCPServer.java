package es.ieeesb.teora.armduino.core;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.ServerSocket;
import java.net.Socket;

import es.ieeesb.teora.armduino.util.Log;

public class TCPServer implements Runnable
{
	private ServerSocket socket;
	private Socket connectionSocket;
	private boolean active = true;
	private boolean connected = false;

	public boolean isActive()
	{
		return active;
	}

	public void setActive(boolean active)
	{
		this.active = active;
	}

	public TCPServer(int port)
	{
		try
		{
			socket = new ServerSocket(port);
		}
		catch (Exception e)
		{
			Log.LogError(Log.SUBTYPE.TCP_SERVER, "Error initializing server: " + e.getMessage());
		}
	}
	
	public void write(String data)
	{
		try
		{
			DataOutputStream outToClient = new DataOutputStream(
					connectionSocket.getOutputStream());
			outToClient.writeBytes(data);
		}
		
		catch (IOException e)
		{
			Log.LogError(Log.SUBTYPE.TCP_SERVER, "Server error: " + e.getMessage());
			if (Log.DEBUG)
				e.printStackTrace();
		}
	}

	public void close()
	{
		try
		{
			socket.close();
			Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "TCP server shutting down");
		}
		catch (IOException e)
		{
			Log.LogError(Log.SUBTYPE.TCP_SERVER, "Server error: " + e.getMessage());
		}
	}

	@Override
	public void run()
	{
		Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "TCP server starting");
		while (active)
		{
			while(!connected)
			{
				try
				{
					connectionSocket = socket.accept();
					connected = true;
					Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "Client connected");
				}
				catch (IOException e)
				{
					Log.LogError(Log.SUBTYPE.TCP_SERVER, "Connection error: " + e.getMessage());
					if (Log.DEBUG)
						e.printStackTrace();
				}
			}
			while (connected)
			{
				try
				{
					BufferedReader inFromClient = new BufferedReader(new InputStreamReader(
							connectionSocket.getInputStream()));
					String[] data = inFromClient.readLine().split(" ");
					String extras = null;
					if (data.length > 1)
					{
						StringBuilder strBuilder = new StringBuilder();
						for (int i = 1; i < Main.FIELD_COUNT + 1; i++)
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
						connected = false;
						connectionSocket.close();
						Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "Client disconnected");
					}
					Main.commandDecoder.decode(CommandDecoder.COMMAND_TYPE.fromString(data[0]),
							extras);
				}
				catch (Exception e)
				{
					connected = false;
					Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "Client disconnected");
					Log.LogError(Log.SUBTYPE.TCP_SERVER, "Server error: " + e.getMessage());
					if (Log.DEBUG)
						e.printStackTrace();
				}
			}
		}
	}

	public boolean isConnected()
	{
		return connected;
	}

	public void setConnected(boolean connected)
	{
		this.connected = connected;
	}
}
