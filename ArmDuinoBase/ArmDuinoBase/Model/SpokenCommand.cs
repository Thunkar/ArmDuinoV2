using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArmDuinoBase.Model
{
    public class SpokenCommand : ArmCommand
    {
        public String Response { get; set; }
        public String  Name {get; set;}

        public SpokenCommand() { }

        public SpokenCommand(string Name, string Response)
        {
            this.Response = Response;
            this.Name = Name;
        }

    }
}
