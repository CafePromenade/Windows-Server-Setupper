using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UsefulTools;
using Command = UsefulTools.Command;

namespace Windows_Server_Tools
{
    public partial class MainWindow : Window
    {

        public static async Task SolveWindowsTasks()
        {
            await Command.RunCommandHidden("@echo off\r\n\r\n:: Disable all firewalls\r\nnetsh advfirewall set allprofiles state off\r\n\r\n:: Disable sleep and monitor off settings\r\npowercfg -change -standby-timeout-ac 0\r\npowercfg -change -monitor-timeout-ac 0\r\npowercfg -change -disk-timeout-ac 0\r\npowercfg -change -hibernate-timeout-ac 0\r\n\r\n:: Enable Remote Desktop without Network Level Authentication\r\nreg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\" /v fDenyTSConnections /t REG_DWORD /d 0 /f\r\nreg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp\" /v UserAuthentication /t REG_DWORD /d 0 /f\r\n\r\n:: Disable Internet Explorer Enhanced Security Configuration (IE ESC)\r\nreg add \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 0 /f\r\nreg add \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 0 /f\r\ntaskkill /F /IM explorer.exe\r\nstart explorer.exe\r\n\r\n:: Disable Windows SmartScreen\r\nreg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v EnableSmartScreen /t REG_DWORD /d 0 /f\r\n\r\n:: Disable Windows Defender\r\npowershell -Command \"Set-MpPreference -DisableRealtimeMonitoring $true\"\r\n\r\necho All tasks completed.");

            RunPowerShellScript("# Install DNS Server\r\nInstall-WindowsFeature -Name DNS -IncludeManagementTools\r\n\r\n# Install DHCP Server\r\nInstall-WindowsFeature -Name DHCP -IncludeManagementTools\r\n\r\n# Post-installation configuration for DHCP\r\n# Authorize the DHCP server in Active Directory\r\nAdd-DhcpServerInDC -DnsName (Get-ComputerInfo).CsName -IPAddress (Get-NetIPAddress -AddressFamily IPv4).IPAddress");

            await Chocolatey.InstallChocolatey();

            ChocoInstall("filezilla winscp vscode googlechrome veracrypt firefox opera python nodejs dotnetfx");
        }
        public static void SetStaticIp(string adapterName, string ipAddress, string subnetMask)
        {
            try
            {
                // Get the default gateway
                string defaultGateway = GetDefaultGateway();

                if (string.IsNullOrEmpty(defaultGateway))
                {
                    Console.WriteLine("No default gateway found.");
                    return;
                }

                // Prepare the netsh commands
                string setIpCommand = $"interface ip set address name=\"{adapterName}\" static {ipAddress} {subnetMask} {defaultGateway}";
                string setDnsCommand = $"interface ip set dns name=\"{adapterName}\" static {defaultGateway}";

                // Execute the commands
                ExecuteCommand(setIpCommand);
                ExecuteCommand(setDnsCommand);

                Console.WriteLine("IP address and DNS set successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private static string GetDefaultGateway()
        {
            var gateways = NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(ni => ni.GetIPProperties().GatewayAddresses)
                .Select(g => g.Address.ToString())
                .ToList();

            return gateways.FirstOrDefault();
        }

        private static void ExecuteCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"Error: {error}");
                }
                Console.WriteLine(output);
            }
        }



        public static async Task InstallActiveDirectoryAndPromoteToDC(string domainName, string safeModeAdminPassword, string domainNetbiosName = "CONTOSO")
        {
            try
            {
                // First script to install AD DS and Management Tools
                string installADDSCommand = @"
            Install-WindowsFeature -Name AD-Domain-Services -IncludeManagementTools;
        ";

                // Second script to promote the server to a Domain Controller
                string promoteCommand = $@"
            Import-Module ADDSDeployment;
            Install-ADDSForest `
            -CreateDnsDelegation:$false `
            -DatabasePath ""C:\Windows\NTDS"" `
            -DomainMode ""WinThreshold"" `
            -DomainName ""{domainName}"" `
            -DomainNetbiosName ""{domainNetbiosName}"" `
            -ForestMode ""WinThreshold"" `
            -InstallDns:$true `
            -LogPath ""C:\Windows\NTDS"" `
            -NoRebootOnCompletion:$false `
            -SysvolPath ""C:\Windows\SYSVOL"" `
            -SafeModeAdministratorPassword (ConvertTo-SecureString ""{safeModeAdminPassword}"" -AsPlainText -Force) `
            -Force:$true
        ";

                // Create a new process to run PowerShell to install AD DS
                Process process = new Process();
                process.StartInfo.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{installADDSCommand}\"";
                process.StartInfo.RedirectStandardOutput = false; // Allows the window to show
                process.StartInfo.UseShellExecute = true; // This will show the PowerShell window
                process.StartInfo.CreateNoWindow = false; // Do not create a hidden window
                process.StartInfo.Verb = "runas"; // This ensures the process starts with administrative privileges

                // Start the AD DS installation process
                process.Start();
                process.WaitForExit();

                // Check if AD DS installation was successful
                if (true)
                {
                    //MessageBox.Show("Active Directory Domain Services and Management Tools installed successfully.");

                    File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\Domain.txt", domainName);
                    // Now promote the server to a Domain Controller
                    process.StartInfo.Verb = "runas"; // This ensures the process starts with administrative privileges
                    process.StartInfo.Arguments = $"-NoExit -Command \"{promoteCommand}\"";
                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show("Server successfully promoted to Domain Controller.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to promote server to Domain Controller.");
                    }
                }
                else
                {
                    MessageBox.Show("Failed to install Active Directory Domain Services.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }


        public static void RunPowerShellScript(string script)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
            process.StartInfo.Arguments = $"-Command \"{script}\"";
            process.StartInfo.RedirectStandardOutput = false; // Allows the window to show
            process.StartInfo.UseShellExecute = true; // This will show the PowerShell window
            process.StartInfo.CreateNoWindow = false; // Do not create a hidden window
            process.StartInfo.Verb = "runas"; // This ensures the process starts with administrative privileges

            // Start the AD DS installation process
            process.Start();
            process.WaitForExit();
        }
    }
}
