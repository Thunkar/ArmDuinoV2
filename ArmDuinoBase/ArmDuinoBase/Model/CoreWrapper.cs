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
    public class CoreWrapper : INotifyPropertyChanged
    {
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

        public void InitializeCore(long speed, int segments, int fields, bool server, int port, bool debug)
        {
            CoreProcess = new Process();
            string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string starter = "-jar " + currentDir + "\\ArmDuinoCore\\ArmDuinoCore.jar "+ COMPort + " "  + speed + " " + segments + " " + fields + " " + server + " " + port + " " + debug;
            string javaPath = GetJavaInstallationPath();
            if (!string.IsNullOrEmpty(javaPath))
            {
                
                var startInfo = new ProcessStartInfo(javaPath + "\\java.exe", starter);
                startInfo.WorkingDirectory = currentDir + "\\ArmDuinoCore";
                startInfo.RedirectStandardInput = startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                CoreProcess.StartInfo = startInfo;
                CoreProcess.EnableRaisingEvents = true;
            }
        }

        private string GetJavaInstallationPath()
        {
            string environmentPath = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(environmentPath))
            {
                return environmentPath;
            }

            string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment\\";
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
            CoreProcess.StandardInput.WriteLine(input);
        }

        public void StartCore()
        {
            CoreProcess.Start();
            IsStarted = true; ;
            CoreProcess.BeginOutputReadLine();
        }

        public void StopCore()
        {
            try
            {
                CoreProcess.StandardInput.WriteLine("STOP");
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
