using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
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
            Load += Form1_Load2;
            FormClosing += Form1_FormClosing;
        }

        public class AutoInstall
        {
            public Computer[] Computers { get; set; }
        }

        public class Computer
        {
            public string PCName { get; set; }
            public string DomainName { get; set; }
        }


        private async void Form1_Load2(object sender, EventArgs e)
        {
            
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
        string FourthStepTimeFile = Environment.GetEnvironmentVariable("APPDATA") + "\\FourthStepTimeFile.txt";

        public void RetrieveTimes()
        {
            DateTime firstStepTime = DateTime.MinValue;
            DateTime secondStepTime = DateTime.MinValue;
            DateTime thirdStepTime = DateTime.MinValue;
            DateTime fourthStepTime = DateTime.MinValue;

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

            if(File.Exists(FourthStepTimeFile))
            {
                FourthStepTimeLabel.Text = "Ready to use since: " + fourthStepTime.ToString("T");
                if (firstStepTime != DateTime.MinValue)
                {
                    TimeSpan timeBetweenFirstAndFourth = fourthStepTime - firstStepTime;
                    FullyReadyTimeLabel.Text = "From Start To Finish: " + timeBetweenFirstAndFourth.ToString();
                }
            }
        }



        private async void Form1_Load1(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("process_install"))
            {
                OKButton.Enabled = false;
                textBox1.Enabled = false;
                await Functions.SetStaticIP("8.8.8.8");
                await Functions.ClearPendingReboots();
                // Install UCMA4 again //
                if (!Directory.Exists("C:\\Program Files\\Microsoft UCMA 4.0"))
                {
                    await Functions.RunPowerShellScript("choco install ucma4 --force -y"); 
                }
                await Functions.ClearPendingReboots();
                File.WriteAllText(SecondStepTimeFile,DateTime.Now.ToString("O"));
                RetrieveTimes();
                string exchangeSetupPath = "\"C:\\Exchange\\Setup.exe\"";


                // Prepare Exchange environment //
                bool Schema = false, AD = false, AllDomain = false, Domain = false;
                MainProgressBar.Maximum = 5;

                DomainNameLabel.Text = "Preparing Schema";
                //DomainNameLabel.Text = "";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareSchema");
                MainProgressBar.Value = 1;
                DomainNameLabel.Text = "Preparing AD";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareAD /OrganizationName:\"" + DomainName + "\"");
                MainProgressBar.Value = 2;
                DomainNameLabel.Text = "Preparing All Domains";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareAllDomains");
                MainProgressBar.Value = 3;
                DomainNameLabel.Text = "Preparing The Domain";
                await Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareDomain:" + DomainName + "." + DomainCOM);
                MainProgressBar.Value = 4;
                DomainNameLabel.Text = "INSTALLING EXCHANGE SERVER 2019";
                // Install Mailbox Role //
                await Functions.ClearPendingReboots();
                await Functions.RunPowerShellScript(exchangeSetupPath + " /Mode:Install /Roles:Mailbox /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /InstallWindowsComponents");
                MainProgressBar.Value = 5;
                Chocolatey.ChocolateyDownload("googlechrome");
                // 500 Error //
                DomainNameLabel.Text = "Configuring Mailbox...";
                if (Directory.Exists(@"C:\Program Files\Microsoft\Exchange Server\V15\Bin\"))
                {
                    MainProgressBar.Style = ProgressBarStyle.Marquee;
                    string ExchangeBinDir = Environment.GetEnvironmentVariable("TEMP");
                    string UpdateCASPath = Path.Combine(ExchangeBinDir, "UpdateCas.ps1");
                    string UpdateConfigPath = Path.Combine(ExchangeBinDir, "UpdateConfigFiles.ps1");

                    // Write the script files to the TEMP directory
                    File.WriteAllBytes(UpdateCASPath, Properties.Resources.UpdateCas);
                    File.WriteAllBytes(UpdateConfigPath, Properties.Resources.UpdateConfigFiles);

                    // Run the PowerShell scripts using shorter paths
                    await Functions.RunPowerShellScript(UpdateCASPath);
                    await Functions.RunPowerShellScript(UpdateConfigPath);
                }
                // Save Time Lapse //
                File.WriteAllText(ThirdStepTimeFile, DateTime.Now.ToString("O"));
                RetrieveTimes();
                // DELETE TASK //
                await Command.RunCommandHidden("schtasks /delete /tn \"" + "Run EXCHANGE\" /f");
                //await Task.Delay(2000);
                DoNotClose = false;
                DomainNameLabel.Text = "Exchange Server 2019 installed";
                await Functions.DaDhui(false, "post_install");
                Command.RunCommandHidden("shutdown /r /f /t 0");
            }

            if (Environment.GetCommandLineArgs().Contains("post_install"))
            {
                DoNotClose = false;
                Command.RunCommandHidden("schtasks /delete /tn \"" + "Run EXCHANGE\" /f");
                try
                {
                    Process.Start(@"C:\Program Files\Google\Chrome\Application\chrome.exe", "https://localhost/ecp");
                }
                catch 
                {
                    
                }
                File.WriteAllText(FourthStepTimeFile, DateTime.Now.ToString("O"));
                RetrieveTimes();
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
                DomainNameLabel.Text = "Processing Install";
                await Functions.ChocoInstall("vcredist2013 vcredist140 ucma4 urlrewrite");
                //DomainNameLabel.Text = "Unified Communications API";
                //await Functions.ChocoInstall("");
                //DomainNameLabel.Text = "IIS URL Rewrite";
                //await Functions.ChocoInstall("");
                await Command.RunCommandHidden("schtasks /delete /tn \"" + "Run EXCHANGE\" /f");
                //await Task.Delay(2000);
                Functions.DaDhui(true, "process_install");
                File.WriteAllText(FirstStepTimeFile, DateTime.Now.ToString("O"));
                RetrieveTimes();
                DoNotClose = false;
                Environment.Exit(0);
            }

            if (Environment.GetCommandLineArgs().Contains("config"))
            {
                string ExchangeBinDir = Environment.GetEnvironmentVariable("TEMP");
                string UpdateCASPath = Path.Combine(ExchangeBinDir, "UpdateCas.ps1");
                string UpdateConfigPath = Path.Combine(ExchangeBinDir, "UpdateConfigFiles.ps1");

                // Write the script files to the TEMP directory
                File.WriteAllBytes(UpdateCASPath, Properties.Resources.UpdateCas);
                File.WriteAllBytes(UpdateConfigPath, Properties.Resources.UpdateConfigFiles);

                // Run the PowerShell scripts using shorter paths
                await Functions.RunPowerShellScript(UpdateCASPath);
                await Functions.RunPowerShellScript(UpdateConfigPath);

                DoNotClose = false;
                Close();
            }


            if (Environment.GetCommandLineArgs().Contains("dadhui"))
            {
                // DaDhui The Program //
                await Functions.DaDhui(true);
                DoNotClose = false;
                Close();
            }

            if (Environment.GetCommandLineArgs().Contains("pre"))
            {
                // Install Prerequisites //
                await Functions.RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS\n pause");
                await Functions.RunPowerShellScript("choco install vcredist2013 vcredist140 ucma4 googlechrome urlrewrite -y");
                DoNotClose = false;
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

            AutoInstall silent = new AutoInstall();
            string JSON = new WebClient().DownloadString("http://exchange-install.bigheados.com/api/autoinstall");
            Console.WriteLine(JSON);
            silent = JsonConvert.DeserializeObject<AutoInstall>(JSON);

            foreach (var computer in silent.Computers)
            {
                if (Environment.MachineName == computer.PCName)
                {
                    await ProcessEverything(computer.DomainName);
                    break;
                }
            }
        }

        public static readonly string ExchangeDirectory = "C:\\Exchange";

        private async void button1_Click(object sender, EventArgs e)
        {
            await ProcessEverything(textBox1.Text);
        }

        public async Task ProcessEverything(string DomainNameText)
        {
            if (DomainNameText.Contains(".") && !DomainNameText.Contains(" "))
            {
                textBox1.Enabled = false;
                OKButton.Enabled = false;
                //Visible = false;
                // Install Prequisites //
                DomainNameLabel.Text = "Installing server roles & Prequisites Using Parallel Technology";
                await Functions.InstallPrerequisitesParallel();
                // Enable Detailed Windows Stuff //
                await Functions.RunPowerShellScript("# Enable Verbose Status Messages\r\nWrite-Host \"Enabling verbose status messages...\" -ForegroundColor Green\r\nNew-Item -Path \"HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" -Name \"VerboseStatus\" -Force | Out-Null\r\nSet-ItemProperty -Path \"HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" -Name \"VerboseStatus\" -Value 1\r\n\r\n# Enable Boot Logging\r\nWrite-Host \"Enabling boot logging...\" -ForegroundColor Green\r\nbcdedit /set bootlog Yes\r\n\r\n# Enable Group Policy Operational Logging\r\nWrite-Host \"Enabling Group Policy operational logging...\" -ForegroundColor Green\r\nNew-Item -Path \"HKLM:\\Software\\Microsoft NT\\CurrentVersion\\Diagnostics\" -Force | Out-Null\r\nSet-ItemProperty -Path \"HKLM:\\Software\\Microsoft NT\\CurrentVersion\\Diagnostics\" -Name \"GPSvcDebugLevel\" -Value 0x30002 -Type DWord\r\n\r\n# Confirm Changes\r\nWrite-Host \"All logging settings have been enabled. Please reboot your system to apply the changes.\" -ForegroundColor Cyan");
                // Long Path Support //
                await Functions.RunPowerShellScript("# Enable long path support in the Windows Registry\nWrite-Host \"Enabling long path support in the registry (unattended mode)...\" -ForegroundColor Green\nSet-ItemProperty -Path \"HKLM:\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\" -Name \"LongPathsEnabled\" -Value 1 -Type DWord -ErrorAction Stop\n\n# Check and confirm the change\n$longPathsEnabled = Get-ItemProperty -Path \"HKLM:\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\" -Name \"LongPathsEnabled\"\nif ($longPathsEnabled.LongPathsEnabled -eq 1) {\n    Write-Host \"Long path support has been successfully enabled in the registry.\" -ForegroundColor Cyan\n} else {\n    Write-Host \"Failed to enable long path support in the registry.\" -ForegroundColor Red\n    Exit 1\n}");
                DomainNameLabel.Text = "Promoting to domain";
                // DNS To ItSelf //
                await Functions.SetStaticIP("127.0.0.1");
                // DNS Forward //
                await Functions.RunPowerShellScript("Add-DnsServerForwarder -IPAddress 8.8.8.8");
                await Functions.RunPowerShellScript("Add-DnsServerForwarder -IPAddress 8.8.4.4");
                // Promote to DC //
                await Functions.InstallActiveDirectoryAndPromoteToDC(DomainNameText, "P@ssw0rd", DomainNameText.Split('.')[0].ToUpper());
                DoNotClose = false;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid domain name");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
