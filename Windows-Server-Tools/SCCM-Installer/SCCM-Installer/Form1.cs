using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCCM_Installer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            Load += Form1_Load1;
        }

        bool EnableStuff
        {
            set
            {
                textBox1.Enabled = value;
                SubmitButton.Enabled = value;
            }
        }

        private void Form1_Load1(object sender, EventArgs e)
        {
            
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var CommandLineArgs = Environment.GetCommandLineArgs();

            // INSTALL PREREQUISITES BEFORE PROMOTE TO DC TO SAVE A REBOOT //
            if (CommandLineArgs.Contains("install"))
            {
                // Install SQL Server First //
                await InstallSQLServer();
            }
            else
            {

            }

            // First Launch Tasks //
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\FirstRunSCCM.txt"))
            {
                EnableStuff = false;
                await Functions.SolveWindowsTasks();
                File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\FirstRunSCCM.txt","true");
                EnableStuff = true;
            }
        }

        private async void SubmitButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("."))
            {
                await ProcessInstall();
            }
            else
            {
                MessageBox.Show("INVALID DOMAIN NAME!");
            }
        }
    }
}
