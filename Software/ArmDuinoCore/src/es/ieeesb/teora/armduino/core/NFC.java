package es.ieeesb.teora.armduino.core;

import java.io.File;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Reader;

import es.ieeesb.teora.armduino.util.Log;

/**
 * @author Gregorio Handler that controls NFC authentication. Work in progress
 */

public class NFC implements Runnable
{
	private String baseDir;
	private boolean active;

	/**
	 * Constructor. Looks for Smartcard readers.
	 */
	public NFC()
	{
		Log.LogEvent(Log.SUBTYPE.NFC, "Initializing NFC subsystem");
		active = true;
		File currentDir = null;
		try
		{
			currentDir = new File("./").getCanonicalFile();
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
		baseDir = currentDir.getAbsolutePath();
	}

	/*
	 * Continuosly tries to read the smartcard reader and opens the door if it
	 * should. Also checks users's Latch.
	 */
	public void run()
	{
		while (active)
		{
			try
			{
				Log.LogEvent(Log.SUBTYPE.NFC, readNFC());
				try
				{
					Thread.sleep(100);
				}
				catch (InterruptedException e)
				{
					Log.LogError(Log.SUBTYPE.NFC,
							"NFC error: " + e.getMessage());
				}

			}
			catch (Exception e)
			{

			}

		}
	}

	/**
	 * @return
	 * @throws Exception
	 */
	public String readNFC()
	{
		try
		{
			ProcessBuilder pb = new ProcessBuilder("python", baseDir + "/nfcpy/reader.py");
			pb.redirectErrorStream(true);
			Process proc;
			proc = pb.start();
			Reader reader = new InputStreamReader(proc.getInputStream());
			int ch;
			StringBuilder strBuilder = new StringBuilder();
			while ((ch = reader.read()) != -1)
			{
				strBuilder.append((char) ch);
			}
			String result = strBuilder.toString().trim();
			reader.close();
			return result;
		}
		catch (Exception e)
		{
			Log.LogError(Log.SUBTYPE.NFC, "Error reading NFC: " + e.getMessage());
			return null;
		}
	}

	public boolean isActive()
	{
		return active;
	}

	public void setActive(boolean active)
	{
		this.active = active;
	}

}
