using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
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
    }
}
