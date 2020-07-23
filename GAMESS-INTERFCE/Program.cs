using System;
using System.Windows.Forms;
using GAMESS_INTERFCE.Properties;

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
            if (Settings.Default.FirstTime == true)
            {
                Settings.Default.Reset();
                Settings.Default.Terminated_before_save = false;
                Settings.Default.Enable_email = false;
                Settings.Default.IsGamessInstalled = false;
                Settings.Default.FirstTime = false;
                Settings.Default.Save();
            }
            Application.Run(new Form1());
        }
    }
}