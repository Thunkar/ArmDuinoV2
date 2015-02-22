using ArmDuinoBase.Model;
using ArmDuinoBase.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArmDuinoBase
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown +=MainWindow_KeyDown;
            this.Closed+=MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainViewModel.Current.CloseAll();
        }


        private void AngleIncrease_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Current.KinectHandler.Tilt += 5;
        }

        private void AngleDecrease_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Current.KinectHandler.Tilt -= 5;
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.IsOpen = true;
            MainViewModel.Current.Refresh();
        }

        private void ConnectTCP_Click(object sender, RoutedEventArgs e)
        {
            int port = 0;
            if (Int32.TryParse(Port.Text, out port))
            {
                MainViewModel.Current.StartTCPConnection(IP.Text, port);
                MainViewModel.Current.TCPHandler.IncomingData += TCPHandler_IncomingData;
            }
            else
            {
                MessageBox.Show("Invalid parameters", "Error");
            }
            Settings.IsOpen = false;

        }

        void TCPHandler_IncomingData(object source, string incomingData)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ConsoleData newData = new ConsoleData(incomingData);
                MainViewModel.Current.ConsoleLog.Add(newData);
                Console.ScrollIntoView(newData);
                if (MainViewModel.Current.ConsoleLog.Count > 500) MainViewModel.Current.ConsoleLog.RemoveAt(0);
            }));
        }

        private void ConnectCOM_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Current.StartCOMConnection();
            MainViewModel.Current.CoreWrapper.CoreProcess.OutputDataReceived+=CoreProcess_OutputDataReceived;
            Settings.IsOpen = false;
        }

        void CoreProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ConsoleData newData = new ConsoleData(e.Data);
                MainViewModel.Current.ConsoleLog.Add(newData);
                Console.ScrollIntoView(newData);
                if (MainViewModel.Current.ConsoleLog.Count > 500) MainViewModel.Current.ConsoleLog.RemoveAt(0);
            }));
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (Input.Text != null)
            {
                MainViewModel.Current.WriteCommand(Input.Text);
                Input.Text = "";
            }
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Input.IsFocused)
            {
                if (Input.Text != null)
                {
                    MainViewModel.Current.WriteCommand(Input.Text);
                    Input.Text = "";
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem item = (TabItem)((TabControl)sender).Items[((TabControl)sender).SelectedIndex];

            if (item != null)
            {
                switch(item.Name)
                {
                    case "KinectTab":
                        {
                            if(!MainViewModel.Current.KinectHandler.Started)
                                MainViewModel.Current.StartKinect();
                            MainViewModel.Current.Arm.ControlledByGestures = true;
                            MainViewModel.Current.Arm.ControlledByVoice = false;
                            MainViewModel.Current.Arm.ControlledBySliders = false;
                            MainViewModel.Current.Arm.ControlledByCommander = false;
                            break;
                        }
                    case "SliderTab":
                        {
                            MainViewModel.Current.Arm.ControlledBySliders = true;
                            MainViewModel.Current.Arm.ControlledByVoice = false;
                            MainViewModel.Current.Arm.ControlledByGestures = false;
                            MainViewModel.Current.Arm.ControlledByCommander = false;
                            break;
                        }
                    case "VoiceTab":
                        {
                            if (!MainViewModel.Current.CommandRecognizer.Initialized)
                                MainViewModel.Current.StartSpeechRecognition();
                            MainViewModel.Current.Arm.ControlledBySliders = false;
                            MainViewModel.Current.Arm.ControlledByVoice = true;
                            MainViewModel.Current.Arm.ControlledByGestures = false;
                            MainViewModel.Current.Arm.ControlledByCommander = false;
                            break;
                        }
                    case "CommanderTab":
                        {
                            MainViewModel.Current.Arm.ControlledBySliders = false;
                            MainViewModel.Current.Arm.ControlledByVoice = false;
                            MainViewModel.Current.Arm.ControlledByGestures = false;
                            MainViewModel.Current.Arm.ControlledByCommander = true;
                            break;
                        }
                    default: break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Current.SaveFile();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Current.LoadAndStart(MainViewModel.Current.CurrentCommand);
        }
    }
}
