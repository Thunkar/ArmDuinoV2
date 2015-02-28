using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public delegate void ControlCommandRecognizedEventHandler(object source, ControlCommandRecognizedEventInfo command);


    public class ControlCommandRecognizedEventArgs : EventArgs
    {
        private ControlCommandRecognizedEventInfo EventInfo;

        public ControlCommandRecognizedEventArgs(ControlCommandRecognizedEventInfo command)
        {
            EventInfo = command;
        }
        public ControlCommandRecognizedEventInfo GetInfo()
        {
            return EventInfo;
        }
    }
}
