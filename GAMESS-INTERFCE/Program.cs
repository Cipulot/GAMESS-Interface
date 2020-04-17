using GAMESS_INTERFCE.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace GAMESS_INTERFCE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            //Settings reset when in debug mode
            Settings.Default.Version = "2019.R1.P1.mkl";
            Settings.Default.Gamess = "";
            Settings.Default.Gamess_dat_file = "";
            Settings.Default.sender_email = "";
            Settings.Default.sender_password = "";
            Settings.Default.receiver_email = "";
            Settings.Default.Terminated_before_save = false;
            Settings.Default.Enable_email = false;
            Settings.Default.Is_First_Run = true;
            Application.Run(new Form2());
            if (Settings.Default.Terminated_before_save != true) Application.Run(new Form1());
            else Application.Exit();
            Check_running_instances();
#else
            //Check if it's first run or not
            if (Settings.Default.Is_First_Run == true)
            {
                MessageBox.Show("Seems that this is first run...\nWe'll proceed to set up some stuff");
                //Check for GAMESS install before runnigs application
                Application.Run(new Form2()); //The first run bool value will be set to false @ the end of form 2
                Check_GAMESS_install();
                if (Settings.Default.Terminated_before_save != true)
                {
                    Application.Run(new Form1());
                    Check_running_instances();
                }
                else Application.Exit();
            }
            else
            {
                Check_GAMESS_install();
                Application.Run(new Form1());
                Check_running_instances();
            }
#endif
        }

        static void Check_running_instances()
        {
            bool killed = false;
            try //CLOSE any process that it's still running
            {
                foreach (Process proc in Process.GetProcessesByName("gamess.2019.R1.P1.mkl.exe"))
                {
                    proc.Kill();
                    killed = true;
                }
                if (killed) MessageBox.Show("Some instances of GAMESS were still running.\nThe end of the process has been completed!");
            }
            catch (Exception ex)
            {
            }
        }

        static void Check_GAMESS_install()
        {
            //If file doesn't exists then gamess isn't installed in the specified directory or at all
            if (!File.Exists(Settings.Default.Gamess + "\\rungms.bat"))
            {
                MessageBox.Show("No valid GAMESS installation was found in the specified directory!\nPlease check directory or installation...", "GAMESS not found");
                System.Environment.Exit(1);
            }
        }
    }
}