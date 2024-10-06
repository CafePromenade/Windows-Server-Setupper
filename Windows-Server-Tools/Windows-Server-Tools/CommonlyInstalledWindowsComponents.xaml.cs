using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Windows_Server_Tools
{
    /// <summary>
    /// Interaction logic for CommonlyInstalledWindowsComponents.xaml
    /// </summary>
    public partial class CommonlyInstalledWindowsComponents : Window
    {
        public CommonlyInstalledWindowsComponents()
        {
            InitializeComponent();
        }

        private void IIS_Click(RoutedEventArgs e)
        {

        }

        private async void IIS_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.RunPowerShellScript("# Install IIS and all its components\r\nInstall-WindowsFeature -Name Web-Server -IncludeAllSubFeature -IncludeManagementTools\r\n");
        }

        private void FileAndStorage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.RunPowerShellScript("# Install File and Storage Services and all its components\r\nInstall-WindowsFeature -Name FS-FileServer, FS-BranchCache, FS-Data-Deduplication, FS-DFS-Namespace, FS-DFS-Replication, FS-FileServer-VSS, FS-NFS-Service, FS-Resource-Manager, FS-SMB1, FS-SMB2, FS-SMB3, FS-SyncShareService, FS-Data-Deduplication, FS-FileServer-Resource-Manager, FS-iSCSITarget-Server, FS-iSCSITarget-VSS-VDS, FS-NFS-Service, FS-Resource-Manager, FS-VSS-Agent, Storage-Services -IncludeManagementTools\r\n");
        }
    }
}
