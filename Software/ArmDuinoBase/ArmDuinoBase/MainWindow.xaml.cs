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
    /// Interaction logiv for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		/// <summary>
		/// Constructor
		/// </summary>
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += MainWindow_KeyDown;
            this.Closed += MainWindow_Closed;
        }

		/// <summary>
		/// Closed event handler. Ensures everything is gracefully closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainViewModel.Current.CloseAll();
        }

		/// <summary>
		/// Settings button click handler. Opens the settings popup.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.IsOpen = true;
            MainViewModel.Current.Refresh();
        }

		/// <summary>
		/// Connect button click handler. Connects the TCPClient to the specified server and enables the video stream.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void ConnectTCP_Click(object sender, RoutedEventArgs e)
        {
            int port = 0;
			int videoPort = 0;
            if (Int32.TryParse(Port.Text, out port) && Int32.TryParse(VideoPort.Text, out videoPort))
            {
                MainViewModel.Current.StartTCPConnection(IP.Text, port);
				MainViewModel.Current.StartVideoConnection(IP.Text, videoPort);
                MainViewModel.Current.TCPHandler.IncomingData += TCPHandler_IncomingData;
				MainViewModel.Current.WebcamWrapper.CambozolaProcess.OutputDataReceived += CambozolaProcess_OutputDataReceived;
            }
            else
            {
                MessageBox.Show("Invalid parameters", "Error");
            }
            Settings.IsOpen = false;

        }

		/// <summary>
		/// Handles the standard output of the video stream process and shows it in our console
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CambozolaProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
		{
			try
			{
				Dispatcher.Invoke(new Action(() =>
				{
					ConsoleData newData = new ConsoleData(e.Data);
					MainViewModel.Current.ConsoleLog.Add(newData);
					Console.ScrollIntoView(newData);
					if (MainViewModel.Current.ConsoleLog.Count > 500) MainViewModel.Current.ConsoleLog.RemoveAt(0);
				}));
			}
			catch (Exception) { }
		}

		/// <summary>
		/// Handles the output stream of the tcp connection and shows it in our console
		/// </summary>
		/// <param name="source"></param>
		/// <param name="incomingData"></param>
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

		/// <summary>
		/// Connect button click handler for the COM port.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void ConnectCOM_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Current.StartCOMConnection();
            MainViewModel.Current.CoreWrapper.CoreProcess.OutputDataReceived += CoreProcess_OutputDataReceived;
            Settings.IsOpen = false;
        }


		/// <summary>
		/// Handles the standard input of the CLI java process
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void CoreProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ConsoleData newData = new ConsoleData(e.Data);
                    MainViewModel.Current.ConsoleLog.Add(newData);
                    Console.ScrollIntoView(newData);
                    if (MainViewModel.Current.ConsoleLog.Count > 500) MainViewModel.Current.ConsoleLog.RemoveAt(0);
                }));
            }
            catch (Exception) { }
        }


		/// <summary>
		/// Sends whatever is written in the commmand box to the current active connection (COM or TCP)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (Input.Text != null)
            {
                MainViewModel.Current.WriteCommand(Input.Text);
                Input.Text = "";
            }
        }


		/// <summary>
		/// ends whatever is written in the commmand box to the current active connection (COM or TCP)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Handles navigation between tabs
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem item = (TabItem)((TabControl)sender).Items[((TabControl)sender).SelectedIndex];

            if (item != null)
            {
                switch (item.Name)
                {
                    case "SliderTab":
                        {
                            MainViewModel.Current.Arm.ControlledBySliders = true;
                            MainViewModel.Current.Arm.ControlledByVoice = false;
                            MainViewModel.Current.Arm.ControlledByGestures = false;
                            MainViewModel.Current.Arm.ControlledByCommander = false;
							MainViewModel.Current.Arm.ControlledByGamePad = false;
							break;
                        }
					case "GamePadTab":
						{
							MainViewModel.Current.StartGamePad();
							MainViewModel.Current.Arm.ControlledBySliders = false;
							MainViewModel.Current.Arm.ControlledByVoice = false;
							MainViewModel.Current.Arm.ControlledByGestures = false;
							MainViewModel.Current.Arm.ControlledByCommander = false;
							MainViewModel.Current.Arm.ControlledByGamePad = true;
							break;
						}
                    default: break;
                }
            }
        }


		/// <summary>
		/// Accelerate button click handler. Increments every motor speed at once.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Acc_Click(object sender, RoutedEventArgs e)
		{
			MainViewModel.Current.Rover.FrontLeftSpeed++;
			MainViewModel.Current.Rover.FrontRightSpeed++;
			MainViewModel.Current.Rover.RearLeftSpeed++;
			MainViewModel.Current.Rover.RearRightSpeed++;
		}

		/// Deccelerate button click handler. Reduces every motor speed at once.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Dec_Click(object sender, RoutedEventArgs e)
		{
			MainViewModel.Current.Rover.FrontLeftSpeed--;
			MainViewModel.Current.Rover.FrontRightSpeed--;
			MainViewModel.Current.Rover.RearLeftSpeed--;
			MainViewModel.Current.Rover.RearRightSpeed--;
		}

		/// <summary>
		/// Resets speed values to the defaults
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ResetSpeed_Click(object sender, RoutedEventArgs e)
		{
			MainViewModel.Current.Rover.FrontLeftSpeed = MainViewModel.Current.Rover.FrontRightSpeed = MainViewModel.Current.Rover.RearLeftSpeed = MainViewModel.Current.Rover.RearRightSpeed = 255;
		}
	}
}
