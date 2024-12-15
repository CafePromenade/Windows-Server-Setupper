using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCCM_Installer
{
    public partial class Form1 : Form
    {
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

        public async Task InstallSQLServer()
        {
            string SQLPath = "C:\\SQL_Setup\\setup.exe";
            string script = $"Start-Process -FilePath \"{SQLPath}\" -ArgumentList \"/QUIET /ACTION=Install /FEATURES=SQLENGINE /INSTANCENAME=MSSQLSERVER /SQLSVCACCOUNT='NT AUTHORITY\\SYSTEM' /SQLSYSADMINACCOUNTS='BUILTIN\\Administrators' /SAPWD='P@ssw0rd123!' /SECURITYMODE=SQL /IACCEPTSQLSERVERLICENSETERMS\" -Wait -NoNewWindow";

            await Functions.RunPowerShellScript(script);
        }

        public async Task ProcessInstall()
        {
            EnableStuff = false;
            await Functions.DaDhui(true, "install");
            await Functions.RunPowerShellScript("Install-WindowsFeature -Name Web-Server, Web-Windows-Auth, Web-Asp-Net45, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Mgmt-Console, NET-Framework-Features, NET-Framework-Core, BITS, RDC, RSAT-ADDS -IncludeManagementTools");
            await Functions.RunPowerShellScript("Install-WindowsFeature -Name UpdateServices -IncludeManagementTools");
            await Functions.ChocoInstall("windows-adk sql-server-management-studio sqlserver-odbcdriver");

            EnableStuff = true;
        }
    }
}
