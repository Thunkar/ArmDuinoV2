using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public delegate void TCPIncomingDataEventHandler(object source, string incomingData);

	/// <summary>
	/// Helper class used for TCP data incoming event handling
	/// </summary>
    public class TCPIncomingDataEventArgs : EventArgs
    {
        private string EventInfo;

        public TCPIncomingDataEventArgs(string incomingData)
        {
            EventInfo = incomingData;
        }
        public string GetInfo()
        {
            return EventInfo;
        }
    }
}
