package es.ieeesb.teora.armduino.core;

import java.io.DataOutputStream;
import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.LinkedBlockingQueue;

import es.ieeesb.teora.armduino.util.Log;

public class TCPServer implements Runnable
{
	private ServerSocket socket;
	private boolean active = true;

	public static LinkedBlockingQueue<Socket> Clients = new LinkedBlockingQueue<Socket>();

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

	public static void write(String data)
	{
		for (Socket client : Clients)
		{
			if (client.isClosed())
			{
				Clients.remove(client);
				continue;
			}
			try
			{
				DataOutputStream outToClient = new DataOutputStream(client.getOutputStream());
				outToClient.writeBytes(data);
			}
			catch (IOException e)
			{
				Clients.remove(client);
				Log.LogError(Log.SUBTYPE.TCP_SERVER, "Server writing error: " + e.getMessage());
				if (Log.DEBUG)
					e.printStackTrace();
			}
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
			Log.LogError(Log.SUBTYPE.TCP_SERVER, "Server closing error: " + e.getMessage());
		}
	}

	@Override
	public void run()
	{
		Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "TCP server starting");
		while (active)
		{
			Socket clientSocket = null;
			try
			{
				clientSocket = socket.accept();
				Clients.add(clientSocket);
				new Thread(new TCPClientWorker(clientSocket)).start();
				Log.LogEvent(Log.SUBTYPE.TCP_SERVER, "Client connected");
			}
			catch (IOException e)
			{
				Log.LogWarning(Log.SUBTYPE.TCP_SERVER, "Connection error: " + e.getMessage());
				if (Log.DEBUG)
					e.printStackTrace();
			}
		}
	}
}
