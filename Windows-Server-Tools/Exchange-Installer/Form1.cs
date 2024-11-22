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

        private async void Form1_Load1(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("process_install"))
            {
                string exchangeSetupPath = "\"C:\\Exchange\\Setup.exe\"";

                // Prepare Exchange environment //
                Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareSchema");
                Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareAD /OrganizationName:\"" + DomainName + "\"");
                Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareAllDomains");
                Functions.RunPowerShellScript(exchangeSetupPath + " /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /PrepareDomain:" + DomainName + "." + DomainCOM);

                // Install Mailbox Role //
                Functions.RunPowerShellScript(exchangeSetupPath + " /Mode:Install /Roles:Mailbox /IAcceptExchangeServerLicenseTerms_DiagnosticDataON /InstallWindowsComponents");
                // RESTART //
                //Functions.RunPowerShellScript("shutdown /r /f /t 0");
                //Command.RunCommandHidden("shutdown /r /f /t 0");
                Close();
            }

            if (Environment.GetCommandLineArgs().Contains("exchange"))
            {
                string exchangeSetupPath = "\"C:\\Exchange\\Setup.exe\"";
                Functions.RunPowerShellScript("choco install urlrewrite -y");
                Functions.RunPowerShellScript("choco install vcredist2013 vcredist140 ucma4 googlechrome urlrewrite -y");
                await Chocolatey.ChocolateyDownload("vcredist2013 vcredist140 ucma4 googlechrome urlrewrite");
                Command.RunCommandHidden("schtasks /delete /tn \"" + "Run EXCHANGE\" /f");
                await Task.Delay(2000);
                Functions.DaDhui(true, "process_install");
                Command.RunCommandHidden("shutdown /r /f /t 0");
            }

            

            if (Environment.GetCommandLineArgs().Contains("dadhui"))
            {
                await Functions.DaDhui(true);
                Close();
            }

            if (Environment.GetCommandLineArgs().Contains("pre"))
            {
                Functions.RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS\n pause");
                Functions.RunPowerShellScript("choco install vcredist2013 vcredist140 ucma4 googlechrome urlrewrite -y");
                Close();
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
            Functions.RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS");
            Functions.RunPowerShellScript("choco install vcredist2013 vcredist140 ucma4 googlechrome urlrewrite -y");
            //await Chocolatey.ChocolateyDownload("vcredist2013 vcredist140 ucma4 urlrewrite googlechrome");
            // DA DHUI AND COMPLETE RESTART TASKS //
            await Functions.DaDhui(true);
            // Promote to DC //
            await Functions.InstallActiveDirectoryAndPromoteToDC(textBox1.Text, "P@ssw0rd", textBox1.Text.Split('.')[0].ToUpper());
            Close();
        }

    }
}
