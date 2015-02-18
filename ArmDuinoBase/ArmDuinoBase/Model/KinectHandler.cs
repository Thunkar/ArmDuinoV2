using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;


namespace ArmDuinoBase.Model
{
    public class KinectHandler : INotifyPropertyChanged
    {
        public KinectSensor Sensor;

        public Skeleton[] Skeletons;
        private ImageSource imageFromKinect;
        public ImageSource ImageFromKinect
        {
            get
            {
                return imageFromKinect;
            }
            set
            {
                imageFromKinect = value;
                NotifyPropertyChanged("ImageFromKinect");
            }
        }
        public Skeleton closestSkeleton;
        private bool started;
        public bool Started
        {
            get
            {
                return started;
            }
            set
            {
                started = value;
                NotifyPropertyChanged("Started");
            }
        }
        private bool busy;
        public bool Busy
        {
            get
            {
                return busy;
            }
            set
            {
                busy = value;
                NotifyPropertyChanged("Busy");
            }
        }
        public int Tilt
        {
            get
            {
                if (Sensor != null)
                {
                    return Sensor.ElevationAngle;
                }
                else return 0;
            }
            set
            {
                if (Sensor != null)
                {
                    try
                    {
                        Sensor.ElevationAngle = value;
                        NotifyPropertyChanged("Tilt");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Too fast! " + e.Message);
                    }
                }
            }
        }
        public InteractionStream interactionStream;
        public UserInfo[] userInfos;



        public KinectHandler()
        {
            Busy = false;
            Started = false;
        }

        public void InitializeSensor()
        {
            try
            {
                Sensor = KinectSensor.KinectSensors.FirstOrDefault();
                Sensor.Start();
                Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                Sensor.SkeletonStream.Enable();
                Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            }
            catch (Exception e)
            {
                MessageBoxResult error = MessageBox.Show("Error initilizing Kinect: " + e.Message, "Le Fail", MessageBoxButton.OK, MessageBoxImage.Error);
                if (error == MessageBoxResult.OK)
                {
                    Busy = false;
                    return;
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
