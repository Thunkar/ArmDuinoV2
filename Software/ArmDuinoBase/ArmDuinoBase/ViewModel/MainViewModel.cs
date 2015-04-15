using ArmDuinoBase.Model;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArmDuinoBase.ViewModel
{
	/// <summary>
	/// MainViewModel, handles the communication between the view and the model. Fires property changed events and such.
	/// </summary>
	public class MainViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Singleton initialization
		/// </summary>
		public static MainViewModel Current { get; set; }
		/// <summary>
		/// Model variables initialization
		/// </summary>
		public CoreWrapper CoreWrapper { get; set; }
		public TCPHandler TCPHandler { get; set; }
		public WebcamWrapper WebcamWrapper { get; set; }
		public Arm Arm { get; set; }
		public Rover Rover { get; set; }
		public Sensors Sensors { get; set; }
		public GamepadState GamepadState { get; set; }

		/// <summary>
		/// Console collection. Receives all the incoming messages to show.
		/// </summary>
		public ObservableCollection<ConsoleData> ConsoleLog { get; set; }
		/// <summary>
		/// COM port collection. Synchronized with system COM ports.
		/// </summary>
		public ObservableCollection<string> COMPorts { get; set; }

		/// <summary>
		/// Timer that communicates with the robot by periodically sending messages
		/// </summary>
		private System.Timers.Timer SendTimer;
		/// <summary>
		/// Timer that polls the gamepad.
		/// </summary>
		private System.Timers.Timer GamePadTimer;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainViewModel()
		{
			Current = this;
			ConsoleLog = new ObservableCollection<ConsoleData>();
			COMPorts = new ObservableCollection<string>();
			CoreWrapper = new CoreWrapper();
			TCPHandler = new TCPHandler();
			WebcamWrapper = new WebcamWrapper();
			GamepadState = new GamepadState(SlimDX.XInput.UserIndex.One);
			SendTimer = new System.Timers.Timer();
			GamePadTimer = new System.Timers.Timer();
			Arm = new Arm();
			Rover = new Rover();
			Sensors = new Sensors();
			//LoadFromFile("commands.arm");
			TimeSpan SendSpan = new TimeSpan(0, 0, 0, 0, 100);
			TimeSpan CommandSpan = new TimeSpan(0, 0, 0, 0, 500);
			TimeSpan GamePadSpan = new TimeSpan(0, 0, 0, 0, 10);
			SendTimer.Interval = SendSpan.TotalMilliseconds;
			GamePadTimer.Interval = GamePadSpan.TotalMilliseconds;
			SendTimer.Elapsed += SendTimer_Elapsed;
			GamePadTimer.Elapsed += GamePadTimer_Elapsed;
		}

		/// <summary>
		/// Wrapper for gamepad initialization
		/// </summary>
		public void StartGamePad()
		{
			GamePadTimer.Start();
		}

		/// <summary>
		/// GamePad polling timer event handler. Transforms the input into usable information for the arm control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GamePadTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			GamepadState.Update();
			Arm.BaseAng += (int)(5*GamepadState.LeftStick.Position.X);
			Arm.Horizontal1Ang += (int)(5*GamepadState.LeftStick.Position.Y);
			Arm.Vertical1Ang += (int)(5 * GamepadState.RightStick.Position.X);
			Arm.Horizontal2Ang += (int)(5 * GamepadState.RightStick.Position.Y);
			if (GamepadState.LeftTrigger >= 0.5)
			{
				Arm.Gripper = 110;
			}
			if (GamepadState.RightTrigger >= 0.5)
			{
				Arm.Gripper = 170;
			}
			if (GamepadState.DPad.Up)
			{
				Arm.Horizontal3Ang += 3;
			}
			if (GamepadState.DPad.Down)
			{
				Arm.Horizontal3Ang -= 3;
			}
			if (GamepadState.Y)
			{
				Rover.FrontLeftSpeed++;
				Rover.FrontRightSpeed ++;
				Rover.RearLeftSpeed ++;
				Rover.RearRightSpeed ++;
			}
			else if (GamepadState.A)
			{
				Rover.FrontLeftSpeed --;
				Rover.FrontRightSpeed --;
				Rover.RearLeftSpeed --;
				Rover.RearRightSpeed --;
			}
			if (GamepadState.X)
			{
				Rover.FrontLeftSpeed = 0;
				Rover.RearLeftSpeed = 0;
				Rover.FrontRightSpeed = 500;
				Rover.RearRightSpeed = 500;
			}
			else if (GamepadState.B)
			{
				Rover.FrontLeftSpeed = 500;
				Rover.RearLeftSpeed = 500;
				Rover.FrontRightSpeed = 0;
				Rover.RearRightSpeed = 0;
			}
			if (GamepadState.Start)
			{
				Rover.FrontLeftSpeed = 255;
				Rover.RearLeftSpeed = 255;
				Rover.FrontRightSpeed = 255;
				Rover.RearRightSpeed = 255;
				Rover.FrontLeftAng = 90;
				Rover.FrontRightAng = 90;
				Rover.RearLeftAng = 90;
				Rover.RearRightAng = 90;
			}
			if (GamepadState.LeftShoulder)
			{
				Rover.FrontLeftAng--;
				Rover.FrontRightAng--;
				Rover.RearLeftAng--;
				Rover.RearRightAng--;
			}
			else if (GamepadState.RightShoulder)
			{
				Rover.FrontLeftAng++;
				Rover.FrontRightAng++;
				Rover.RearLeftAng++;
				Rover.RearRightAng++;
			}
			Rover.Normalize();
			Arm.Normalize();
			if (!Arm.ControlledByGamePad) GamePadTimer.Stop();
		}

		/// <summary>
		/// Formats input commands and delivers them to the active connection (COM or TCP)
		/// </summary>
		/// <param name="input"></param>
		public void WriteCommand(string input)
		{
			switch (input.ToUpper())
			{
				case "CONNECT":
					{
						if (CoreWrapper.IsStarted)
							CoreWrapper.Write(input);
						if (TCPHandler.Connected)
							TCPHandler.Write(input);
						SendTimer.Start();
						break;
					}
				case "RESET":
					{
						SendTimer.Stop();
						if (CoreWrapper.IsStarted)
							CoreWrapper.Write(input);
						if (TCPHandler.Connected)
							TCPHandler.Write(input);
						Arm.Connected = false;
						Rover.Connected = false;
						Rover.Reset();
						Arm.Reset();
						break;
					}
				case "STOP":
					{
						SendTimer.Stop();
						if (CoreWrapper.IsStarted)
						{
							CoreWrapper.StopCore();
						}
						if (TCPHandler.Connected)
						{
							TCPHandler.Close();
						}
						Arm.Connected = false;
						Rover.Connected = false;
						Rover.Reset();
						Arm.Reset();
						break;
					}
				case "MOVE":
					{
						if (Arm.Connected && Rover.Connected)
							WriteMoveCommand("111111111111111");
						else if (Arm.Connected)
							WriteMoveCommand("111111100000000");
						else if (Rover.Connected)
							WriteMoveCommand("000000011111111");
						break;
					}
				default:
					{
						if (CoreWrapper.IsStarted)
							CoreWrapper.Write(input);
						if (TCPHandler.Connected)
							TCPHandler.Write(input);
						break;
					}

			}
		}

		/// <summary>
		/// Formats movement commands to match the communcation protocol
		/// </summary>
		/// <param name="active"></param>
		private void WriteMoveCommand(string active)
		{
			if (CoreWrapper.IsStarted)
				CoreWrapper.Write("MOVE " + Arm.BaseAng + " " + Arm.Horizontal1Ang + " " + Arm.Vertical1Ang + " " + Arm.Horizontal2Ang + " " + Arm.Vertical2Ang + " " + Arm.Horizontal3Ang + " " + Arm.Gripper + " " 
					+ Rover.FrontLeftAng + " " + Rover.FrontRightAng + " " + Rover.RearLeftAng + " " + Rover.RearRightAng + " " + Rover.FrontLeftSpeed + " " + Rover.FrontRightSpeed + " " + Rover.RearLeftSpeed + " " + Rover.RearRightSpeed + " " + active);
			else if (TCPHandler.Connected)
				TCPHandler.Write("MOVE " + Arm.BaseAng + " " + Arm.Horizontal1Ang + " " + Arm.Vertical1Ang + " " + Arm.Horizontal2Ang + " " + Arm.Vertical2Ang + " " + Arm.Horizontal3Ang + " " + Arm.Gripper + " "
					+ Rover.FrontLeftAng + " " + Rover.FrontRightAng + " " + Rover.RearLeftAng + " " + Rover.RearRightAng + " " + Rover.FrontLeftSpeed + " " + Rover.FrontRightSpeed + " " + Rover.RearLeftSpeed + " " + Rover.RearRightSpeed + " " + active);
		}

		/// <summary>
		/// Starts a connection with the robot via COM port.
		/// </summary>
		public void StartCOMConnection()
		{
			try
			{
				CoreWrapper.InitializeCore(115200, 15, 3, false, 6789, false);
				CoreWrapper.StartCore();
				WriteCommand("CONNECT");
				SendTimer.Start();
			}
			catch (Exception e)
			{
				MessageBox.Show("Couldn't connect to COM Port: " + e.Message, "Error");
			}
		}

		/// <summary>
		/// Opens cambozola software with the proper arguments in order to receive the video feed from the robot
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		public void StartVideoConnection(string ip, int port)
		{
			try
			{
				WebcamWrapper.InitializeFeed(ip, port.ToString());
				WebcamWrapper.StartCambozola();
			}
			catch (Exception e)
			{
				MessageBox.Show("Couldn't connect to COM Port: " + e.Message, "Error");
			}
		}

		/// <summary>
		/// Starts a connection with the robot via TCP socket.
		/// </summary>
		public async void StartTCPConnection(string ip, int port)
		{
			try
			{
				await TCPHandler.Connect(ip, port);
				WriteCommand("CONNECT");
				SendTimer.Start();
			}
			catch (Exception e)
			{
				MessageBox.Show("Couldn't connect to TCP server: " + e.Message, "Error");
			}
		}
	

		/// <summary>
		/// Send timer event handler. Sends data to the robot periodically.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			WriteCommand("MOVE");
			WriteCommand("READSENSORS");
		}

		/// <summary>
		/// Refreshes the COM port collection in order to match the system's
		/// </summary>
		public void Refresh()
		{
			COMPorts.Clear();
			string[] portnames = SerialPort.GetPortNames();
			foreach (string name in portnames)
			{
				COMPorts.Add(name);
			}
		}


		/// <summary>
		/// Gracefully closes the CLI in order to stop the COM port communication
		/// </summary>
		public void CloseCore()
		{
			if (CoreWrapper.IsStarted)
			{
				CoreWrapper.StopCore();
			}
		}

		/// <summary>
		/// Gracefully closes the TCP connection
		/// </summary>
		public void CloseTCP()
		{
			if (TCPHandler.Connected)
			{
				TCPHandler.Close();
			}
		}

		/// <summary>
		/// Gracefully ends the cambozola process, ending the video feed
		/// </summary>
		public void CloseWebcam()
		{
			if (WebcamWrapper.IsStarted)
			{
				WebcamWrapper.StopWebcam();
			}
		}

		/// <summary>
		/// Closes everything. Used before app shutdown
		/// </summary>
		public void CloseAll()
		{
			Arm.Connected = false;
			CloseCore();
			CloseTCP();
			CloseWebcam();
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
