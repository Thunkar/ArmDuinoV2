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
	public class WebcamWrapper : INotifyPropertyChanged
	{
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


		public WebcamWrapper()
		{
			IsStarted = false;
		}

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

		public void Write(string input)
		{
			CambozolaProcess.StandardInput.WriteLine(input);
		}

		public void StartCambozola()
		{
			CambozolaProcess.Start();
			IsStarted = true; ;
			CambozolaProcess.BeginOutputReadLine();
		}

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

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
