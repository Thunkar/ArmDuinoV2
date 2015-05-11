package es.ieeesb.teora.armduino.core;

public class MessageConformer
{
	public static final char CONNECTCODE = 'C';
	public static final char SENDCODE = 'M';
	public static final char RESETCODE = 'R';
	
	
	/**
	 * Takes a message and adds the start and finish control characters
	 * @param message
	 * @return
	 */
	public static char[] finishMessage(char[] message)
	{
		char[] finalMessage = new char[message.length + 2];
		finalMessage[0] = '&';
		finalMessage[finalMessage.length - 1] = '%';
		System.arraycopy(message, 0, finalMessage, 1, message.length);
		return finalMessage;
	}
	
	
	/**
	 * This is where the magic happens. This method takes a move message as an input (MOVE field field field.... activeflags) and formats it according to the protocol, using the input arguments 
	 * FIELD_SIZE and FIELD_COUNT.
	 * @param movement data
	 * @return formatted move message
	 */
	public static char[] getMoveMessage(String data)
	{
		char[] finalMessage = new char[Main.FIELD_COUNT*(Main.FIELD_SIZE+1)+3];
		String[] stringArray = data.split(" ");
		finalMessage[0] = SENDCODE;
		finalMessage[1] = String.valueOf(Main.FIELD_COUNT).toCharArray()[0];
		finalMessage[2] = String.valueOf(Main.FIELD_SIZE).toCharArray()[0];
		String activeFlags = stringArray[stringArray.length-1];
		int dataCounter = 0;
		for(int i = 0; i < Main.FIELD_COUNT*(Main.FIELD_SIZE+1); i = i + Main.FIELD_SIZE+1)
		{
			int difference = Main.FIELD_SIZE - stringArray[dataCounter].length();
			int dataPointer = 0;
			for(int j = 0; j < Main.FIELD_SIZE+1; j++)
			{
				if(j == 0)
				{
					finalMessage[i + 3] = activeFlags.charAt(dataCounter);
					continue;
				}
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

	/**
	 * Transforms a connect request into a connect message
	 * @return the connect message according to the protocol
	 */
	public static char[] getConnectRequest()
	{
		char[] statusRequest = {CONNECTCODE};
		return finishMessage(statusRequest);
	}

	/**
	 * Transforms a reset request into a reset message
	 * @return the reset message according to the protocol
	 */
	public static char[] getResetRequest()
	{
		char[] statusRequest = {RESETCODE};
		return finishMessage(statusRequest);
	}
	
}
