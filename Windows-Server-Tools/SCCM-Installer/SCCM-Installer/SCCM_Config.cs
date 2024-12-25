using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCM_Installer
{
    public class SCCM_Config
    {
        public static string GetConfigScript(string FQDN)
        {
            return ConfigScript.Replace("dew.jerjer.hui",FQDN);
        }

        public static string ConfigScript = @"[Identification]
Action=InstallPrimarySite
CDLatest=1

[Options]
ProductID=Eval
SiteCode=XYZ
SiteName=JERJER MANAGEMENT
SMSInstallDir=C:\Program Files\Microsoft Configuration Manager
SDKServer=dew.jerjer.hui
PrerequisiteComp=0
PrerequisitePath=C:\Sources\Redist
AdminConsole=1
JoinCEIP=0
ManagementPoint=dew.jerjer.hui
ManagementPointProtocol=HTTP
DistributionPoint=dew.jerjer.hui
DistributionPointProtocol=HTTP
DistributionPointInstallIIS=1
RoleCommunicationProtocol=HTTPorHTTPS
ClientsUsePKICertificate=0
MobileDeviceLanguage=0

[SQLConfigOptions]
SQLServerName=dew.jerjer.hui
SQLServerPort=1433
DatabaseName=CM_XYZ
SQLSSBPort=4022
SQLDataFilePath=C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\
SQLLogFilePath=C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\

[CloudConnectorOptions]
CloudConnector=1
CloudConnectorServer=dew.jerjer.hui
UseProxy=0

[SABranchOptions]
SAActive=1
CurrentBranch=1";
    }
}
