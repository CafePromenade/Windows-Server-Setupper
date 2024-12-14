using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var CommandLineArgs = Environment.GetCommandLineArgs();

            // INSTALL PREREQUISITES BEFORE PROMOTE TO DC TO SAVE A REBOOT //
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {

        }
    }
}
