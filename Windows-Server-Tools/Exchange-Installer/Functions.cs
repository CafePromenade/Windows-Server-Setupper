﻿using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
            //await Chocolatey.InstallChocolatey();
            await RunPowerShellScript("Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))");

            await Command.RunCommandHidden("@echo off\r\n\r\n:: Disable all firewalls\r\nnetsh advfirewall set allprofiles state off\r\n\r\n:: Disable sleep and monitor off settings\r\npowercfg -change -standby-timeout-ac 0\r\npowercfg -change -monitor-timeout-ac 0\r\npowercfg -change -disk-timeout-ac 0\r\npowercfg -change -hibernate-timeout-ac 0\r\n\r\n:: Enable Remote Desktop without Network Level Authentication\r\nreg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\" /v fDenyTSConnections /t REG_DWORD /d 0 /f\r\nreg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp\" /v UserAuthentication /t REG_DWORD /d 0 /f\r\n\r\n:: Disable Internet Explorer Enhanced Security Configuration (IE ESC)\r\nreg add \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A7-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 0 /f\r\nreg add \"HKLM\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components\\{A509B1A8-37EF-4b3f-8CFC-4F3A74704073}\" /v IsInstalled /t REG_DWORD /d 0 /f\r\ntaskkill /F /IM explorer.exe\r\nstart explorer.exe\r\n\r\n:: Disable Windows SmartScreen\r\nreg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v EnableSmartScreen /t REG_DWORD /d 0 /f\r\n\r\n:: Disable Windows Defender\r\npowershell -Command \"Set-MpPreference -DisableRealtimeMonitoring $true\"\r\n\r\necho All tasks completed.");

            RunPowerShellScript("# Install DNS Server\r\nInstall-WindowsFeature -Name DNS -IncludeManagementTools\r\n\r\n# Install DHCP Server\r\nInstall-WindowsFeature -Name DHCP -IncludeManagementTools\r\n\r\n# Post-installation configuration for DHCP\r\n# Authorize the DHCP server in Active Directory\r\nAdd-DhcpServerInDC -DnsName (Get-ComputerInfo).CsName -IPAddress (Get-NetIPAddress -AddressFamily IPv4).IPAddress");
            //RunPowerShellScript("Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Lgcy-Mgmt-Console, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS");

            // STATIC IP //
            Command.RunCommandHidden("@echo off\r\nSETLOCAL ENABLEDELAYEDEXPANSION\r\n\r\n:: Get the current IP address, Subnet Mask, and Default Gateway\r\nFOR /F \"tokens=2 delims=:\" %%a in ('ipconfig ^| findstr /C:\"IPv4 Address\"') do set currentIP=%%a\r\nFOR /F \"tokens=2 delims=:\" %%b in ('ipconfig ^| findstr /C:\"Subnet Mask\"') do set subnetMask=%%b\r\nFOR /F \"tokens=2 delims=:\" %%c in ('ipconfig ^| findstr /C:\"Default Gateway\"') do set defaultGateway=%%c\r\n\r\n:: Remove leading spaces\r\nSET currentIP=%currentIP:~1%\r\nSET subnetMask=%subnetMask:~1%\r\nSET defaultGateway=%defaultGateway:~1%\r\n\r\n:: Set the static IP address (using the current IP)\r\nnetsh interface ip set address \"Ethernet0\" static %currentIP% %subnetMask% %defaultGateway%\r\n\r\n:: Set the DNS server to the default gateway\r\nnetsh interface ip set dns \"Ethernet0\" static 8.8.8.8\r\n\r\necho New IP configuration:\r\necho IP Address: %currentIP%\r\necho Subnet Mask: %subnetMask%\r\necho Default Gateway: %defaultGateway%\r\necho DNS Server: %defaultGateway%\r\n\r\nENDLOCAL\r\n");
        }

        public static async Task SetStaticIP(string DNS)
        {
            Command.RunCommandHidden("@echo off\r\nSETLOCAL ENABLEDELAYEDEXPANSION\r\n\r\n:: Get the current IP address, Subnet Mask, and Default Gateway\r\nFOR /F \"tokens=2 delims=:\" %%a in ('ipconfig ^| findstr /C:\"IPv4 Address\"') do set currentIP=%%a\r\nFOR /F \"tokens=2 delims=:\" %%b in ('ipconfig ^| findstr /C:\"Subnet Mask\"') do set subnetMask=%%b\r\nFOR /F \"tokens=2 delims=:\" %%c in ('ipconfig ^| findstr /C:\"Default Gateway\"') do set defaultGateway=%%c\r\n\r\n:: Remove leading spaces\r\nSET currentIP=%currentIP:~1%\r\nSET subnetMask=%subnetMask:~1%\r\nSET defaultGateway=%defaultGateway:~1%\r\n\r\n:: Set the static IP address (using the current IP)\r\nnetsh interface ip set address \"Ethernet0\" static %currentIP% %subnetMask% %defaultGateway%\r\n\r\n:: Set the DNS server to the default gateway\r\nnetsh interface ip set dns \"Ethernet0\" static " + DNS + "\r\n\r\necho New IP configuration:\r\necho IP Address: %currentIP%\r\necho Subnet Mask: %subnetMask%\r\necho Default Gateway: %defaultGateway%\r\necho DNS Server: %defaultGateway%\r\n\r\nENDLOCAL\r\n");
        }

        public static async Task ChocoInstall(string SOFTWARE)
        {
            string ChocoPath = "C:\\ProgramData\\chocolatey\\bin\\choco.exe";
            await RunPowerShellScript(ChocoPath + " install " + SOFTWARE + " -y --ignore-checksums");
            //await Task.Run(() =>
            //{
            //    Process.Start(ChocoPath, "install " + SOFTWARE + " -y --ignore-checksums").WaitForExit();
            //});
            //await Command.RunCommandHidden("\"C:\\ProgramData\\chocolatey\\bin\\choco.exe\" install " + SOFTWARE + " -y --ignore-checksums");
        }

        public static async Task DownloadAndInstallChromeTemplates()
        {
            try
            {
                string downloadUrl = "https://dl.google.com/dl/edgedl/chrome/policy/policy_templates.zip";
                string destinationZip = Path.Combine(Path.GetTempPath(), "policy_templates.zip");
                string extractPath = Path.Combine(Path.GetTempPath(), "policy_templates");

                Console.WriteLine("Downloading Chrome ADMX templates...");
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(downloadUrl, destinationZip);
                }
                Console.WriteLine("Download completed successfully.");

                Console.WriteLine("Extracting Chrome ADMX templates...");
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                System.IO.Compression.ZipFile.ExtractToDirectory(destinationZip, extractPath);

                Console.WriteLine("Installing Chrome ADMX templates...");
                string policyDefinitionsPath = @"C:\Windows\PolicyDefinitions";
                string languagePath = "en-US"; // Adjust for your language
                string sourceAdmx = Path.Combine(extractPath, "windows\\admx\\chrome.admx");
                string sourceAdml = Path.Combine(extractPath, $"windows\\admx\\{languagePath}\\chrome.adml");
                string destinationAdml = Path.Combine(policyDefinitionsPath, languagePath);

                // Ensure directories exist
                if (!Directory.Exists(policyDefinitionsPath))
                {
                    Directory.CreateDirectory(policyDefinitionsPath);
                }
                if (!Directory.Exists(destinationAdml))
                {
                    Directory.CreateDirectory(destinationAdml);
                }

                // Copy ADMX and ADML files
                File.Copy(sourceAdmx, Path.Combine(policyDefinitionsPath, "chrome.admx"), true);
                File.Copy(sourceAdml, Path.Combine(destinationAdml, "chrome.adml"), true);

                Console.WriteLine("Templates installed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while installing templates: " + ex.Message);
            }
        }

        public static async Task ConfigureChromePolicies()
        {
            string script = @"
        # Set paths
        $chromePolicyPath = 'HKLM:\SOFTWARE\Policies\Google\Chrome'
        $managedBookmarksPath = 'HKLM:\SOFTWARE\Policies\Google\Chrome\ManagedBookmarks'

        # Ensure the main Chrome policy registry path exists
        if (-Not (Test-Path $chromePolicyPath)) {
            Write-Host 'Creating Chrome policy registry path...' -ForegroundColor Green
            New-Item -Path $chromePolicyPath -Force | Out-Null
        }

        # Set policy to disable default startup page
        Write-Host 'Setting ''RestoreOnStartup'' to 4...' -ForegroundColor Green
        Set-ItemProperty -Path $chromePolicyPath -Name 'RestoreOnStartup' -Value 4 -Force

        # Clear existing startup URLs
        Write-Host 'Clearing ''RestoreOnStartupURLs''...' -ForegroundColor Green
        if (Test-Path -Path $chromePolicyPath) {
            Remove-ItemProperty -Path $chromePolicyPath -Name 'RestoreOnStartupURLs' -ErrorAction SilentlyContinue
        }

        # Ensure the ManagedBookmarks key exists
        if (-Not (Test-Path $managedBookmarksPath)) {
            Write-Host 'Creating ManagedBookmarks registry path...' -ForegroundColor Green
            New-Item -Path $managedBookmarksPath -Force | Out-Null
        }

        # Define bookmarks to add
        $bookmarks = @(
            @{
                Name = 'Exchange Admin Centre'
                URL  = 'https://localhost/ecp'
            },
            @{
                Name = 'Outlook'
                URL  = 'https://localhost/owa'
            }
        )

        # Add bookmarks under the ManagedBookmarks registry
        Write-Host 'Adding Managed Bookmarks...' -ForegroundColor Green
        $index = 0
        foreach ($bookmark in $bookmarks) {
            $bookmarkKeyPath = Join-Path $managedBookmarksPath $index
            if (-Not (Test-Path $bookmarkKeyPath)) {
                New-Item -Path $bookmarkKeyPath -Force | Out-Null
            }
            Set-ItemProperty -Path $bookmarkKeyPath -Name 'Name' -Value $bookmark.Name -Force
            Set-ItemProperty -Path $bookmarkKeyPath -Name 'URL' -Value $bookmark.URL -Force
            Write-Host 'Bookmark added: $($bookmark.Name) -> $($bookmark.URL)' -ForegroundColor Cyan
            $index++
        }

        # Set Chrome as the default browser
        Write-Host 'Setting Chrome as the default browser...' -ForegroundColor Green
        $chromePath = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
        if (Test-Path $chromePath) {
            $assocXml = @'
<?xml version=""1.0"" encoding=""UTF-8""?>
<DefaultAssociations xmlns=""http://schemas.microsoft.com/2009/11/DefaultAssociations"">
    <Association Identifier=""http"" ProgId=""ChromeHTML"" ApplicationName=""Google Chrome""/>
    <Association Identifier=""https"" ProgId=""ChromeHTML"" ApplicationName=""Google Chrome""/>
    <Association Identifier="".htm"" ProgId=""ChromeHTML"" ApplicationName=""Google Chrome""/>
    <Association Identifier="".html"" ProgId=""ChromeHTML"" ApplicationName=""Google Chrome""/>
</DefaultAssociations>
'@

            $xmlPath = Join-Path $env:TEMP 'ChromeDefaultAssociations.xml'
            $assocXml | Out-File -FilePath $xmlPath -Encoding UTF8

            # Apply the file associations
            Start-Process 'dism.exe' -ArgumentList '/online', '/import-defaultappassociations:$xmlPath' -Wait -NoNewWindow
            Write-Host 'Chrome set as the default browser successfully.' -ForegroundColor Cyan
        } else {
            Write-Host 'Chrome executable not found. Make sure Chrome is installed at $chromePath' -ForegroundColor Red
        }

        # Refresh Group Policy
        Write-Host 'Refreshing Group Policy...' -ForegroundColor Green
        gpupdate /force

        Write-Host 'Google Chrome policies, bookmarks, and default browser settings configured successfully.' -ForegroundColor Cyan
        ";

            await RunPowerShellScript(script);
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
            process.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"";
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

        public static async Task ConfigureSendConnectors(string fqdn)
        {
            string serverName = Environment.MachineName; // Dynamically fetch the server name

            // Provide credentials for Gmail and Outlook
            string gmailUsername = "Administrator"; // Replace with Gmail username
            string gmailPassword = "P@ssw0rd";      // Replace with Gmail password
            string outlookUsername = "Administrator"; // Replace with Outlook username
            string outlookPassword = "P@ssw0rd";      // Replace with Outlook password

            // PowerShell script to configure Gmail and Outlook Send Connectors with credentials
            string script = $@"
    # Convert credentials to a PSCredential object
    $gmailSecurePassword = ConvertTo-SecureString '{gmailPassword}' -AsPlainText -Force;
    $gmailCredential = New-Object System.Management.Automation.PSCredential('{gmailUsername}', $gmailSecurePassword);

    $outlookSecurePassword = ConvertTo-SecureString '{outlookPassword}' -AsPlainText -Force;
    $outlookCredential = New-Object System.Management.Automation.PSCredential('{outlookUsername}', $outlookSecurePassword);

    # Add Gmail Send Connector
    if (!(Get-SendConnector | Where-Object {{ $_.Name -eq 'Gmail Send Connector' }})) {{
        New-SendConnector -Name 'Gmail Send Connector' -Usage Internet -AddressSpaces 'smtp.gmail.com' -SmartHosts 'smtp.gmail.com' `
            -SourceTransportServers '{serverName}' -Port 587 -RequireTLS $true -AuthenticationCredential $gmailCredential;
        Write-Output 'Gmail Send Connector created successfully.';
    }} else {{
        Write-Output 'Gmail Send Connector already exists.';
    }}

    # Add Outlook Send Connector
    if (!(Get-SendConnector | Where-Object {{ $_.Name -eq 'Outlook Send Connector' }})) {{
        New-SendConnector -Name 'Outlook Send Connector' -Usage Internet -AddressSpaces 'smtp.office365.com' -SmartHosts 'smtp.office365.com' `
            -SourceTransportServers '{serverName}' -Port 587 -RequireTLS $true -AuthenticationCredential $outlookCredential;
        Write-Output 'Outlook Send Connector created successfully.';
    }} else {{
        Write-Output 'Outlook Send Connector already exists.';
    }}
";

            // Run the PowerShell script
            await RunExchangePowerShellScript(script);
        }



        public static async Task RunExchangePowerShellScript(string script)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

            // Load the Exchange Management Shell module and execute the script
            string exchangeInitScript = @"
        Add-PSSnapin Microsoft.Exchange.Management.PowerShell.SnapIn -ErrorAction SilentlyContinue;
        " + script;

            process.StartInfo.Arguments = $"-Command \"{exchangeInitScript}\"";
            process.StartInfo.RedirectStandardOutput = true; // Capture output
            process.StartInfo.RedirectStandardError = true; // Capture errors
            process.StartInfo.UseShellExecute = false; // Required for redirection
            process.StartInfo.CreateNoWindow = false; // Show the PowerShell window
            process.StartInfo.Verb = "runas"; // Run as administrator

            // Start the process
            await Task.Factory.StartNew(() =>
            {
                process.Start();

                // Optionally, read the output and errors
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // Debug logs (optional)
                Console.WriteLine("Output: " + output);
                Console.WriteLine("Errors: " + errors);

                // Handle errors
                if (!string.IsNullOrEmpty(errors))
                {
                    try
                    {
                        File.AppendAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\ExchangeShellError.txt", errors);
                    }
                    catch 
                    {

                    }
                }
            });
        }

        public static async Task CreateInternalMailSendConnector(string fqdn)
        {
            string serverName = Environment.MachineName; // Dynamically fetch the server name

            // PowerShell script to create the "Internal Mail" Send Connector
            string script = $@"
        # Check if the Internal Mail Send Connector exists
        if (!(Get-SendConnector | Where-Object {{ $_.Name -eq 'Internal Mail' }})) {{
            # Create the Internal Mail Send Connector
            New-SendConnector -Name 'Internal Mail' -Usage Internal -AddressSpaces 'jc.local' `
                -SourceTransportServers '{serverName}' -Fqdn '{fqdn}' -DNSRoutingEnabled $true;
            Write-Output 'Internal Mail Send Connector created successfully.';
        }} else {{
            Write-Output 'Internal Mail Send Connector already exists.';
        }}
    ";

            // Run the PowerShell script
            await RunExchangePowerShellScript(script);
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
            await Functions.RunPowerShellScript(
                "Install-WindowsFeature Server-Media-Foundation, NET-Framework-45-Features, RPC-over-HTTP-proxy, RSAT-Clustering, RSAT-Clustering-CmdInterface, RSAT-Clustering-Mgmt, RSAT-Clustering-PowerShell, WAS-Process-Model, Web-Asp-Net45, Web-Basic-Auth, Web-Client-Auth, Web-Digest-Auth, Web-Dir-Browsing, Web-Dyn-Compression, Web-Http-Errors, Web-Http-Logging, Web-Http-Redirect, Web-Http-Tracing, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Metabase, Web-Mgmt-Console, Web-Mgmt-Service, Web-Net-Ext45, Web-Request-Monitor, Web-Server, Web-Stat-Compression, Web-Static-Content, Web-Windows-Auth, Web-WMI, Windows-Identity-Foundation, RSAT-ADDS"
            );

            // Install Chocolatey packages in parallel
            string currentPath = Process.GetCurrentProcess().MainModule.FileName;
            await Task.Run(async () =>
            {
                Process.Start(currentPath, "exchange").WaitForExit();
            });
        }

        public static async Task ClearPendingReboots()
        {
            // Clear Pending Reboots //
            string script = @"
            Remove-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager' -Name 'PendingFileRenameOperations' -ErrorAction SilentlyContinue
            Write-Host 'Pending file rename operations cleared.'
        ";
            await Functions.RunPowerShellScript(script);
            string Allscript = @"
# Clear all pending reboot indicators
Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing' -Name 'RebootPending' -Force -ErrorAction SilentlyContinue;
Remove-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager' -Name 'PendingFileRenameOperations' -Force -ErrorAction SilentlyContinue;
Remove-Item -Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired' -Recurse -Force -ErrorAction SilentlyContinue;
";

            await Functions.RunPowerShellScript(Allscript);
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
