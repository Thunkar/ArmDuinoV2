using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
    public class Arm : INotifyPropertyChanged
    {
        private int baseang;
        public int BaseAng
        {
            get
            {
                return baseang;
            }
            set
            {
                baseang = value;
                NotifyPropertyChanged("BaseAng");
            }
        }
        private int horizontal1ang;
        public int Horizontal1Ang
        {
            get
            {
                return horizontal1ang;
            }
            set
            {
                horizontal1ang = value;
                NotifyPropertyChanged("Horizontal1Ang");
            }
        }
        private int vertical1ang;
        public int Vertical1Ang
        {
            get
            {
                return vertical1ang;
            }
            set
            {
                vertical1ang = value;
                NotifyPropertyChanged("Vertical1Ang");
            }
        }
        private int horizontal2ang;
        public int Horizontal2Ang
        {
            get
            {
                return horizontal2ang;
            }
            set
            {
                horizontal2ang = value;
                NotifyPropertyChanged("Horizontal2Ang");
            }
        }
        private int vertical2ang;
        public int Vertical2Ang
        {
            get
            {
                return vertical2ang;
            }
            set
            {
                vertical2ang = value;
                NotifyPropertyChanged("Vertical2Ang");
            }
        }
        private int horizontal3ang;
        public int Horizontal3Ang
        {
            get
            {
                return horizontal3ang;
            }
            set
            {
                horizontal3ang = value;
                NotifyPropertyChanged("Horizontal3Ang");
            }
        }
        private int gripper;
        public int Gripper
        {
            get
            {
                return gripper;
            }
            set
            {
                gripper = value;
                NotifyPropertyChanged("Pinza");
            }
        }

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

        private bool controlledByGestures;
        public bool ControlledByGestures
        {
            get
            {
                return controlledByGestures;
            }
            set
            {
                controlledByGestures = value;
                NotifyPropertyChanged("ControlledByGestures");
            }
        }

        private bool controlledBySliders;
        public bool ControlledBySliders
        {
            get
            {
                return controlledBySliders;
            }
            set
            {
                controlledBySliders = value;
                NotifyPropertyChanged("ControlledBySliders");
            }
        }

        private bool controlledByVoice;
        public bool ControlledByVoice
        {
            get
            {
                return controlledByVoice;
            }
            set
            {
                controlledByVoice = value;
                NotifyPropertyChanged("ControlledByVoice");
            }
        }

        private bool controlledByCommander;
        public bool ControlledByCommander
        {
            get
            {
                return controlledByCommander;
            }
            set
            {
                controlledByCommander = value;
                NotifyPropertyChanged("ControlledByCommander");
            }
        }

        private int selectedTab;
        public int SelectedTab
        {
            get
            {
                return selectedTab;
            }
            set
            {
                selectedTab = value;
                NotifyPropertyChanged("SelectedTab");
            }
        }


        public Arm()
        {
            BaseAng = 90;
            Horizontal1Ang = 90;
            Vertical1Ang = 90;
            Horizontal2Ang = 90;
            Vertical2Ang = 90;
            Horizontal3Ang = 90;
            Gripper = 170;
        }

        public void Reset()
        {
            BaseAng = 90;
            Horizontal1Ang = 90;
            Vertical1Ang = 90;
            Horizontal2Ang = 90;
            Vertical2Ang = 90;
            Horizontal3Ang = 90;
            Gripper = 170;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
