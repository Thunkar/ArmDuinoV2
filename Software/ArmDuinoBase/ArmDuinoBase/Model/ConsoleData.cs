using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
	/// <summary>
	/// Incoming data model
	/// </summary>
    public class ConsoleData
    {
        public string Data { get; set; }
        public ConsoleData(string data)
        {
            this.Data = data;
        }
    }
}
