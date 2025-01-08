using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UsefulTools;
using Task = System.Threading.Tasks.Task;

namespace Windows_Server_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\TasksComplete.txt"))
            {
                // Set Static IP //
                await Command.RunCommandHidden("@echo off\r\nSETLOCAL ENABLEDELAYEDEXPANSION\r\n\r\n:: Get the current IP address, Subnet Mask, and Default Gateway\r\nFOR /F \"tokens=2 delims=:\" %%a in ('ipconfig ^| findstr /C:\"IPv4 Address\"') do set currentIP=%%a\r\nFOR /F \"tokens=2 delims=:\" %%b in ('ipconfig ^| findstr /C:\"Subnet Mask\"') do set subnetMask=%%b\r\nFOR /F \"tokens=2 delims=:\" %%c in ('ipconfig ^| findstr /C:\"Default Gateway\"') do set defaultGateway=%%c\r\n\r\n:: Remove leading spaces\r\nSET currentIP=%currentIP:~1%\r\nSET subnetMask=%subnetMask:~1%\r\nSET defaultGateway=%defaultGateway:~1%\r\n\r\n:: Set the static IP address (using the current IP)\r\nnetsh interface ip set address \"Ethernet0\" static %currentIP% %subnetMask% %defaultGateway%\r\n\r\n:: Set the DNS server to the default gateway\r\nnetsh interface ip set dns \"Ethernet0\" static %defaultGateway%\r\n\r\necho New IP configuration:\r\necho IP Address: %currentIP%\r\necho Subnet Mask: %subnetMask%\r\necho Default Gateway: %defaultGateway%\r\necho DNS Server: %defaultGateway%\r\n\r\nENDLOCAL\r\n");
                // Do the repetitive tasks //
                await SolveWindowsTasks(); 
                File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\TasksComplete.txt","true");
            }
            HandleCommandLineArgs(Environment.GetCommandLineArgs());
            if(Directory.Exists("C:\\Exchange") || Directory.Exists("C:\\SCCM"))
            {
                OmegaServerPromoteButton.IsEnabled = true;
                DomainNameTextBox.IsEnabled = false;
                InstallActiveDirectoryButton.IsEnabled = false;
                InstallActiveDirectoryButton.Content = "Please promote to omega server";
                OmegaServerPromoteButton.Click += OmegaServerPromoteButton_Click;
            }
        }

        private async void OmegaServerPromoteButton_Click(object sender, RoutedEventArgs e)
        {
            await PromoteToOmegaServer();
        }

        private async Task PromoteToOmegaServer()
        {
            OmegaServerPromoteButton.IsEnabled = false;
            new WebClient().DownloadFile(new Uri("https://raw.githubusercontent.com/CafePromenade/Windows-Server-Setupper/refs/heads/main/Windows-Server-Tools/Exchange-Installer/bin/x64/Debug/Exchange-Installer.exe"), Environment.GetEnvironmentVariable("TEMP") + "\\EXCHANGE.exe");
            Process.Start(Environment.GetEnvironmentVariable("TEMP") + "\\EXCHANGE.exe");
        }

        private void ShowUsage()
        {
            MessageBox.Show("Usage:\nYourApp promotetodc <domainName> <safeModeAdminPassword> [domainNetbiosName]\nYourApp simpsons", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private async void HandleCommandLineArgs(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    //ShowUsage();
                    if (!File.Exists(Environment.GetEnvironmentVariable("APPDATA") + "\\ChocoComplete.txt"))
                    {
                        File.WriteAllText(Environment.GetEnvironmentVariable("APPDATA") + "\\ChocoComplete.txt","true");
                        await Chocolatey.InstallChocolatey();

                        ChocoInstall("filezilla winscp vscode googlechrome veracrypt firefox opera python nodejs dotnetfx"); 
                    }
                    return;
                }

                string command = args[1].ToLower();

                if (command == "promotetodc")
                {
                    if (args.Length < 4)
                    {
                        MessageBox.Show("Usage: YourApp promotetodc <domainName> <safeModeAdminPassword> [domainNetbiosName]", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string domainName = args[2];
                    //MessageBox.Show("DOMAIN: " + domainName);
                    string safeModeAdminPassword = args[3];
                    //MessageBox.Show("ADMIN PASSWORD: " + safeModeAdminPassword);
                    string domainNetbiosName = args.Length > 3 ? args[4] : "CONTOSO";
                    //MessageBox.Show("NETBIOS: " + domainNetbiosName);
                    // Call the function to install Active Directory
                    CreateSimpsonsTask();
                    InstallActiveDirectoryAndPromoteToDC(domainName, safeModeAdminPassword, domainNetbiosName);
                    Close();
                }
                else if(command == "task")
                {
                    CreateSimpsonsTask();
                    Close();
                }
                else if (command == "simpsons")
                {
                    await SimpsonsSolution();
                    Close();
                }
                else
                {
                    ShowUsage();
                }
            }
            catch 
            {

            }
        }


        public void CreateSimpsonsTask()
        {
            // Define the task details
            string taskName = "Run Simpsons Setup";
            string executablePath = @"C:\Users\Administrator\Desktop\Setup.exe";
            string arguments = "simpsons";

            using (TaskService ts = new TaskService())
            {
                // Create a new task definition
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Runs Setup.exe with 'simpsons' arguments after login.";
                td.Principal.UserId = Environment.UserDomainName + "\\" + Environment.UserName;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest; // Run with highest privileges

                // Set the trigger to run at logon
                td.Triggers.Add(new LogonTrigger());

                // Create the action to run the executable with arguments
                td.Actions.Add(new ExecAction(executablePath, arguments, null));

                // Action to delete the task after it finishes
                string deleteTaskScript = $@"
                    schtasks /delete /tn ""{taskName}"" /f
                ";
                td.Actions.Add(new ExecAction("cmd.exe", $"/c {deleteTaskScript}", null));

                // Register the task in the Task Scheduler
                ts.RootFolder.RegisterTaskDefinition(taskName, td);
            }
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

        private async void InstallActiveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            InstallActiveDirectoryButton.IsEnabled = false;
            await Task.Delay(500);
            InstallActiveDirectoryButton.Content = "Please wait";
            InstallActiveDirectoryAndPromoteToDC(DomainNameTextBox.Text, "P@ssw0rd", DomainNameTextBox.Text.Split('.')[0].ToUpper());
            InstallActiveDirectoryButton.Content = "DONE";
            await Task.Delay(500);
            InstallActiveDirectoryButton.IsEnabled = true;
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Command.RunCommandHidden("@echo off\r\nSETLOCAL ENABLEDELAYEDEXPANSION\r\n\r\n:: Get the current IP address, Subnet Mask, and Default Gateway\r\nFOR /F \"tokens=2 delims=:\" %%a in ('ipconfig ^| findstr /C:\"IPv4 Address\"') do set currentIP=%%a\r\nFOR /F \"tokens=2 delims=:\" %%b in ('ipconfig ^| findstr /C:\"Subnet Mask\"') do set subnetMask=%%b\r\nFOR /F \"tokens=2 delims=:\" %%c in ('ipconfig ^| findstr /C:\"Default Gateway\"') do set defaultGateway=%%c\r\n\r\n:: Remove leading spaces\r\nSET currentIP=%currentIP:~1%\r\nSET subnetMask=%subnetMask:~1%\r\nSET defaultGateway=%defaultGateway:~1%\r\n\r\n:: Set the static IP address (using the current IP)\r\nnetsh interface ip set address \"Ethernet0\" static %currentIP% %subnetMask% %defaultGateway%\r\n\r\n:: Set the DNS server to the default gateway\r\nnetsh interface ip set dns \"Ethernet0\" static %defaultGateway%\r\n\r\necho New IP configuration:\r\necho IP Address: %currentIP%\r\necho Subnet Mask: %subnetMask%\r\necho Default Gateway: %defaultGateway%\r\necho DNS Server: %defaultGateway%\r\n\r\nENDLOCAL\r\n");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await SimpsonsSolution();
        }

        public async Task SimpsonsSolution()
        {
            // Create Shared Folders //
            Directory.CreateDirectory("C:\\Staff");
            Directory.CreateDirectory("C:\\Staff\\HR");
            Directory.CreateDirectory("C:\\Staff\\IT");
            Directory.CreateDirectory("C:\\Staff\\Sales");

            // CREATE SIMPSONS OU //
            string SimpsonsOUScript = ReplaceWithDomainStuff(@"
    Import-Module ActiveDirectory;
    $ouName = 'Simpsons';
    $ouPath = 'DC=jackson,DC=local';  # Ensure this is quoted
    New-ADOrganizationalUnit -Name $ouName -Path $ouPath;
    Write-Host 'OU Simpsons successfully created in jackson.local';
");

            RunPowerShellScript(SimpsonsOUScript);


            // CSV-DE The Simpsons Users //
            string ReplacedCSV = Data.SimpsonsUsers.Replace("DC=jackson", "DC=" + DomainName);
            ReplacedCSV = Data.SimpsonsUsers.Replace("DC=local", "DC=" + DomainCOM);
            ReplacedCSV = ReplaceWithDomainStuff(Data.SimpsonsUsers);
            File.WriteAllText("C:\\lol.csv", ReplacedCSV);
            await Command.RunCommandHidden("csvde -i -f \"" + "C:\\lol.csv" + "\"");

            // JOIN THE GROUPS AND CREATE THE OU //
            string CreateOUScript = ReplaceWithDomainStuff(@"
    New-ADOrganizationalUnit -Name 'Staff' -Path 'DC=jackson,DC=local';  # Quoted path
    New-ADOrganizationalUnit -Name 'Sales' -Path 'OU=Staff,DC=jackson,DC=local';
    New-ADOrganizationalUnit -Name 'HR' -Path 'OU=Staff,DC=jackson,DC=local';
    New-ADOrganizationalUnit -Name 'IT' -Path 'OU=Staff,DC=jackson,DC=local';
");

            RunPowerShellScript(CreateOUScript);

            string CreateGroupScript = ReplaceWithDomainStuff(@"
    Import-Module ActiveDirectory;
    $groups = @('IT-Group', 'HR-Group', 'Sales-Group');
    $ou = 'OU=Staff,DC=jackson,DC=local';  # Quoted path
    foreach ($group in $groups) {
        New-ADGroup -Name $group -GroupScope Global -GroupCategory Security -Path $ou -Description '$group group';
    }
    Write-Host 'Groups successfully created';
");

            RunPowerShellScript(CreateGroupScript);

            string SpreadUsersScript = ReplaceWithDomainStuff(@"
    $SimpsonsUsers = Get-ADUser -Filter * -SearchBase 'OU=Simpsons,DC=jackson,DC=local';  # Quoted path
    $i = 0;
    foreach ($user in $SimpsonsUsers) {
        switch ($i % 3) {
            0 {
                Move-ADObject -Identity $user.DistinguishedName -TargetPath 'OU=HR,OU=Staff,DC=jackson,DC=local';
                Add-ADGroupMember -Identity 'HR-Group' -Members $user;
            }
            1 {
                Move-ADObject -Identity $user.DistinguishedName -TargetPath 'OU=IT,OU=Staff,DC=jackson,DC=local';
                Add-ADGroupMember -Identity 'IT-Group' -Members $user;
            }
            2 {
                Move-ADObject -Identity $user.DistinguishedName -TargetPath 'OU=Sales,OU=Staff,DC=jackson,DC=local';
                Add-ADGroupMember -Identity 'Sales-Group' -Members $user;
            }
        }
        $i++;
    }
");

            RunPowerShellScript(SpreadUsersScript);

            // SHARE FOLDERS WITH RESPECTING MEMBERS //
            string ShareScript = ReplaceWithDomainStuff(@"
    $folders = @(
        @{Name = 'HR'; Group = 'HR-Group'},
        @{Name = 'IT'; Group = 'IT-Group'},
        @{Name = 'Sales'; Group = 'Sales-Group'}
    );
    $basePath = 'C:\\Staff';
    foreach ($folder in $folders) {
        $folderPath = Join-Path $basePath $folder.Name;
        New-SmbShare -Name $folder.Name -Path $folderPath -FullAccess $folder.Group;
        $acl = Get-Acl $folderPath;
        $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($folder.Group, 'FullControl', 'ContainerInherit, ObjectInherit', 'None', 'Allow');
        $acl.AddAccessRule($rule);
        Set-Acl $folderPath $acl;
    }
");

            RunPowerShellScript(ShareScript);


            // Install Stuff //
            Visibility = Visibility.Hidden;
            await ChocoInstall("filezilla winscp vscode googlechrome veracrypt firefox opera python nodejs dotnetfx");
        }

        public string ReplaceWithDomainStuff(string input)
        {
            return input.Replace("DC=jackson","DC=" + DomainName).Replace("DC=local","DC=" + DomainCOM);
        }

        private async void InstallChocolateyButton_Click(object sender, RoutedEventArgs e)
        {
            InstallChocolateyButton.IsEnabled = false;
            Command.RunCommand("@\"%SystemRoot%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe\" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command \"[System.Net.ServicePointManager]::SecurityProtocol = 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))\" && SET \"PATH=%PATH%;%ALLUSERSPROFILE%\\chocolatey\\bin\"");
            InstallChocolateyButton.IsEnabled = true;
        }

        private static async Task ChocoInstall(string SOFTWARE)
        {
            await Command.RunCommandHidden("\"C:\\ProgramData\\chocolatey\\bin\\choco.exe\" install " + SOFTWARE + " -y --ignore-checksums");
        }
        private async void InstallChocolateySoftwareButton_Click(object sender, RoutedEventArgs e)
        {
            InstallChocolateyButton.IsEnabled = false;
            Visibility = Visibility.Hidden;
            await Command.RunCommandHidden("\"C:\\ProgramData\\chocolatey\\bin\\choco.exe\" install " + ChocoSoftwareTextBox.Text + " -y --ignore-checksums");
            //await Chocolatey.ChocolateyDownload(ChocoSoftwareTextBox.Text);
            InstallChocolateyButton.IsEnabled = true;
            Visibility = Visibility.Visible;
        }

        private void CommonInstallStuffButton_Click(object sender, RoutedEventArgs e)
        {
            CommonlyInstalledWindowsComponents d = new CommonlyInstalledWindowsComponents();
            d.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Process.Start("ncpa.cpl");
        }
    }
}
