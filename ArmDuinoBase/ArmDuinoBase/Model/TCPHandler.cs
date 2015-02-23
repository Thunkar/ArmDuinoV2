using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public class TCPHandler : INotifyPropertyChanged
    {
        public TcpClient Client;
        public Stream Stream;
        public StreamReader Reader; 
        public Thread ReaderThread;

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

        public TCPHandler()
        {
        }


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

        public void Close()
        {
            Write("STOP");
            Connected = false;
            ReaderThread.Join();
            Stream.Flush();
            Stream.Close();
            Client.Close();
        }

        public void ReadSocket()
        {
            while (Connected)
            {
                if (Client != null && Client.Available > 0)
                {
                    incomingData(this, Reader.ReadLine());
                }
            }
        }

        public byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public void Write(string input)
        {
            StreamWriter writer = new StreamWriter(Stream);
            writer.WriteLine(input);
            writer.Flush();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

    }
}
