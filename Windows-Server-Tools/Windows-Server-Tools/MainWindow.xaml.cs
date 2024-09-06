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
            PromoteToDomainController(DomainNameTextBox.Text, "P@ssw0rd");
            InstallActiveDirectoryButton.IsEnabled = true;
        }

        public static void PromoteToDomainController(string domainName, string safeModeAdminPassword)
        {
            try
            {
                // Create a PowerShell instance
                using (PowerShell psInstance = PowerShell.Create())
                {
                    // Command to install ADDS feature
                    psInstance.AddScript("Install-WindowsFeature -Name AD-Domain-Services -IncludeManagementTools");

                    // Execute the script
                    Collection<PSObject> result = psInstance.Invoke();
                    foreach (var outputItem in result)
                    {
                        Console.WriteLine(outputItem.ToString());
                    }

                    // Reset the PowerShell instance for promoting the server to a DC
                    psInstance.Commands.Clear();

                    // Create the PowerShell script to promote the server to a DC
                    string promoteCommand = $"Install-ADDSForest -DomainName {domainName} -SafeModeAdministratorPassword (ConvertTo-SecureString \"{safeModeAdminPassword}\" -AsPlainText -Force) -Force -InstallDns";

                    psInstance.AddScript(promoteCommand);

                    // Execute the promotion command
                    result = psInstance.Invoke();

                    // Display any result or error
                    foreach (var outputItem in result)
                    {
                        Console.WriteLine(outputItem.ToString());
                    }

                    // Check for errors
                    if (psInstance.Streams.Error.Count > 0)
                    {
                        foreach (var error in psInstance.Streams.Error)
                        {
                            Console.WriteLine($"Error: {error.ToString()}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Server successfully promoted to Domain Controller.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

    }
}
