﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsefulTools;

namespace SCCM_Installer
{
    public partial class Form1 : Form
    {
        private string logFilePath = @"C:\ConfigMgrSetup.log";
        private FileSystemWatcher fileWatcher;
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

        public static string FQDN = Environment.MachineName + "." + DomainName + "." + DomainCOM;
        public static string DomainSite = DomainName + "." + DomainCOM;
        public static string DatabaseName = "CM_XYZ";

        public async Task InstallSQLServer()
        {
            string SQLPath = "C:\\SQL_Setup\\setup.exe";
            string script = $"Start-Process -FilePath \"{SQLPath}\" -ArgumentList \"/QUIET /ACTION=Install /FEATURES=SQLENGINE /INSTANCENAME=MSSQLSERVER /SQLSVCACCOUNT='NT AUTHORITY\\SYSTEM' /SQLSYSADMINACCOUNTS='BUILTIN\\Administrators' /SAPWD='P@ssw0rd123!' /SECURITYMODE=SQL /IACCEPTSQLSERVERLICENSETERMS\" -Wait";

            
                //await Functions.RunPowerShellScript(script);
                await Task.Run(() =>
                {
                    Process.Start(SQLPath, "/QUIET /ACTION=Install /FEATURES=SQLENGINE /INSTANCENAME=MSSQLSERVER /SQLSVCACCOUNT=\"NT AUTHORITY\\SYSTEM\" /SQLSYSADMINACCOUNTS=\"BUILTIN\\Administrators\" /SAPWD=\"P@ssw0rd123!\" /SECURITYMODE=SQL /IACCEPTSQLSERVERLICENSETERMS").WaitForExit();
                }); 
            
        }
        static void StartSQLServerService(string instanceName)
        {
            string serviceName = instanceName == "MSSQLSERVER" ? "MSSQLSERVER" : $"MSSQL${instanceName}";

            using (ServiceController service = new ServiceController(serviceName))
            {
                if (service.Status != ServiceControllerStatus.Running)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                    Console.WriteLine($"SQL Server service '{serviceName}' started.");
                }
                else
                {
                    Console.WriteLine($"SQL Server service '{serviceName}' is already running.");
                }
            }
        }

        static void CreateDatabase(string instanceName, string databaseName)
        {
            string connectionString = $"Server=localhost\\{instanceName};Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string createDbQuery = $"CREATE DATABASE {databaseName} COLLATE SQL_Latin1_General_CP1_CI_AS;";
                using (SqlCommand command = new SqlCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Database '{databaseName}' created successfully.");
                }
            }
        }

        public async Task SQLDealer()
        {
            string StartSQL_Script = "Start-Service -Name \"MSSQLSERVER\"";

            //await Task.Delay(-1);

            string EnableTCP_Script = @"# Get access to SqlWmiManagement DLL on the machine with SQL
# we are on, which is where SQL Server was installed.
# Note: This is installed in the GAC by SQL Server Setup.

[System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SqlWmiManagement')

# Instantiate a ManagedComputer object that exposes primitives to control the
# Installation of SQL Server on this machine.

$wmi = New-Object 'Microsoft.SqlServer.Management.Smo.Wmi.ManagedComputer' localhost

# Enable the TCP protocol on the default instance. If the instance is named,
# replace MSSQLSERVER with the instance name in the following line.

$tcp = $wmi.ServerInstances['MSSQLSERVER'].ServerProtocols['Tcp']
$tcp.IsEnabled = $true
$tcp.Alter()

# You need to restart SQL Server for the change to persist
# -Force takes care of any dependent services, like SQL Agent.
# Note: If the instance is named, replace MSSQLSERVER with MSSQL$ followed by
# the name of the instance (e.g., MSSQL$MYINSTANCE)

Restart-Service -Name MSSQLSERVER -Force";

            string CreateDatabaseScript = @"# Variables
Set-ExecutionPolicy Bypass -Scope LocalMachine

$SQLInstance = ""MSSQLSERVER""  # Replace with your instance name if different
$SCCMDBName = ""CM_XYZ""  # Desired SCCM database name

# Load the SQL Server module (use SQLPS if SqlServer is unavailable)
Import-Module SqlServer -ErrorAction SilentlyContinue
if (-not (Get-Module -Name SqlServer)) {
    Import-Module SQLPS -DisableNameChecking
}

# Enable TCP/IP for SQL Server
Write-Host ""Enabling TCP/IP for SQL Server...""
Invoke-Sqlcmd -Query ""
DECLARE @TcpEnabled INT;
SELECT @TcpEnabled = protocol_id FROM sys.dm_server_services WHERE service_name = 'SQL Server ($SQLInstance)';
IF (@TcpEnabled IS NOT NULL)
BEGIN
    EXEC xp_cmdshell 'netsh int ipv4 show dynamicport tcp';
    EXEC xp_cmdshell 'netsh int ipv4 set dynamicport tcp start=49152 num=16384';
END;
""

# Restart SQL Server Service
Write-Host ""Restarting SQL Server service to apply changes...""
Restart-Service -Name ""MSSQLSERVER""

# Create SCCM Database
Invoke-Sqlcmd -Query ""CREATE DATABASE [$SCCMDBName] COLLATE SQL_Latin1_General_CP1_CI_AS;"" -ServerInstance $SQLInstance
Start-Sleep -Seconds 5.5

Write-Host ""TCP/IP has been enabled and database [$SCCMDBName] created.""
".Replace("MSSQLSERVER",Environment.MachineName);
            File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\SQLScript.ps1",CreateDatabaseScript);
            await Functions.RunExchangePowerShellScript(StartSQL_Script);
            // Wait for sql server //
            //await Functions.WaitForServiceAsync("MSSQLSERVER");
            string myConnectionString = "Server=localhost;Database=master;User Id=sa;Password=P@ssw0rd123!;";
            try
            {
                using (var connection = new SqlConnection(myConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "CREATE DATABASE CM_XYZ";
                    command.ExecuteNonQuery();
                }

            }
            catch 
            {

            }
            await Functions.RunPowerShellScript(CreateDatabaseScript);
            ExecuteScript(File.ReadAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\SQLScript.ps1"));
            await Functions.RunPowerShellScript(@"Invoke-Sqlcmd -Query ""CREATE DATABASE [hui] COLLATE SQL_Latin1_General_CP1_CI_AS;"" -ServerInstance " + Environment.MachineName);
            await Functions.RunExchangePowerShellScript(EnableTCP_Script);
        }

        public void ExecuteScript(string pathToScript)
        {
            var scriptArguments = "-ExecutionPolicy Bypass -File \"" + pathToScript + "\"";
            var processStartInfo = new ProcessStartInfo("powershell.exe", scriptArguments);

            var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
        }

        public async Task InstallADKPE()
        {
            
            
                new WebClient().DownloadFile("http://exchange-install.bigheados.com/files/adkwinpesetup.exe", Environment.GetEnvironmentVariable("APPDATA") + "\\ADKPE.exe");
                await Task.Run(() =>
                {
                    Process.Start(Environment.GetEnvironmentVariable("APPDATA") + "\\ADKPE.exe", "/quiet").WaitForExit();
                }); 
            
        }
        public async Task InstallSCCM()
        {
            string SCCMPath = "C:\\SCCM\\cd.retail.LN\\SMSSETUP\\BIN\\X64\\setupwpf.exe";
            Directory.CreateDirectory("C:\\Sources\\Redist");



            // Initialize the FileSystemWatcher
            fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(logFilePath),
                Filter = Path.GetFileName(logFilePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            // Event handler for changes
            fileWatcher.Changed += OnLogFileChanged;
            fileWatcher.EnableRaisingEvents = true;

            // Load the initial contents
            LoadLogFile();



            // THE MAGIC BEGINS //
            File.WriteAllText("C:\\Thing.ini",SCCM_Config.GetConfigScript(FQDN));
            await Task.Factory.StartNew(() =>
            {
                Process.Start(SCCMPath, "/SCRIPT C:\\Thing.ini").WaitForExit();
            });
        }

        private void LoadLogFile()
        {
            if (File.Exists(logFilePath))
            {
                try
                {
                    var lines = File.ReadLines(logFilePath)
                                .Reverse()
                                .Take(20)
                                .Reverse();
                    MainTextBox.Invoke((MethodInvoker)(() =>
                    {
                        try
                        {
                            MainTextBox.Text = string.Join(Environment.NewLine, lines);
                            AutoScroll();
                        }
                        catch 
                        {

                        }
                    }));
                }
                catch 
                {

                }
            }
        }

        private void AutoScroll()
        {
            MainTextBox.SelectionStart = MainTextBox.Text.Length;
            MainTextBox.ScrollToCaret();
        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            // Delay slightly to allow file to finish writing
            System.Threading.Thread.Sleep(100);
            LoadLogFile();
        }

        bool QuickInstall => File.Exists("C:\\quick.txt");

        public async Task ProcessInstall(bool NoDomain = false)
        {
            EnableStuff = false;
            MainTextBox.Text += "Starting install " + DateTime.Now.ToString("F");
            MainTextBox.Text += "\nInstalling windows features " + DateTime.Now.ToString("F");
            if (!QuickInstall)
            {
                await Functions.RunPowerShellScript("Install-WindowsFeature -Name Web-Server, Web-Windows-Auth, Web-Asp-Net45, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Mgmt-Console, NET-Framework-Features, NET-Framework-Core, BITS, RDC, RSAT-ADDS -IncludeManagementTools");
                await Functions.RunPowerShellScript("Install-WindowsFeature -Name UpdateServices -IncludeManagementTools"); 
            }
            MainTextBox.Text += "\nInstalling prerequisites" + DateTime.Now.ToString("F");
            await Functions.ChocoInstall("windows-adk sql-server-management-studio sqlserver-odbcdriver vscode");
            // Install Windows ADK PE //
            MainTextBox.Text += "\nInstalling Windows ADK" + DateTime.Now.ToString("F");
            await InstallADKPE();
            // Install SQL Server First //
            MainTextBox.Text += "\nInstalling SQL Server" + DateTime.Now.ToString("F");
            await InstallSQLServer();
            // Configure SQL Database //
            MainTextBox.Text += "\nConfiguring SQL" + DateTime.Now.ToString("F");
            await SQLDealer();
            // DA DHUI //
            await Functions.DaDhui(true, "install");
            if (!NoDomain)
            {
                MainTextBox.Text += "\nPromoting to Domain" + DateTime.Now.ToString("F");
                await Functions.InstallActiveDirectoryAndPromoteToDC(textBox1.Text, "P@ssw0rd", textBox1.Text.Split('.')[0].ToUpper()); 
            }
            else
            {
                Command.RunCommandHidden("shutdown /r /f /t 0");
            }
            EnableStuff = true;
        }
    }
}
