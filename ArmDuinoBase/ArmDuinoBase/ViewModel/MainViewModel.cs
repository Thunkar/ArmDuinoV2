using ArmDuinoBase.Model;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArmDuinoBase.ViewModel
{
    public class MainViewModel
    {
        public static MainViewModel Current { get; set; }

        public CoreWrapper CoreWrapper { get; set; }
        public TCPHandler TCPHandler { get; set; }
        public Arm Arm { get; set; }
        public KinectHandler KinectHandler { get; set; }
        public KinectGestureProcessor KinectGestureProcessor { get; set; }

        public ObservableCollection<ConsoleData> ConsoleLog { get; set; }
        public ObservableCollection<string> COMPorts { get; set; }

        private System.Timers.Timer SendTimer;
        private Thread KinectInitializer { get; set; }

        public MainViewModel()
        {
            Current = this; 
            ConsoleLog = new ObservableCollection<ConsoleData>();
            COMPorts = new ObservableCollection<string>();
            CoreWrapper = new CoreWrapper();
            TCPHandler = new TCPHandler();
            KinectHandler = new KinectHandler();
            KinectGestureProcessor = new KinectGestureProcessor();
            SendTimer = new System.Timers.Timer();
            Arm = new Arm();
            TimeSpan SendSpan = new TimeSpan(0, 0, 0, 0, 100);
            SendTimer.Interval = SendSpan.TotalMilliseconds;
            SendTimer.Elapsed += SendTimer_Elapsed;
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
                        Arm.Reset();
                        break;
                    }
                case "STOP":
                    {
                        SendTimer.Stop();
                        if (CoreWrapper.IsStarted)
                        {
                            CoreWrapper.Write(input);
                            CoreWrapper.StopCore();
                        }
                        if (TCPHandler.Connected)
                        {
                            TCPHandler.Write(input);
                            TCPHandler.Close();
                        }
                        Arm.Connected = false;
                        Arm.Reset();
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

        public void StartCOMConnection()
        {
            CoreWrapper.InitializeCore(115200, 7, 3, false, 6789, false);
            CoreWrapper.StartCore();
            WriteCommand("CONNECT");
            Arm.Connected = true;
            SendTimer.Start();
        }

        public async void StartTCPConnection(string ip, int port)
        {
            try
            {
                await TCPHandler.Connect(ip, port);
                if (TCPHandler.Connected)
                    TCPHandler.Write("CONNECT");
                Arm.Connected = true;
                SendTimer.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't connect to TCP server: " + e.Message, "Error");
            }
        }

        void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Arm.Connected && CoreWrapper.IsStarted)
                CoreWrapper.Write("MOVE " + Arm.BaseAng + " " + Arm.Horizontal1Ang + " " + Arm.Vertical1Ang + " " + Arm.Horizontal2Ang + " " + Arm.Vertical2Ang + " " + Arm.Horizontal3Ang + " " + Arm.Gripper);
            else if (Arm.Connected && TCPHandler.Connected)
                TCPHandler.Write("MOVE " + Arm.BaseAng + " " + Arm.Horizontal1Ang + " " + Arm.Vertical1Ang + " " + Arm.Horizontal2Ang + " " + Arm.Vertical2Ang + " " + Arm.Horizontal3Ang + " " + Arm.Gripper);
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

        public void CloseAll()
        {
            Arm.Connected = false;
            CloseCore();
            CloseTCP();
        }
    }
}
