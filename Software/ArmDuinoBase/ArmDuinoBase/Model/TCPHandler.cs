using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ArmDuinoBase.Model
{

	/// <summary>
	/// TCP client implementation. Connects directly to the CLI running remotely
	/// </summary>
	public class TCPHandler : INotifyPropertyChanged
	{
		/// Main client
		public TcpClient Client;
		/// Client network stream
		public Stream Stream;
		/// Reads the incoming stream
		public StreamReader Reader;
		/// Flushes the incoing stream asynchronously
		public Thread ReaderThread;

		/// Returns wether the client is connected or not
		private bool connected;
		public bool Connected
		{
			get
			{
				return connected;
			}
			set
			{
				connected = value;
				NotifyPropertyChanged("Connected");
			}
		}

		/// Events fired on data received
		private event TCPIncomingDataEventHandler incomingData;
		private object incomingDataEventLock = new object();
		public event TCPIncomingDataEventHandler IncomingData
		{
			add
			{
				lock (incomingDataEventLock)
				{
					incomingData -= value;
					incomingData += value;
				}
			}
			remove
			{
				lock (incomingDataEventLock)
				{
					incomingData -= value;
				}
			}
		}

		/// <summary>
		/// Connects to the server provided an ip and a port. Starts reading data immediatelly.
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <returns></returns>
		public async Task Connect(string ip, int port)
		{
			Client = new TcpClient();
			await Client.ConnectAsync(ip, port);
			Stream = Client.GetStream();
			Reader = new StreamReader(Stream);
			Connected = true;
			ReaderThread = new Thread(new ThreadStart(ReadSocket));
			ReaderThread.Start();
		}

		/// <summary>
		/// Gracefully closes the connection
		/// </summary>
		public void Close()
		{
			Write("RESET");
			Connected = false;
			ReaderThread.Join();
			Stream.Flush();
			Stream.Close();
			Client.Close();
		}

		/// <summary>
		/// Blocking method that reads the incoming data and fires event every time that happens. To be used by the reader thread.
		/// </summary>
		public void ReadSocket()
		{
			while (Connected)
			{
				if (Client != null && Client.Available > 0)
				{
					try
					{
						incomingData(this, Reader.ReadLine());
					}
					catch (Exception e)
					{
						Close();
					}
				}
			}
		}

		/// <summary>
		/// Helper method used to convert strings into bytes using the default encoding (UTF8)
		/// </summary>
		/// <param name="str"></param>
		/// <returns>The byte array representing the string</returns>
		public byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		/// <summary>
		/// Writes data to the socket.
		/// </summary>
		/// <param name="input"></param>
		public void Write(string input)
		{
			StreamWriter writer = new StreamWriter(Stream);
			try
			{
				writer.WriteLine(input);
				writer.Flush();
			}
			catch (Exception e)
			{
				incomingData(this, e.Message);
				Close();
			}
		}

		/// <summary>
		/// INotifyPropertyChanged implementation for the MVVM pattern
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

	}
}
