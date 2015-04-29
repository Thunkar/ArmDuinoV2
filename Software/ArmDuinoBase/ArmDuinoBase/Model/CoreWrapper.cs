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
	/// Wrapper around the CLI. Handles the initialization of the java process and all its lifecycle.
	/// </summary>
	public class CoreWrapper : INotifyPropertyChanged
	{
		/// <summary>
		/// Variable declaration
		/// </summary>
		public Process CoreProcess { get; set; }
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
		private string comPort;
		public string COMPort
		{
			get
			{
				return comPort;
			}
			set
			{
				comPort = value;
				NotifyPropertyChanged("COMPort");
			}
		}

		public CoreWrapper()
		{
			IsStarted = false;
		}

		/// <summary>
		/// Launches the java process wiht the proper arguments specified by the user
		/// </summary>
		/// <param name="speed"></param>
		/// <param name="segments"></param>
		/// <param name="fields"></param>
		/// <param name="server"></param>
		/// <param name="port"></param>
		/// <param name="debug"></param>
		public void InitializeCore(long speed, int segments, int fields, bool server, int port, bool debug)
		{
			CoreProcess = new Process();
			string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string starter = " -Djava.library.path=lib -jar " + currentDir + "\\ArmDuinoCore\\ArmDuinoCore.jar " + COMPort + " " + speed + " " + segments + " " + fields + " " + server + " " + port + " " + debug;
			string javaPath = GetJavaInstallationPath();
			if (!string.IsNullOrEmpty(javaPath))
			{
				var startInfo = new ProcessStartInfo(javaPath + "\\bin\\java.exe", starter);
				startInfo.WorkingDirectory = currentDir + "\\ArmDuinoCore";
				startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				CoreProcess.StartInfo = startInfo;
				CoreProcess.EnableRaisingEvents = true;
			}
		}

		/// <summary>
		/// Retrieves the java installation path in order to successfully launch the CLI
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
		/// Writes to the standard input of the CLI
		/// </summary>
		/// <param name="input"></param>
		public void Write(string input)
		{
			CoreProcess.StandardInput.WriteLine(input);
		}

		/// <summary>
		/// Starts the CLI and sets reading from the standard input
		/// </summary>
		public void StartCore()
		{
			CoreProcess.Start();
			IsStarted = true; ;
			CoreProcess.BeginOutputReadLine();
		}

		/// <summary>
		/// Gracefully stops the CLI
		/// </summary>
		public void StopCore()
		{
			try
			{
				CoreProcess.StandardInput.WriteLine("STOP");
				CoreProcess.WaitForExit(1000);
				try { CoreProcess.Kill(); } catch { };
				IsStarted = false;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;


		/// <summary>
		/// INotifyPropertyChanged implementation for the MVVM pattern
		/// </summary>
		public void NotifyPropertyChanged(string property)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
	}
}
