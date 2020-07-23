using GAMESS_INTERFCE.Properties;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Collections;
using System.Windows.Forms;

namespace GAMESS_INTERFCE
{
    public partial class Form1 : Form
    {

        public string FileNameWithoutExtension, Input_job_file, Input_dat_file, Ncpus, Log_file, Wfn_file, Datfileline;
        public bool doIhavetocopy = false;
        public string AIMPACString, AIMPACStringtofile;
        public int AIMPACIndex;
        public bool doIhavetosendemail;

        public Form1()
        {
            InitializeComponent();
            Properties.Settings.Default.PropertyChanged += SettingChanged;

            void SettingChanged(object sender, PropertyChangedEventArgs e)
            {
                // Do something
                if (Settings.Default.Enable_email) checkBox1.Enabled = true;
                else checkBox1.Enabled = false;
            }
        }

        //Run the gamess bat file that will execute the actual run inp to log
        public async void Button1_Click(object sender, EventArgs e)
        {
            //Check that fields are not empty
            if (!String.IsNullOrEmpty(Input_job_file) && !String.IsNullOrEmpty(Log_file))
            {
                //Disable buttons during job run
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                textBox3.Text = "Executing code...\r\n";

                //Here we prepare the arguments for the bat gamess file with the correct format
                string outtext;
                string batpath = string.Format(Settings.Default.Gamess + "\\rungms.bat");
                string arguments = string.Format("{0} {1} {2} 0 {3}", Input_job_file, Settings.Default.Version, Ncpus, Log_file);

                //Here we need to copy the input file to the gamess directory in order to avoid errors
                File.Copy(Input_job_file, Settings.Default.Gamess + "\\" + FileNameWithoutExtension + ".inp");
                Process gamessjob = new Process();
                gamessjob.StartInfo.ErrorDialog = true;
                gamessjob.StartInfo.UseShellExecute = false;
                gamessjob.StartInfo.CreateNoWindow = true;
                gamessjob.StartInfo.RedirectStandardOutput = true;
                gamessjob.StartInfo.RedirectStandardError = true;
                gamessjob.EnableRaisingEvents = true;
                gamessjob.StartInfo.WorkingDirectory = Settings.Default.Gamess; //Set the work directory according to where gamess is installed
                gamessjob.StartInfo.FileName = batpath;
                gamessjob.StartInfo.Arguments = arguments; //input_file, version, number_of_processors, "0", output_name]
                gamessjob.Start(); //Start the process

                //Here we intercept the details that the bat file would normally show in the terminal to the textbox to the user in this gui
                while ((outtext = await gamessjob.StandardOutput.ReadLineAsync()) != null)
                {
                    textBox3.AppendText(outtext + "\r\n");
                }

                //Now the process is completed
                textBox3.AppendText("\r\nProcess executed!");

                //here we clean up some stuff after GAMESS code ends
                //This is because gamess will create a copy of the original *.inp file to the working directory but after the run it will still be here
                File.Delete(Settings.Default.Gamess + "\\" + FileNameWithoutExtension + ".inp");
                MessageBox.Show("Code executed!\nCheck output for errors or messages.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Re-enable buttons
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            else MessageBox.Show("Input file and/or output location is invalid!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //Browse to search for the input file *.inp
        private void Button2_Click(object sender, EventArgs e)
        {
            //Filter results with the *.inp file format since those are the files that we're interested in...
            OpenFileDialog Openinpfile = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                Title = "Chose *.inp file",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "inp",
                Filter = "inp files (*.inp)|*.inp",
                FilterIndex = 2,
                RestoreDirectory = true,
                Multiselect = false,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (Openinpfile.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = Openinpfile.FileName;
                Input_job_file = Openinpfile.FileName;
                FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(Openinpfile.FileName);
                textBox2.Text = ""; //Here we clear the destination path text box so that the user is forced to browse for a new one
                                    //Will help prevent overwrite op previous files
            }
        }

        //Browse where to save the output file *.log
        private void Button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog Savelogfile = new SaveFileDialog
            {
                FileName = FileNameWithoutExtension,
                DefaultExt = ".log",
                Filter = "log files (*.log)|*.log",
                RestoreDirectory = true

            };

            if (Savelogfile.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = Savelogfile.FileName;
                Log_file = Savelogfile.FileName;
            }
        }

        //Clear the folder containing previous runs to ensure that you use the most updated data
        private void Button4_Click(object sender, EventArgs e)
        {
            string[] filePaths = Directory.GetFiles(Settings.Default.Gamess_dat_file);

            foreach (string filePath in filePaths)
                File.Delete(filePath);
            MessageBox.Show("Restart folder clean!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Browse to search for the input file *.dat
        private void Button5_Click(object sender, EventArgs e)
        {
            //Filter results with the *.dat file format since those are the files that we're interested in...
            OpenFileDialog Openinpfile = new OpenFileDialog
            {
                InitialDirectory = Settings.Default.Gamess_dat_file,
                Title = "Chose *.dat file",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "dat",
                Filter = "dat files (*.dat)|*.dat",
                FilterIndex = 2,
                RestoreDirectory = true,
                Multiselect = false,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (Openinpfile.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = Openinpfile.FileName;
                Input_dat_file = Openinpfile.FileName;
                FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(Openinpfile.FileName);
                textBox5.Text = "";//Here we clear the destination path textbox so that the user is forced to browse for a new one
                                   //Will help prevent overwrite op previous files
            }
        }

        //Browse to search for the output file *.wfn
        private void Button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog Savelogfile = new SaveFileDialog
            {
                FileName = FileNameWithoutExtension,
                DefaultExt = ".wfn",
                Filter = "wfn files (*.wfn)|*.wfn",
                RestoreDirectory = true

            };
            if (Savelogfile.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = Savelogfile.FileName;
                Wfn_file = Savelogfile.FileName;
            }
        }

        //Run the gamess bat file that will execute the actual run inp to log
        //This is an async function due to the fact that the run will be so time consuming that the gui will be stuck and prompt the user that the process does not respond
        //This way the app will survive until the process is properly executed
        private async void Button7_Click(object sender, EventArgs e)
        {
            //Here we create the file and do a 2 time pass to determin if the file contains the necessary data for the waveform file
            //the async reading is done only for a single byte[]. We need to split to single lines

            if (!String.IsNullOrEmpty(Input_dat_file) && !String.IsNullOrEmpty(Wfn_file))
            {
                //Disable buttons during job
                button5.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                byte[] result;
                textBox6.Text = "Executing code...\r\n\n";
                using (FileStream SourceStream = File.Open(Input_dat_file, FileMode.Open))
                {
                    result = new byte[SourceStream.Length];
                    await SourceStream.ReadAsync(result, 0, (int)SourceStream.Length);
                }
                AIMPACString = System.Text.Encoding.ASCII.GetString(result);

                //Check for AIMPAC data
                if (AIMPACString.Contains("GAUSSIAN"))
                {
                    AIMPACIndex = AIMPACString.IndexOf("GAUSSIAN");
                    doIhavetocopy = true;
                    AIMPACStringtofile = "Title\r";
                }
                else
                {
                    textBox6.Text = "No AIMPAC data found!";
                    MessageBox.Show("No AIMPAC data found in the specified file!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                //If AIMPAC data is found do a new pass that will copy the necessary part of the file
                if (doIhavetocopy)
                {
                    AIMPACString = AIMPACString.Remove(0, AIMPACIndex - 1);
                    AIMPACStringtofile += AIMPACString;
                    textBox6.AppendText(AIMPACStringtofile + "\r\nProcess executed!");
                    doIhavetocopy = false;

                    FileStream fs = File.Create(Wfn_file);
                    fs.Close();
                    File.WriteAllText(Wfn_file, AIMPACStringtofile);
                    fs.Close();

                    MessageBox.Show("Code executed!\nCheck output for errors or messages.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if ((Settings.Default.Enable_email) && (checkBox1.Checked))
                    {
                        textBox6.AppendText("\r\nSending email...");
                        Email_send();
                    }
                    else textBox6.AppendText("\r\nEmail sending aborted!");
                }
                //Re enable buttons
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
            }
            else MessageBox.Show("Input file and/or output location is invalid!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //Function that handle email preparation and sending
        public void Email_send()
        {
            string provider = "";
            //Limit is 25Mb, at least for gmail
            //Now get file to attach and filesize
            FileInfo f = new FileInfo(Wfn_file);
            long filesize = f.Length / 1048576;
            //file size = file size / 1048576;
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();
            //Here we check the email service provider of the sender email and apply proper settings
            if (Settings.Default.sender_email.Contains("gmail"))
            {
                SmtpServer = new SmtpClient("smtp.gmail.com");
                provider = "Google Gmail";

            }
            else if (Settings.Default.sender_email.Contains("yahoo"))
            {
                SmtpServer = new SmtpClient("smtp.mail.yahoo.com");
                provider = "Yahoo Mail";
            }
            mail.From = new MailAddress(Settings.Default.sender_email);
            mail.To.Add(Settings.Default.receiver_email);
            mail.Subject = "WFN file is ready!";
            //Check if filesize is in limit of the email provider and write an appropriate body
            if (filesize < 25)
            {
                mail.Body = "WFN file for " + Path.GetFileNameWithoutExtension(Wfn_file) + " has been generated!\nCheck out the attachment...";
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(Wfn_file);
                mail.Attachments.Add(attachment);
            }
            else mail.Body = "WFN file for " + Path.GetFileNameWithoutExtension(Wfn_file) + " has been generated!\nNOTE: due to attachment size limit the file hasn't been sent...";

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new NetworkCredential(Settings.Default.sender_email, Settings.Default.sender_password);
            SmtpServer.EnableSsl = true;
            try
            {
                SmtpServer.Send(mail);
                textBox6.AppendText("\r\nDetected email provider: ");
                textBox6.AppendText(provider);
                textBox6.AppendText("\r\nEmail sent successfully!");
                MessageBox.Show("Success email!");
            }
            catch (Exception ex)
            {
                textBox6.AppendText("\r\nDetected email provider: ");
                textBox6.AppendText(provider);
                textBox6.AppendText("\r\nEmail sending failed!");
                MessageBox.Show(ex.Message);
            }
        }

        //Update a variable that keep track of the will of the user to send the output file via email
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            doIhavetosendemail = checkBox1.Checked;
        }

        //Combo box where you can specify how many processes you want to set up for the job
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Ncpus = comboBox1.Text;
        }

        private void FileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form2 settings = new Form2();
            settings.ShowDialog();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void AboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }

        private void OnlineFeedbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://forms.gle/myfXTZ1urTQtmsFE8");
        }

        //This is an automated function that checks if GAMESS is installed in the system and retrieves the Version and InstallPath
        //This will be run every time the program is executed, to ensure that GAMESS is still installed
        static void Check_GAMESS_install()
        {
            //Base registry key
            string registryKey = @"SOFTWARE\Mark S. Gordon Quantum Theory Group";
            string version = @"";
            string path = @"";

            //Read the value of the "Path" and "Version" registry key and store them into separate strings
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
                using (key)
                {
                    (from a in key.GetSubKeyNames()
                     let r = key.OpenSubKey(a)
                     select new
                     {
                         GamessPath = r.GetValue("Path"),
                         GamessVer = r.GetValue("Version"),
                     }
                    )
                    .Where(c => c.GamessPath != null)
                    .Where(c => c.GamessVer != null)
                    .ToList()
                    .ForEach(c =>
                    {
                        version = c.GamessVer.ToString();
                        path = c.GamessPath.ToString();
                    });
                }
            }

            catch (Exception ex)
            {
                Settings.Default.IsGamessInstalled = false;
                Settings.Default.Save();
                MessageBox.Show(ex.ToString());
            }

            //If an installation exists we manipulate the version string to create the exe-filename prototype and the proper version naming
            if (path != "" && version != "")
            {
                Settings.Default.IsGamessInstalled = true;
                Settings.Default.Gamess = path;

                var builder = new StringBuilder();
                int cnt = 0;
                foreach (var c in version)
                {
                    builder.Append(c);
                    if (c == '.')
                    {
                        switch (cnt)
                        {
                            case 0:
                                builder.Append('R');
                                cnt++;
                                break;
                            case 1:
                                builder.Append('P');
                                cnt++;
                                break;
                        }
                    }
                }
                builder.Append(".mkl");

                Settings.Default.Version = builder.ToString();
                Settings.Default.Gamess = path;
                Settings.Default.Gamess_dat_file = Settings.Default.Gamess + @"restart\";
                Settings.Default.ExeFileName = "gamess." + Settings.Default.Version + ".exe";
                Settings.Default.Save();

#if DEBUG
                MessageBox.Show(Settings.Default.IsGamessInstalled.ToString());
                MessageBox.Show(Settings.Default.Version);
                MessageBox.Show(Settings.Default.Gamess);
                MessageBox.Show(Settings.Default.Gamess_dat_file);
                MessageBox.Show(Settings.Default.ExeFileName);
#endif
            }

            else
            {
                MessageBox.Show("No valid GAMESS installation was found in the system!\nPlease install GAMESS before running this program...", "GAMESS not found");
                System.Environment.Exit(1);
            }
        }

        //Check if any previous instances of GAMESS are running
        static void Check_running_instances()
        {
            bool killed = false;
            if (Settings.Default.ExeFileName != "")
            {
                try //CLOSE any process that it's still running
                {
                    foreach (Process proc in Process.GetProcessesByName(Settings.Default.ExeFileName))
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Create a closing event handler
            this.FormClosing += new FormClosingEventHandler(Form1_Closing);

#if DEBUG
            MessageBox.Show(Environment.ProcessorCount.ToString());
#endif
            for (int i = 1; i <= Environment.ProcessorCount; i++)
            {
                comboBox1.Items.Add(i.ToString());
            }

            //Check the installation and retrieve important data, like version and path of install
            Check_GAMESS_install();

            Check_running_instances();

            //Check for restart folder
            //This folder not always is created when installing gamess
            if (!Directory.Exists(Settings.Default.Gamess_dat_file))
            {
                MessageBox.Show("Path to gamess dat files does not exist.\nCreating one", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Directory.CreateDirectory(Settings.Default.Gamess);
            }

            comboBox1.SelectedIndex = 0;
            //Set visual check based on settings
            if (!Settings.Default.Enable_email) checkBox1.Enabled = false;

            //clear folder from any *.inp files
            //Maybe the user used gamess before but didn't cleared old input files...
            DirectoryInfo di = new DirectoryInfo(Settings.Default.Gamess);
            FileInfo[] files = di.GetFiles("*.inp")
                                 .Where(p => p.Extension == ".inp").ToArray();
            foreach (FileInfo file in files)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            //Here we check if the app is being closed based on user will or if it due to the fact that the user didn't ended the first run settings process
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Are you sure to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    //check for process running

                    e.Cancel = true;
                }

            }
        }
    }
}