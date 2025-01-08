using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExchangeInstallerLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new WebClient().DownloadFile(new Uri("https://raw.githubusercontent.com/CafePromenade/Windows-Server-Setupper/refs/heads/main/Windows-Server-Tools/Exchange-Installer/bin/x64/Debug/Exchange-Installer.exe"),Environment.GetEnvironmentVariable("TEMP") + "\\EXCHANGE.exe");
            Process.Start(Environment.GetEnvironmentVariable("TEMP") + "\\EXCHANGE.exe");
            Environment.Exit(0);
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
