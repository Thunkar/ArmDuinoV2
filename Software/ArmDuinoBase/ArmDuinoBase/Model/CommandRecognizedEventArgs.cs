using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public delegate void CommandRecognizedEventHandler(object source, string command);


    public class CommandRecognizedEventArgs : EventArgs
    {
        private string EventInfo;

        public CommandRecognizedEventArgs(string command)
        {
            EventInfo = command;
        }
        public string GetInfo()
        {
            return EventInfo;
        }
    }
}
