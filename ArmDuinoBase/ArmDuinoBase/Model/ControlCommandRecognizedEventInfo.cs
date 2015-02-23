using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public class ControlCommandRecognizedEventInfo
    {
        public enum ControlType { VOICE, GESTURE, SLIDER, COMMANDER };

        public bool Activated { get; set; }
        public ControlType Type { get; set; }

        public ControlCommandRecognizedEventInfo(ControlType Type, bool Activated)
        {
            this.Activated = Activated;
            this.Type = Type;
        }
    }
}
