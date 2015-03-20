using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
	public class Rover : INotifyPropertyChanged
	{
		private int frontLeftAng;
		public int FrontLeftAng
		{
			get
			{
				return frontLeftAng;
			}
			set
			{
				frontLeftAng = value;
				NotifyPropertyChanged("FrontLeftAng");
			}
		}
		private int frontRightAng;
		public int FrontRightAng
		{
			get
			{
				return frontRightAng;
			}
			set
			{
				frontRightAng = value;
				NotifyPropertyChanged("FrontRightAng");
			}
		}
		private int rearLeftAng;
		public int RearLeftAng
		{
			get
			{
				return rearLeftAng;
			}
			set
			{
				rearLeftAng = value;
				NotifyPropertyChanged("RearLeftAng");
			}
		}
		private int rearRightAng;
		public int RearRightAng
		{
			get
			{
				return rearRightAng;
			}
			set
			{
				rearRightAng = value;
				NotifyPropertyChanged("RearRightAng");
			}
		}
		private int frontLeftSpeed;
		public int FrontLeftSpeed
		{
			get
			{
				return frontLeftSpeed;
			}
			set
			{
				frontLeftSpeed = value;
				NotifyPropertyChanged("FrontLeftSpeed");
			}
		}
		private int frontRightSpeed;
		public int FrontRightSpeed
		{
			get
			{
				return frontRightSpeed;
			}
			set
			{
				frontRightSpeed = value;
				NotifyPropertyChanged("FrontRightSpeed");
			}
		}
		private int rearLeftSpeed;
		public int RearLeftSpeed
		{
			get
			{
				return rearLeftSpeed;
			}
			set
			{
				rearLeftSpeed = value;
				NotifyPropertyChanged("RearLeftSpeed");
			}
		}
		private int rearRightSpeed;
		public int RearRightSpeed
		{
			get
			{
				return rearRightSpeed;
			}
			set
			{
				rearRightSpeed = value;
				NotifyPropertyChanged("RearRightSpeed");
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


		public Rover()
		{
			FrontLeftAng = 90;
			FrontRightAng = 90;
			RearLeftAng = 90;
			RearRightAng = 90;

			FrontLeftSpeed = 255;
			FrontRightSpeed = 255;
			RearLeftSpeed = 255;
			RearRightSpeed = 255;
		}

		public void Reset()
		{
			FrontLeftAng = 90;
			FrontRightAng = 90;
			RearLeftAng = 90;
			RearRightAng = 90;

			FrontLeftSpeed = 255;
			FrontRightSpeed = 255;
			RearLeftSpeed = 255;
			RearRightSpeed = 255;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
