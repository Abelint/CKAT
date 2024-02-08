
using System;

using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace CKAT
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        static System.Threading.Mutex mutex = new System.Threading.Mutex(false, "CKAT_v2");

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
           


            if (!mutex.WaitOne(500, false))
            {
                IntPtr wndHandle = FindWindowByCaption(null, "MainWindow");
                //ShowWindow(wndHandle, WinStyle.ShowMaximized);
                ShowWindow(wndHandle, 1);
               
                Application.Current.Shutdown();
                return;
            }

            Process currentProcess = Process.GetCurrentProcess();
            var proc = Process.GetProcessesByName("CKAT_v2");
            if (proc.Length > 1)
            {
                MessageBox.Show("Это приложение уже запущено. Если вы его не видите, найдите приложение в трее или обратитесь к системному администратору");
                //PipeClient pipeClient = new PipeClient();
                //foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
                foreach (Process process in proc)
                {
                    if (process.Id != currentProcess.Id)
                    {
                        IntPtr mainWindowHandle = process.MainWindowHandle;
                        if (mainWindowHandle != IntPtr.Zero)
                        {
                            ShowWindow(mainWindowHandle, SW_RESTORE);
                            SetForegroundWindow(mainWindowHandle);
                            break;
                        }
                    }
                }
                Application.Current.Shutdown();
                WindowCollection windowCollection = Current.Windows;
                currentProcess.Close();
            }

           
        }
    }
}
