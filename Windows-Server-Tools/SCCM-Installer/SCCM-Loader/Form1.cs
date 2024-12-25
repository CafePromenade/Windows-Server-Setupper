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

namespace SCCM_Loader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new WebClient().DownloadFile("https://raw.githubusercontent.com/CafePromenade/Windows-Server-Setupper/refs/heads/main/Windows-Server-Tools/SCCM-Installer/SCCM-Installer/bin/x64/Debug/SCCM-Installer.exe",Environment.GetEnvironmentVariable("APPDATA") + "\\SCCM_INSTALLER.exe");
            Process.Start(Environment.GetEnvironmentVariable("APPDATA") + "\\SCCM_INSTALLER.exe");
            Environment.Exit(0);
        }
    }
}
