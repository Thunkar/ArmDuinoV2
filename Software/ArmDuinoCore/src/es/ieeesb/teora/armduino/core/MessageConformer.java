package es.ieeesb.teora.armduino.core;

public class MessageConformer
{

	public static final char STATUSCODE = 'S';  
	public static final char CONNECTCODE = 'C';
	public static final char SENDCODE = 'M';
	public static final char RESETCODE = 'R';
	
	public static char[] getStatusRequest()
	{
		char[] statusRequest = {STATUSCODE};
		return finishMessage(statusRequest);
	}

	
	public static char[] finishMessage(char[] message)
	{
		char[] finalMessage = new char[message.length + 2];
		finalMessage[0] = '&';
		finalMessage[finalMessage.length - 1] = '%';
		System.arraycopy(message, 0, finalMessage, 1, message.length);
		return finalMessage;
	}
	
	
	public static char[] getMoveMessage(String data)
	{
		char[] finalMessage = new char[Main.FIELD_COUNT*Main.FIELD_SIZE+3];
		String[] stringArray = data.split(" ");
		finalMessage[0] = SENDCODE;
		finalMessage[1] = String.valueOf(Main.FIELD_COUNT).toCharArray()[0];
		finalMessage[2] = String.valueOf(Main.FIELD_SIZE).toCharArray()[0];
		int dataCounter = 0;
		for(int i = 0; i < Main.FIELD_COUNT*Main.FIELD_SIZE; i = i + Main.FIELD_SIZE)
		{
			int difference = Main.FIELD_SIZE - stringArray[dataCounter].length();
			int dataPointer = 0;
			for(int j = 0; j < Main.FIELD_SIZE; j++)
			{
				if(difference != 0)
				{
					finalMessage[i + 3 + j] = '0';
					difference--;
				}
				else
				{
					finalMessage[i + 3 + j] = stringArray[dataCounter].toCharArray()[dataPointer];
					dataPointer++;
				}
			}
			dataCounter++;
		}
		return finishMessage(finalMessage);
	}


	public static char[] getConnectRequest()
	{
		char[] statusRequest = {CONNECTCODE};
		return finishMessage(statusRequest);
	}


	public static char[] getResetRequest()
	{
		char[] statusRequest = {RESETCODE};
		return finishMessage(statusRequest);
	}
	
}
