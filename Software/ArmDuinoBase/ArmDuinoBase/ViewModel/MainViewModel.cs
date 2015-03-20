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
	public class MainViewModel : INotifyPropertyChanged
	{
		public static MainViewModel Current { get; set; }

		public CoreWrapper CoreWrapper { get; set; }
		public TCPHandler TCPHandler { get; set; }
		public WebcamWrapper WebcamWrapper { get; set; }
		public Arm Arm { get; set; }
		public Rover Rover { get; set; }
		public KinectHandler KinectHandler { get; set; }
		public KinectGestureProcessor KinectGestureProcessor { get; set; }
		public CommandRecognizer CommandRecognizer { get; set; }
		public GamepadState GamepadState { get; set; }
		public ArmCommand CurrentCommand { get; set; }
		public ArmCommand SelectedCommand { get; set; }

		public ObservableCollection<ConsoleData> ConsoleLog { get; set; }
		public ObservableCollection<string> COMPorts { get; set; }
		public ObservableCollection<SpokenCommand> Commands { get; set; }
		public string CurrentFile { get; set; }

		private System.Timers.Timer SendTimer;
		private System.Timers.Timer CommandTimer;
		private System.Timers.Timer GamePadTimer;

		private Thread KinectInitializer { get; set; }
		private Thread VoiceControlInitializer { get; set; }


		public MainViewModel()
		{
			Current = this;
			ConsoleLog = new ObservableCollection<ConsoleData>();
			COMPorts = new ObservableCollection<string>();
			Commands = new ObservableCollection<SpokenCommand>();
			CoreWrapper = new CoreWrapper();
			TCPHandler = new TCPHandler();
			WebcamWrapper = new WebcamWrapper();
			KinectHandler = new KinectHandler();
			KinectGestureProcessor = new KinectGestureProcessor();
			CommandRecognizer = new CommandRecognizer("es-ES");
			GamepadState = new GamepadState(SlimDX.XInput.UserIndex.One);
			SendTimer = new System.Timers.Timer();
			CommandTimer = new System.Timers.Timer();
			GamePadTimer = new System.Timers.Timer();
			Arm = new Arm();
			Rover = new Rover();
			//LoadFromFile("commands.arm");
			TimeSpan SendSpan = new TimeSpan(0, 0, 0, 0, 100);
			TimeSpan CommandSpan = new TimeSpan(0, 0, 0, 0, 500);
			TimeSpan GamePadSpan = new TimeSpan(0, 0, 0, 0, 10);
			CommandTimer.Interval = CommandSpan.TotalMilliseconds;
			SendTimer.Interval = SendSpan.TotalMilliseconds;
			GamePadTimer.Interval = GamePadSpan.TotalMilliseconds;
			SendTimer.Elapsed += SendTimer_Elapsed;
			CommandTimer.Elapsed += CommandTimer_Elapsed;
			GamePadTimer.Elapsed += GamePadTimer_Elapsed;
			CommandRecognizer.CommandRecognized += CommandRecognizer_CommandRecognized;
			CommandRecognizer.ControlCommandRecognized += CommandRecognizer_ControlCommandRecognized;
		}

		public void StartGamePad()
		{
			GamePadTimer.Start();
		}

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
			Arm.Normalize();
			if (!Arm.ControlledByGamePad) GamePadTimer.Stop();
		}

		void CommandRecognizer_ControlCommandRecognized(object source, ControlCommandRecognizedEventInfo command)
		{
			switch (command.Type)
			{
				case ControlCommandRecognizedEventInfo.ControlType.GESTURE:
					{
						if (command.Activated)
						{
							Arm.SelectedTab = 1;
						}
						else
						{
							Arm.SelectedTab = 2;
						}
						break;
					}
				case ControlCommandRecognizedEventInfo.ControlType.VOICE:
					{
						if (command.Activated)
						{
							Arm.SelectedTab = 2;
						}
						else
						{
							Arm.SelectedTab = 0;
						}
						break;
					}
			}
		}

		public void SaveFile()
		{
			File.WriteAllText(Directory.GetCurrentDirectory() + "//commander//commands-temp.arm", CurrentFile);
			try
			{
				LoadFromFile("commands-temp.arm");
				File.WriteAllText(Directory.GetCurrentDirectory() + "//commander//commands.arm", CurrentFile);
				LoadFromFile("commands.arm");
			}
			catch (Exception e)
			{
				MessageBox.Show("Invalid file, please check sintax", "Error");
			}
		}

		public void LoadFromFile(string filename)
		{
			Commands.Clear();
			CurrentFile = "";
			string path = Directory.GetCurrentDirectory() + "//commander";
			string file = path + "//" + filename;
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			if (!File.Exists(file))
			{
				File.Create(file);
			}
			StreamReader configFile = new StreamReader(file);
			while (!configFile.EndOfStream)
			{
				string line = configFile.ReadLine();
				CurrentFile += line + "\r\n";
				if (line.StartsWith("COMMAND"))
				{
					char[] separator = { ';' };
					String[] title = line.Split(separator);
					string name = title[1].Replace(";", "");
					name = name.Trim();
					string response = title[2].Replace(";", "");
					response = response.Trim();

					SpokenCommand command = new SpokenCommand(name, response);
					line = configFile.ReadLine();
					CurrentFile += line + "\r\n";
					while (line.StartsWith("KEYFRAME"))
					{
						line = line.Replace(" ", "");
						String[] nums = line.Split(separator);
						int[] data = new int[15];
						for (int i = 1; i < data.Length + 1; i++)
						{
							int servoCode = Int32.Parse(nums[i]);
							data[i - 1] = servoCode;
						}
						command.MovementQueue.Enqueue(data);
						line = configFile.ReadLine();
						CurrentFile += line + "\r\n";
					}
					CommandRecognizer.LoadCommand(command);
					Commands.Add(command);
					line = configFile.ReadLine();
					CurrentFile += line + "\r\n";
				}
			}
			configFile.Close();
		}

		internal void WriteCommand(string input)
		{
			switch (input.ToUpper())
			{
				case "CONNECT":
					{
						if (CoreWrapper.IsStarted)
							CoreWrapper.Write(input);
						if (TCPHandler.Connected)
							TCPHandler.Write(input);
						Arm.Connected = true;
						//Rover.Connected = true;
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

		private void WriteMoveCommand(string active)
		{
			if (CoreWrapper.IsStarted)
				CoreWrapper.Write("MOVE " + Arm.BaseAng + " " + Arm.Horizontal1Ang + " " + Arm.Vertical1Ang + " " + Arm.Horizontal2Ang + " " + Arm.Vertical2Ang + " " + Arm.Horizontal3Ang + " " + Arm.Gripper + " " 
					+ Rover.FrontLeftAng + " " + Rover.FrontRightAng + " " + Rover.RearLeftAng + " " + Rover.RearRightAng + " " + Rover.FrontLeftSpeed + " " + Rover.FrontRightSpeed + " " + Rover.RearLeftSpeed + " " + Rover.RearRightSpeed + " " + active);
			else if (TCPHandler.Connected)
				TCPHandler.Write("MOVE " + Arm.BaseAng + " " + Arm.Horizontal1Ang + " " + Arm.Vertical1Ang + " " + Arm.Horizontal2Ang + " " + Arm.Vertical2Ang + " " + Arm.Horizontal3Ang + " " + Arm.Gripper + " "
					+ Rover.FrontLeftAng + " " + Rover.FrontRightAng + " " + Rover.RearLeftAng + " " + Rover.RearRightAng + " " + Rover.FrontLeftSpeed + " " + Rover.FrontRightSpeed + " " + Rover.RearLeftSpeed + " " + Rover.RearRightSpeed + " " + active);
		}

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

		void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			WriteCommand("MOVE");
		}

		public void Refresh()
		{
			COMPorts.Clear();
			string[] portnames = SerialPort.GetPortNames();
			foreach (string name in portnames)
			{
				COMPorts.Add(name);
			}
		}

		public void StartKinect()
		{
			if (!KinectHandler.Started)
			{
				KinectInitializer = new Thread(new ThreadStart(InitializeKinect));
				KinectInitializer.Start();
			}
		}

		private void InitializeKinect()
		{
			KinectHandler.Busy = true;
			KinectHandler.InitializeSensor();
			KinectGestureProcessor.EllipseSize = 20;
			if (KinectHandler.Sensor != null)
			{
				Application.Current.Dispatcher.Invoke(new Action(() =>
				{
					KinectHandler.Sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);
					KinectHandler.Sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);
					KinectHandler.Sensor.DepthFrameReady += Sensor_DepthFrameReady;
					KinectHandler.interactionStream = new InteractionStream(KinectHandler.Sensor, new DummyInteractionClient());
					KinectHandler.interactionStream.InteractionFrameReady += interactionStream_InteractionFrameReady;
				}));
				KinectHandler.Busy = false;
				KinectHandler.Started = true;
			}
		}

		private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
			Skeleton[] skeletons = KinectHandler.Skeletons;
			using (var skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame == null)
					return;
				if (skeletons == null || skeletons.Length != skeletonFrame.SkeletonArrayLength)
				{
					skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
				}

				skeletonFrame.CopySkeletonDataTo(skeletons);

				KinectHandler.interactionStream.ProcessSkeleton(skeletons, KinectHandler.Sensor.AccelerometerGetCurrentReading(), skeletonFrame.Timestamp);
			}

			KinectHandler.closestSkeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
												.OrderBy(s => s.Position.Z * Math.Abs(s.Position.X))
												.FirstOrDefault();

			if (KinectHandler.closestSkeleton == null)
				return;

			Joint head = KinectHandler.closestSkeleton.Joints[JointType.Head];
			Joint rightHand = KinectHandler.closestSkeleton.Joints[JointType.HandRight];
			Joint leftHand = KinectHandler.closestSkeleton.Joints[JointType.HandLeft];
			Joint leftElbow = KinectHandler.closestSkeleton.Joints[JointType.ElbowLeft];
			Joint rightElbow = KinectHandler.closestSkeleton.Joints[JointType.ElbowRight];
			Joint leftShoulder = KinectHandler.closestSkeleton.Joints[JointType.ShoulderLeft];
			Joint rightShoulder = KinectHandler.closestSkeleton.Joints[JointType.ShoulderRight];

			KinectGestureProcessor.HeadPosition = head.Position;
			KinectGestureProcessor.RightHandPosition = rightHand.Position;
			KinectGestureProcessor.LeftHandPosition = leftHand.Position;
			KinectGestureProcessor.RightElbowPosition = rightElbow.Position;
			KinectGestureProcessor.LeftElbowPosition = leftElbow.Position;
			KinectGestureProcessor.RightShoulderPosition = rightShoulder.Position;
			KinectGestureProcessor.LeftShoulderPosition = leftShoulder.Position;


			if (head.TrackingState == JointTrackingState.NotTracked ||
				rightHand.TrackingState == JointTrackingState.NotTracked ||
				leftHand.TrackingState == JointTrackingState.NotTracked)
			{
				return;
			}
			else
			{
				if (rightHand.Position.Y < head.Position.Y - 0.45)
				{
					KinectGestureProcessor.RightUp = false;
				}
				else
				{
					KinectGestureProcessor.RightUp = true;
				}

				if (leftHand.Position.Y < head.Position.Y - 0.45)
				{
					KinectGestureProcessor.LeftUp = false;
				}
				else
				{
					KinectGestureProcessor.LeftUp = true;
				}

				if (rightHand.Position.X > head.Position.X + 0.45)
				{
					KinectGestureProcessor.RightSeparated = true;
				}
				else
				{
					KinectGestureProcessor.RightSeparated = false;
				}

				if (leftHand.Position.X < head.Position.X - 0.45)
				{
					KinectGestureProcessor.LeftSeparated = true;
				}
				else
				{
					KinectGestureProcessor.LeftSeparated = false;
				}
			}
			if (Arm.ControlledByGestures)
				MapJoints();
		}

		public void MapJoints()
		{
			Skeleton currentSkeleton = KinectHandler.closestSkeleton;
			if (KinectHandler.Sensor != null && KinectHandler.Sensor.SkeletonStream.IsEnabled
	&& currentSkeleton != null)
			{
				double horizontal1angle = KinectGestureProcessor.RightJointsAngle(currentSkeleton.Joints[JointType.ShoulderLeft], currentSkeleton.Joints[JointType.ShoulderRight], currentSkeleton.Joints[JointType.ElbowRight], currentSkeleton);
				double horizontal2angle = KinectGestureProcessor.RightJointsAngle(currentSkeleton.Joints[JointType.ShoulderRight], currentSkeleton.Joints[JointType.ElbowRight], currentSkeleton.Joints[JointType.WristRight], currentSkeleton);
				double horizontal3angle = KinectGestureProcessor.RightJointsAngle(currentSkeleton.Joints[JointType.ElbowRight], currentSkeleton.Joints[JointType.WristRight], currentSkeleton.Joints[JointType.HandRight], currentSkeleton);
				Arm.Horizontal1Ang = (int)horizontal1angle;
				Arm.Horizontal2Ang = (int)horizontal2angle;
				Arm.Horizontal3Ang = (int)horizontal3angle;
				if (KinectGestureProcessor.LeftGrip) Arm.Gripper = 90;
				else Arm.Gripper = 180;
				Arm.BaseAng = KinectGestureProcessor.GetVerticalAngle(currentSkeleton);
				Arm.Vertical1Ang = KinectGestureProcessor.GetVerticalAngle(currentSkeleton);
				Arm.Vertical2Ang = KinectGestureProcessor.GetVerticalAngle(currentSkeleton);
			}
		}

		private void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
		{
			byte[] colorBytes = null;
			using (var image = e.OpenColorImageFrame())
			{
				if (image == null)
					return;
				if (colorBytes == null || colorBytes.Length != image.PixelDataLength)
				{
					colorBytes = new byte[image.PixelDataLength];
				}

				image.CopyPixelDataTo(colorBytes);

				//You could use PixelFormats.Bgr32 below to ignore the alpha,
				//or if you need to set the alpha you would loop through the bytes 
				//as in this loop below
				int length = colorBytes.Length;
				for (int i = 0; i < length; i += 4)
				{
					colorBytes[i + 3] = 255;
				}

				BitmapSource source = BitmapSource.Create(image.Width,
					image.Height,
					96,
					96,
					PixelFormats.Bgra32,
					null,
					colorBytes,
					image.Width * image.BytesPerPixel);
				KinectHandler.ImageFromKinect = source;
			}
		}

		private void interactionStream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
		{
			using (InteractionFrame frame = e.OpenInteractionFrame())
			{
				if (frame != null)
				{
					if (KinectHandler.userInfos == null)
					{
						KinectHandler.userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
					}

					frame.CopyInteractionDataTo(KinectHandler.userInfos);
				}
				else
				{
					return;
				}
			}



			foreach (UserInfo userInfo in KinectHandler.userInfos)
			{
				foreach (InteractionHandPointer handPointer in userInfo.HandPointers)
				{
					switch (handPointer.HandEventType)
					{
						case InteractionHandEventType.Grip:
							switch (handPointer.HandType)
							{
								case InteractionHandType.Left:
									KinectGestureProcessor.LeftGrip = true;
									break;
								case InteractionHandType.Right:
									KinectGestureProcessor.RightGrip = true;
									break;
							}
							break;

						case InteractionHandEventType.GripRelease:
							switch (handPointer.HandType)
							{
								case InteractionHandType.Left:
									KinectGestureProcessor.LeftGrip = false;
									break;
								case InteractionHandType.Right:
									KinectGestureProcessor.RightGrip = false;
									break;
							}
							break;
					}
				}

			}
		}

		private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
		{
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			{
				if (depthFrame == null)
					return;
				KinectHandler.interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
			}
		}

		public void StartSpeechRecognition()
		{
			VoiceControlInitializer = new Thread(new ThreadStart(InitializeSpeechRecognition));
			VoiceControlInitializer.Start();
		}

		private void InitializeSpeechRecognition()
		{
			CommandRecognizer.Busy = true;
			CommandRecognizer.InitializeSpeechRecognition();
			if (KinectHandler.Sensor != null)
			{
				var audioSource = MainViewModel.Current.KinectHandler.Sensor.AudioSource;
				audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
				var kinectStream = audioSource.Start();
				CommandRecognizer.SetInputToAudioStream(
						kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

			}
			else
			{
				try
				{
					CommandRecognizer.SetInputToDefaultAudioDevice();
				}
				catch
				{
					MessageBoxResult error = MessageBox.Show("No input device found", "Le Fail", MessageBoxButton.OK, MessageBoxImage.Error);
					if (error == MessageBoxResult.OK) return;
				}
			}
			CommandRecognizer.RecognizeAsync(RecognizeMode.Multiple);
			CommandRecognizer.Busy = false;
			CommandRecognizer.Initialized = true;
		}

		void CommandTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (CurrentCommand.MovementQueue.Count != 0)
			{
				int[] nextToSend = new int[7];
				Array.Copy(CurrentCommand.MovementQueue.Dequeue(), nextToSend, 7);
				Arm.BaseAng = nextToSend[0];
				Arm.Horizontal1Ang = nextToSend[1];
				Arm.Vertical1Ang = nextToSend[2];
				Arm.Horizontal2Ang = nextToSend[3];
				Arm.Vertical2Ang = nextToSend[4];
				Arm.Horizontal3Ang = nextToSend[5];
				Arm.Gripper = nextToSend[6];
			}

			else
			{
				CurrentCommand = null;
				CommandTimer.Stop();
			}
		}

		public void LoadAndStart(ArmCommand command)
		{
			if (command == null) return;
			if (CommandTimer.Enabled || CurrentCommand != null)
			{
				CommandTimer.Stop();
				CurrentCommand = null;
			}
			CurrentCommand = (ArmCommand)command.Clone();
			CommandTimer.Start();
		}

		void CommandRecognizer_CommandRecognized(object source, string commandName)
		{
			foreach (SpokenCommand command in Commands)
			{
				if (command.Name.Equals(commandName))
				{
					LoadAndStart(command);
					if (command.Response != null) CommandRecognizer.Synth.SpeakAsync(command.Response);
				}
			}
		}

		public void CloseCore()
		{
			if (CoreWrapper.IsStarted)
			{
				CoreWrapper.StopCore();
			}
		}

		public void CloseTCP()
		{
			if (TCPHandler.Connected)
			{
				TCPHandler.Close();
			}
		}

		public void CloseWebcam()
		{
			if (WebcamWrapper.IsStarted)
			{
				WebcamWrapper.StopWebcam();
			}
		}

		public void CloseAll()
		{
			Arm.Connected = false;
			CloseCore();
			CloseTCP();
			CloseWebcam();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
