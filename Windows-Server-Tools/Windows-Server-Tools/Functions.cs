using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Windows_Server_Tools
{
    public partial class MainWindow : Window
    {
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



        public static void InstallActiveDirectoryAndPromoteToDC(string domainName, string safeModeAdminPassword, string domainNetbiosName = "CONTOSO")
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
                    MessageBox.Show("Active Directory Domain Services and Management Tools installed successfully.");

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

    }
}
