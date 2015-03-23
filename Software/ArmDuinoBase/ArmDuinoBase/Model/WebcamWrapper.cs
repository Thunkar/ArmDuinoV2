using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArmDuinoBase.Model
{
	/// <summary>
	/// Wrapper around the cambozola instance that serves the video feed
	/// </summary>
	public class WebcamWrapper : INotifyPropertyChanged
	{
		/// <summary>
		/// Variable declaration
		/// </summary>
		public Process CambozolaProcess { get; set; }
		private bool isStarted;
		public bool IsStarted
		{
			get
			{
				return isStarted;
			}
			set
			{
				isStarted = value;
				NotifyPropertyChanged("IsStarted");
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public WebcamWrapper()
		{
			IsStarted = false;
		}

		/// <summary>
		/// Initializes the cambozola process with the apropriate arguments in order to connect to the video server
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="videoport"></param>
		public void InitializeFeed(string ip, string videoport)
		{
			CambozolaProcess = new Process();
			string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string starter = "-jar " + currentDir + "\\cambozola.jar http://" + ip + ":" + videoport;
			string javaPath = GetJavaInstallationPath();
			if (!string.IsNullOrEmpty(javaPath))
			{
				var startInfo = new ProcessStartInfo(javaPath + "\\java.exe", starter);
				startInfo.WorkingDirectory = currentDir;
				startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				CambozolaProcess.StartInfo = startInfo;
				CambozolaProcess.EnableRaisingEvents = true;
			}
		}

		/// <summary>
		/// Returns the java installation path in order to initialize the cambozola process successfully
		/// </summary>
		/// <returns></returns>
		private string GetJavaInstallationPath()
		{
			string environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");
			if (!string.IsNullOrEmpty(environmentPath))
			{
				return environmentPath;
			}

			string javaKey = "SOFTWARE/JavaSoft/Java Runtime Environment/";
			using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(javaKey))
			{
				string currentVersion = rk.GetValue("CurrentVersion").ToString();
				using (Microsoft.Win32.RegistryKey key = rk.OpenSubKey(currentVersion))
				{
					return key.GetValue("JavaHome").ToString();
				}
			}
		}

		/// <summary>
		/// Write to cambozola's process standard input
		/// </summary>
		/// <param name="input"></param>
		public void Write(string input)
		{
			CambozolaProcess.StandardInput.WriteLine(input);
		}


		/// <summary>
		/// Starts the process and enables reading from its standard input
		/// </summary>
		public void StartCambozola()
		{
			CambozolaProcess.Start();
			IsStarted = true; ;
			CambozolaProcess.BeginOutputReadLine();
		}


		/// <summary>
		/// Gracefully stops the process and shuts down the video feed
		/// </summary>
		public void StopWebcam()
		{
			try
			{
				CambozolaProcess.Kill();
				IsStarted = false;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error");
			}
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
