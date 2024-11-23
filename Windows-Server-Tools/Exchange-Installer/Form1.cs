using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            FormClosing += Form1_FormClosing;
        }

        bool DoNotClose = true;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = DoNotClose;
        }

        public static string DomainName
        {
            get
            {
                if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\Domain.txt"))
                {
                    return File.ReadAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\Domain.txt").Split('.')[0];
                }
                return "";
            }
        }

        public static string DomainCOM
        {
            get
            {
                if (File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\Domain.txt"))
                {
                    return File.ReadAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\Domain.txt").Split('.')[1];
                }
                return "";
            }
        }

        string FirstStepTimeFile = Environment.GetEnvironmentVariable("APPDATA") + "\\FirstStepTimeFile.txt";
        string SecondStepTimeFile = Environment.GetEnvironmentVariable("APPDATA") + "\\SecondStepTimeFile.txt";
        string ThirdStepTimeFile = Environment.GetEnvironmentVariable("APPDATA") + "\\ThirdStepTimeFile.txt";

        public void RetrieveTimes()
        {
            DateTime firstStepTime = DateTime.MinValue;
            DateTime secondStepTime = DateTime.MinValue;
            DateTime thirdStepTime = DateTime.MinValue;

            // Retrieve and display FirstStepTime
            if (File.Exists(FirstStepTimeFile))
            {
                firstStepTime = DateTime.Parse(File.ReadAllText(FirstStepTimeFile));
                FirstStepLabel.Text = "Prerequisites Since: " + firstStepTime.ToString("T");
            }

            // Retrieve and display SecondStepTime and calculate the difference from FirstStepTime
            if (File.Exists(SecondStepTimeFile))
            {
                secondStepTime = DateTime.Parse(File.ReadAllText(SecondStepTimeFile));
                SecondStepLabel.Text = "Domain Controller Promoted Since: " + secondStepTime.ToString("T");

                if (firstStepTime != DateTime.MinValue)
                {
                    TimeSpan timeBetweenFirstAndSecond = secondStepTime - firstStepTime;
                    FirstToSecondLabel.Text = "Time Between Prerequisites and Domain Controller Promotion: " + timeBetweenFirstAndSecond.ToString();
                }
            }

            // Retrieve and display ThirdStepTime and calculate the differences from previous steps
            if (File.Exists(ThirdStepTimeFile))
            {
                thirdStepTime = DateTime.Parse(File.ReadAllText(ThirdStepTimeFile));
                ThirdStepLabel.Text = "Exchange Fully Installed Since: " + thirdStepTime.ToString("T");

                if (secondStepTime != DateTime.MinValue)
                {
                    TimeSpan timeBetweenSecondAndThird = thirdStepTime - secondStepTime;
                    SecondToThirdLabel.Text = "Time Between Domain Controller Promotion and Exchange Installation: " + timeBetweenSecondAndThird.ToString();
                }

                if (firstStepTime != DateTime.MinValue)
                {
                    TimeSpan timeBetweenFirstAndThird = thirdStepTime - firstStepTime;
                    FirstToThirdLabel.Text = "Total Time Between Prerequisites and Exchange Installation: " + timeBetweenFirstAndThird.ToString();
                }
            }
        }


        private async void Form1_Load1(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("process_install"))
            {
                // Clear Pending Reboots //
                await Functions.RunPowerShellScript("Remove-ItemProperty -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Session Manager' -Name 'PendingFileRenameOperations' -Force");
                // Install UCMA4 again //
                if (!Directory.Exists("C:\\Program Files\\Microsoft UCMA 4.0"))
                {
                    await Functions.RunPowerShellScript("choco install ucma4 --force -y"); 
                }
                OKButton.Enabled = false;
                textBox1.Enabled = false;
                File.WriteAllText(SecondStepTimeFile,DateTime.Now.ToString("O"));
                RetrieveTimes();
                string exchangeSetupPath = "\"C:\\Exchange\\Setup.exe\"";

                // Prepare Exchange environment //
                DomainNameLabel.Text = "Preparing Schema";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareSchema");
                DomainNameLabel.Text = "Preparing AD";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareAD /OrganizationName:\"" + DomainName + "\"");
                DomainNameLabel.Text = "Preparing All Domains";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareAllDomains");
                DomainNameLabel.Text = "Preparing The Domain";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareDomain:" + DomainName + "." + DomainCOM);
                DomainNameLabel.Text = "INSTALLING EXCHANGE SERVER 2019";
                // Install Mailbox Role //
                await Functions.RunPowerShellScript(exchangeSetupPath + " /Mode:Install /Roles:Mailbox /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /InstallWindowsComponents");

                // 500 Error //
                DomainNameLabel.Text = "Configuring Mailbox...";
                if (Directory.Exists(@"C:\Program Files\Microsoft\Exchange Server\V15\Bin\"))
                {
                    string ExchangeBinDir = "C:\\Program Files\\Microsoft\\Exchange Server\\V15\\Bin";
                    await Functions.RunPowerShellScript(File.ReadAllText(ExchangeBinDir + "\\UpdateCas.ps1"));
                    await Functions.RunPowerShellScript(File.ReadAllText(ExchangeBinDir + "\\UpdateConfigFiles.ps1"));
                }
                // Save Time Lapse //
                File.WriteAllText(ThirdStepTimeFile, DateTime.Now.ToString("O"));
                RetrieveTimes();
                // DELETE TASK //
                Command.RunCommandHidden("schtasks /delete /tn \"" + "Run EXCHANGE\" /f");
                DomainNameLabel.Text = "Stuff";
                await Chocolatey.ChocolateyDownload("googlechrome");
                DoNotClose = false;
                DomainNameLabel.Text = "Exchange Server 2019 installed";
            }

            if (Environment.GetCommandLineArgs().Contains("exchange"))
            {
                Visible = false;
                OKButton.Enabled = false;
                DomainNameLabel.Text = "Prerequisites in progress";
                await Task.Delay(1000);
                string exchangeSetupPath = "\"C:\\Exchange\\Setup.exe\"";
                //await Functions.RunPowerShellScript("choco install urlrewrite -y");
                //await Functions.RunPowerShellScript("choco install vcredist2013 vcredist140 ucma4 googlechrome urlrewrite -y");
                DomainNameLabel.Text = "Visual C++ & DotNet";
                await Chocolatey.ChocolateyDownload("vcredist2013 vcredist140");
                DomainNameLabel.Text = "Unified Communications API";
                await Chocolatey.ChocolateyDownload("ucma4");
                DomainNameLabel.Text = "IIS URL Rewrite";
                await Chocolatey.ChocolateyDownload("urlrewrite");
                Command.RunCommandHidden("schtasks /delete /tn \"" + "Run EXCHANGE\" /f");
                await Task.Delay(2000);
                Functions.DaDhui(true, "process_install");
                File.WriteAllText(FirstStepTimeFile, DateTime.Now.ToString("O"));
                RetrieveTimes();
                DoNotClose = false;
                Environment.Exit(0);
            }

            

            if (Environment.GetCommandLineArgs().Contains("dadhui"))
            {
                // DaDhui The Program //
                await Functions.DaDhui(true);
                Close();
            }

            if (Environment.GetCommandLineArgs().Contains("pre"))
            {
                // Install Prerequisites //
                await Functions.RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS\n pause");
                await Functions.RunPowerShellScript("choco install vcredist2013 vcredist140 ucma4 googlechrome urlrewrite -y");
                Close();
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\CompletedFirstTask.txt"))
            {
                // Setup and initialize windows before installing everything //
                MainProgressBar.Style = ProgressBarStyle.Marquee;
                DomainNameLabel.Text = "Windows is updating, will be ready shortly!";
                OKButton.Enabled = false;
                OKButton.Text = "Please wait...";
                await Functions.SolveWindowsTasks();
                OKButton.Enabled = true;
                OKButton.Text = "OK"; 
                File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\CompletedFirstTask.txt","true");
                MainProgressBar.Style = ProgressBarStyle.Blocks;
                DomainNameLabel.Text = "Please enter a new domain name below!";
            }
        }

        public static readonly string ExchangeDirectory = "C:\\Exchange";

        private async void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains(".") && !textBox1.Text.Contains(" "))
            {
                textBox1.Enabled = false;
                OKButton.Enabled = false;
                //Visible = false;
                // Install Prequisites //
                DomainNameLabel.Text = "Installing server roles";
                await Functions.RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS");
                // QUICKER PREREQUISITES //
                string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                DomainNameLabel.Text = "Installing prerequisites";
                await Task.Factory.StartNew(() =>
                {
                    Process.Start(currentPath, "exchange").WaitForExit();
                });

                // Promote to DC //
                DomainNameLabel.Text = "Promoting to domain";
                await Functions.InstallActiveDirectoryAndPromoteToDC(textBox1.Text, "P@ssw0rd", textBox1.Text.Split('.')[0].ToUpper());
                DoNotClose = false;
                Close(); 
            }
            else
            {
                MessageBox.Show("Invalid domain name");
            }
        }

    }
}
