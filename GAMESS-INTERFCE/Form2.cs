using GAMESS_INTERFCE.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;

namespace GAMESS_INTERFCE
{
    //THIS IS A FIRST RUN FORM...here you can set email, password and PATH to gamess directory
    //When the form is compiled in all fields then we can change the boolean value of first run so that we no longer need to ask for this form again @ next load
    //Due to code implementation we need to change a bit the *.bat files in order to exit the entire CMD.exe process instead of the GAMESS PID only
    //Just delete the "\B" or "/B" @ the end of the file. NOTE: multiple exit found....
    public partial class Form2 : Form
    {
        private string Batfiletext;
        bool exit_with_button = false;
        bool mainsettings = true;
        bool wrongemailsettings = false;
        bool emptyemailsettings = false;

        public Form2()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //NOTE: no check made for email credentials, due to SMTP
            Settings.Default.Version = textBox1.Text;
            Settings.Default.Gamess = textBox2.Text;
            Settings.Default.Gamess_dat_file = textBox3.Text;
            /*Settings.Default.sender_email = textBox4.Text;
            Settings.Default.sender_password = textBox5.Text;
            Settings.Default.receiver_email = textBox6.Text;*/

            //Check if main settings fields are empty
            if ((textBox1.Text == "") || (textBox2.Text == "") || (textBox3.Text == "")) mainsettings = false;
            else mainsettings = true;
            //Check if email settings fields are empty
            if ((textBox4.Text == "") && (textBox5.Text == "") && (textBox6.Text == "")) emptyemailsettings = true;
            else emptyemailsettings = false;
            //Check if email settings are wrong (even one will trigger errors!)
            if ((!IsEmailValid(textBox4)) || (!IsEmailValid(textBox6))) wrongemailsettings = true;
            else wrongemailsettings = false;

            if (!mainsettings)
            {
                MessageBox.Show("The fields are empty or have an incorrect format!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (emptyemailsettings)
            {
                if (MessageBox.Show("Email fields are empty!\nDo you want to continue anyway?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Settings.Default.Enable_email = false;
                    Exit_procedures();
                }
            }
            else if (wrongemailsettings) MessageBox.Show("The email fields have an incorrect format!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                Settings.Default.sender_email = textBox4.Text;
                Settings.Default.sender_password = textBox5.Text;
                Settings.Default.receiver_email = textBox6.Text;
                Settings.Default.Enable_email = true;
                Exit_procedures();
            }
        }

        private void TextBox4_TextChanged(object sender, EventArgs e)
        {
            IsEmailValid(textBox4);
        }

        private void TextBox6_TextChanged(object sender, EventArgs e)
        {
            IsEmailValid(textBox6);
        }

        //Check if the entered email is a valid one
        public bool IsEmailValid(TextBox box)
        {
            try
            {
                if (box.Text != "")
                {
                    MailAddress m = new MailAddress(box.Text);
                    box.BackColor = Color.White;
                    return true;
                }
                throw new FormatException();
            }
            catch (FormatException)
            {
                box.BackColor = Color.Red;
                return false;
            }
        }

        private void Exit_procedures()
        {
            //Now we save the settings and replace some stuff in the bat file in order to execute the code as we want...
            Settings.Default.Save();
            MessageBox.Show("Settings saved!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try
            {
                //Here we modify the rungms.bat file to have automatic exit when the code executes in a bad way
                Batfiletext = File.ReadAllText(Settings.Default.Gamess + "\\rungms.bat");//Here we specify the path to the file
                Batfiletext = Batfiletext.Replace(" /B", "");
                //File.WriteAllText(Settings.Default.path_to_gamess + "\\rungms.bat", batfiletext);
                File.WriteAllText(Settings.Default.Gamess + "\\rungms.bat", Batfiletext);
                Settings.Default.Save();
                exit_with_button = true;
            }
            //Here we catch ANY errors, like wrong path
            catch
            {
                MessageBox.Show("Something went wrong...maybe you specified a path that is NOT the GAMESS one...", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(Form2_Closing);
            textBox1.Text = Settings.Default.Version;
            textBox2.Text = Settings.Default.Gamess;
            textBox3.Text = Settings.Default.Gamess_dat_file;
            textBox4.Text = Settings.Default.sender_email;
            textBox5.Text = Settings.Default.sender_password;
            textBox6.Text = Settings.Default.receiver_email;
            //Check if email is valid
            IsEmailValid(textBox4);
            IsEmailValid(textBox6);
        }


        private void Form2_Closing(object sender, FormClosingEventArgs e)
        {
            //Check the exit method....
            //If is the user that closes the app check for stuff
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //If the exit is done with the top exit button the setings won't be saved
                if (exit_with_button == false)
                {
                    MessageBox.Show("Settings won't be saved...", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Settings.Default.Save();
                }
            }
            else
            {
                //Or else the user ended the set up process and a permanent flag is set for future checks
                //Save specified settings
                Settings.Default.Save();
            }
        }
    }
}
