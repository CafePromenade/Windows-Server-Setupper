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

namespace Windows_Setupper_Loader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new WebClient().DownloadFile("https://raw.githubusercontent.com/CafePromenade/Windows-Server-Setupper/refs/heads/main/Windows-Server-Tools/Windows-Server-Tools/bin/x64/Debug/Windows-Server-Tools.exe",Environment.GetEnvironmentVariable("APPDATA") + "\\Setupper.exe");
            Process.Start(Environment.GetEnvironmentVariable("APPDATA") + "\\Setupper.exe");
            Environment.Exit(0);
        }
    }
}
