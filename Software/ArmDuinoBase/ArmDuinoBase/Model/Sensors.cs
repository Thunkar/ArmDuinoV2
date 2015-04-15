using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
	public class Sensors : INotifyPropertyChanged
	{
		private double battery1Level;
		public double Battery1Level
		{
			get
			{
				return battery1Level;
			}
			set
			{
				battery1Level = value;
				NotifyPropertyChanged("Battery1Level");
			}
		}

		private double battery2Level;

		public double Battery2Level
		{
			get
			{
				return battery2Level;
			}
			set
			{
				battery2Level = value;
				NotifyPropertyChanged("Battery2Level");
			}
		}

		public void ParseSensorLine(string sensorLine)
		{
			string[] splitted = sensorLine.Split(' ');
			string type = splitted[1].Substring(splitted[1].LastIndexOf("["));
            if (!type.Equals("[SENSORS]")) return;
			Battery1Level = (Double.Parse(splitted[2])/1024)*10;
			Battery2Level = (Double.Parse(splitted[3])/1024)*10;
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
