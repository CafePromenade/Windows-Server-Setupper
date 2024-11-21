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
using UsefulTools;

namespace Exchange_Installer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            Load += Form1_Load1;
        }

        private async void Form1_Load1(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("exchange"))
            {

            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\CompletedFirstTask.txt"))
            {
                button1.Enabled = false;
                button1.Text = "Please wait...";
                await Functions.SolveWindowsTasks();
                button1.Enabled = true;
                button1.Text = "OK"; 
                File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\CompletedFirstTask.txt","true");
            }
        }

        public static readonly string ExchangeDirectory = "C:\\Exchange";

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Visible = false;
            // Install Prequisites //
            await Chocolatey.ChocolateyDownload("vcredist2013 vcredist140 ucma4 urlrewrite googlechrome");
            // DA DHUI AND COMPLETE RESTART TASKS //
            await Functions.DaDhui();
            // Promote to DC //
            await Functions.InstallActiveDirectoryAndPromoteToDC(textBox1.Text, "P@ssw0rd", textBox1.Text.Split('.')[0].ToUpper());
        }
    }
}
