using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmDuinoBase.Model
{
	/// <summary>
	/// Data model for the rover.
	/// </summary>
	public class Rover : INotifyPropertyChanged
	{
		/// <summary>
		/// Rover variables declaration
		/// </summary>
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

		/// <summary>
		/// Constructor
		/// </summary>
		public Rover()
		{
			Reset();
		}

		/// <summary>
		/// Sets the variables to the default positions
		/// </summary>
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

		public void Normalize()
		{
			if (FrontLeftAng > 180) FrontLeftAng = 180;
			if (FrontLeftAng < 0) FrontLeftAng = 0;
			if (FrontRightAng > 180) FrontRightAng = 180;
			if (FrontRightAng < 0) FrontRightAng = 0;
			if (RearLeftAng > 180) RearLeftAng = 180;
			if (RearLeftAng < 0) RearLeftAng = 0;
			if (RearRightAng > 180) RearRightAng = 180;
			if (RearRightAng < 0) RearRightAng = 0;
			if (FrontLeftSpeed > 500) FrontLeftSpeed = 500;
			if (FrontLeftSpeed < 0) FrontLeftSpeed = 0;
			if (FrontRightSpeed > 500) FrontRightSpeed = 500;
			if (FrontRightSpeed < 0) FrontRightSpeed = 0;
			if (RearLeftSpeed > 500) RearLeftSpeed = 500;
			if (RearLeftSpeed < 0) RearLeftSpeed = 0;
			if (RearRightSpeed > 500) RearRightSpeed = 500;
			if (RearRightSpeed < 0) RearRightSpeed = 0;
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
