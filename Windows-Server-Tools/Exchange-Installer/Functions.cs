using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsefulTools;
using Task = System.Threading.Tasks.Task;

namespace Exchange_Installer
{
    public class Functions
    {
        public static async Task SolveWindowsTasks()
        {
            await Command.RunCommandHidden("@echo off\r\n\r\n:: Disable all firewalls\r\nnetsh advfirewall set allprofiles state off\r\n\r\n:: Disable sleep and monitor off settings\r\npowercfg -change -standby-timeout-ac 0\r\npowercfg -change -monitor-timeout-ac 0\r\npowercfg -change -disk-timeout-ac 0\r\npowercfg -change -hibernate-timeout-ac 0\r\n\r\n:: Enable Remote Desktop without Network Level Authentication\r\nreg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\" /v fDenyTSConnections /t REG_DWORD /d 0 /f\r\nreg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp\" /v UserAuthentication /t REG_DWORD /d 0 /f\r\n\r\n:: Disable Internet Explorer Enhanced Security Configuration (IE ESC)\r\nreg add \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 0 /f\r\nreg add \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 0 /f\r\ntaskkill /F /IM explorer.exe\r\nstart explorer.exe\r\n\r\n:: Disable Windows SmartScreen\r\nreg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v EnableSmartScreen /t REG_DWORD /d 0 /f\r\n\r\n:: Disable Windows Defender\r\npowershell -Command \"Set-MpPreference -DisableRealtimeMonitoring $true\"\r\n\r\necho All tasks completed.");

            RunPowerShellScript("# Install DNS Server\r\nInstall-WindowsFeature -Name DNS -IncludeManagementTools\r\n\r\n# Install DHCP Server\r\nInstall-WindowsFeature -Name DHCP -IncludeManagementTools\r\n\r\n# Post-installation configuration for DHCP\r\n# Authorize the DHCP server in Active Directory\r\nAdd-DhcpServerInDC -DnsName (Get-ComputerInfo).CsName -IPAddress (Get-NetIPAddress -AddressFamily IPv4).IPAddress");
            //RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Lgcy-Mgmt-Console, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS");
            await Chocolatey.InstallChocolatey();

            // STATIC IP //
            Command.RunCommandHidden("@echo off\r\nSETLOCAL ENABLEDELAYEDEXPANSION\r\n\r\n:: Get the current IP address, Subnet Mask, and Default Gateway\r\nFOR /F \"tokens=2 delims=:\" %%a in ('ipconfig ^| findstr /C:\"IPv4 Address\"') do set currentIP=%%a\r\nFOR /F \"tokens=2 delims=:\" %%b in ('ipconfig ^| findstr /C:\"Subnet Mask\"') do set subnetMask=%%b\r\nFOR /F \"tokens=2 delims=:\" %%c in ('ipconfig ^| findstr /C:\"Default Gateway\"') do set defaultGateway=%%c\r\n\r\n:: Remove leading spaces\r\nSET currentIP=%currentIP:~1%\r\nSET subnetMask=%subnetMask:~1%\r\nSET defaultGateway=%defaultGateway:~1%\r\n\r\n:: Set the static IP address (using the current IP)\r\nnetsh interface ip set address \"Ethernet0\" static %currentIP% %subnetMask% %defaultGateway%\r\n\r\n:: Set the DNS server to the default gateway\r\nnetsh interface ip set dns \"Ethernet0\" static 8.8.8.8\r\n\r\necho New IP configuration:\r\necho IP Address: %currentIP%\r\necho Subnet Mask: %subnetMask%\r\necho Default Gateway: %defaultGateway%\r\necho DNS Server: %defaultGateway%\r\n\r\nENDLOCAL\r\n");
        }

        private static async Task ChocoInstall(string SOFTWARE)
        {
            await Command.RunCommandHidden("\"C:\\ProgramData\\chocolatey\\bin\\choco.exe\" install " + SOFTWARE + " -y --ignore-checksums");
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


        public static async Task RunPowerShellScript(string script)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
            process.StartInfo.Arguments = $"-Command \"{script}\"";
            process.StartInfo.RedirectStandardOutput = false; // Allows the window to show
            process.StartInfo.UseShellExecute = true; // This will show the PowerShell window
            process.StartInfo.CreateNoWindow = false; // Do not create a hidden window
            process.StartInfo.Verb = "runas"; // This ensures the process starts with administrative privileges

            // Start the AD DS installation process
            await Task.Factory.StartNew(() => {
                process.Start();
                process.WaitForExit();
            });
        }

        public static async System.Threading.Tasks.Task DaDhui(bool RestartAfter = false,string CustomArg = "exchange")
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string targetPath = Path.Combine(appDataPath, CustomArg + ".exe");

            try
            {
                // Step 1: Copy the program to AppData
                string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                File.Copy(currentPath, targetPath, true);

                // Step 2: Create a Task Scheduler entry
                //CreateTaskSchedulerEntry(targetPath, "exchange");
                CreateSimpsonsTask(targetPath,CustomArg,RestartAfter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static async Task InstallPrerequisitesParallel()
        {
            // Install Windows features in parallel
            var featuresTask = Task.Run(async () =>
            {
                await Functions.RunPowerShellScript(
                    "Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS"
                );
            });

            // Install Chocolatey packages in parallel
            var chocolateyTask = Task.Run(async () =>
            {
                await Task.Delay(30000);
                string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(currentPath, "exchange");
            });

            // Wait for both tasks to complete
            await Task.WhenAll(featuresTask, chocolateyTask);
        }

        public static async Task ClearPendingReboots()
        {
            // Clear Pending Reboots //
            await Functions.RunPowerShellScript("Remove-ItemProperty -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Session Manager' -Name 'PendingFileRenameOperations' -Force");
        }

        public static void CreateSimpsonsTask(string programpath, string arguments,bool RestartAfter = false)
        {
            // Define the task details
            string taskName = "Run EXCHANGE";
            //string arguments = "simpsons";

            using (TaskService ts = new TaskService())
            {
                // Create a new task definition
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Runs Setup.exe with 'exchange' arguments after login.";
                td.Principal.UserId = Environment.UserDomainName + "\\" + Environment.UserName;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest; // Run with highest privileges

                // Set the trigger to run at logon
                td.Triggers.Add(new LogonTrigger());

                // Create the action to run the executable with arguments
                td.Actions.Add(new ExecAction(programpath, arguments, null));

                // Action to delete the task after it finishes
                string deleteTaskScript = $@"
                    schtasks /delete /tn ""{taskName}"" /f
                ";
                td.Actions.Add(new ExecAction("cmd.exe", $"/c {deleteTaskScript}", null));
                if (RestartAfter)
                {
                    td.Actions.Add(new ExecAction("cmd.exe", "shutdown /r /f /t 0",null));
                }

                // Register the task in the Task Scheduler
                ts.RootFolder.RegisterTaskDefinition(taskName, td);
            }
        }

        static void CreateTaskSchedulerEntry(string programPath, string arguments)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Run MyProgram.exe at logon";

                    // Set the task to run at logon
                    td.Triggers.Add(new LogonTrigger());

                    // Add the program action with arguments
                    td.Actions.Add(new ExecAction(programPath, arguments, null));

                    // Set task to run with highest privileges
                    td.Principal.RunLevel = TaskRunLevel.Highest;

                    // Ensure task deletes itself after running once
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(5);
                    td.Settings.AllowDemandStart = true;
                    td.Settings.DeleteExpiredTaskAfter = TimeSpan.FromSeconds(10);

                    // Register the task
                    ts.RootFolder.RegisterTaskDefinition("MyProgramTask", td);
                }

                Console.WriteLine("Task Scheduler entry created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create task: {ex.Message}");
            }
        }
    }
}
