###############################################################################
# This script handles OWA/ECP updates.
###############################################################################

trap {
	Log ("Error updating OWA/ECP: " + $_)
	Exit
}

$WarningPreference = 'SilentlyContinue'
$ConfirmPreference = 'None'

$script:logDir = "$env:SYSTEMDRIVE\ExchangeSetupLogs"

# Log( $entry )
#	Append a string to a well known text file with a time stamp
# Params:
#	Args[0] - Entry to write to log
# Returns:
#	void
function Log
{
	$entry = $Args[0]

	$line = "[{0}] {1}" -F $(get-date).ToString("HH:mm:ss"), $entry
	write-output($line)
	add-content -Path "$logDir\UpdateCas.log" -Value $line
}

# If log file folder doesn't exist, create it
if (!(Test-Path $logDir)){
	New-Item $logDir -type directory	
}

# Load the Exchange PS snap-in
add-PSSnapin -Name Microsoft.Exchange.Management.PowerShell.E2010

Log "***********************************************"
Log ("* UpdateCas.ps1: {0}" -F $(get-date))

# If Mailbox isn't installed on this server, exit without doing anything
if ((Get-ExchangeServer $([Environment]::MachineName)).ServerRole -notmatch "Mailbox") {
		Log "Warning: Mailbox role is not installed on server $([Environment]::MachineName)"
}
Log "Updating OWA/ECP on server $([Environment]::MachineName)"

# get the path to \owa on the filesystem
Log "Finding ClientAccess role install path on the filesystem"
$caspath = (get-itemproperty HKLM:\SOFTWARE\Microsoft\ExchangeServer\v15\Setup).MsiInstallPath + "ClientAccess\"

# GetVersionFromDll
#	Gets the version information from a specified dll
#	appName - the friendly name of the web application
#	webApp  - the alias of the web application, should be the folder name of the physical path
#	dllName  - the name of the assembly that indicate the version of the web application (version folder)
# Returns:
#	String value of the version information on the specified dll.
function GetVersionFromDll($appName, $webApp, $dllName)
{
	$apppath = $caspath + $webApp + "\"
	$dllpath = $apppath + "bin\" + $dllName
	# figure out which version of web application (OWA/ECP) we are moving to
	if (! (test-path ($dllpath))) {
		Log "Could not find '${dllpath}'.  Aborting."
		return $null
	}
	$version = ([diagnostics.fileversioninfo]::getversioninfo($dllpath)).fileversion -replace '0*(\d+)','$1'
	return $version
}

# UpdateWebApp
#	Update a web application in current CAS server.
# Params:
#	appName - the friendly name of the web application
#	webApp  - the alias of the web application, should be the folder name of the physical path
#	version - The version to use. 
#	sourceFolder - The name of the webApp folder to copy files from
#			e.g.: "Current2" for OWA2, "Current" is used for OWA Basic, ECP.
#	destinationFolder - The optional name of the folder to copy to
# Returns:
#	void
# For example, UpdateWebApp "OWA" "owa" "15.0.815" "Current" "prem"
function UpdateWebApp($appName, $webApp, $version, $sourceFolder, $destinationFolder)
{
	if ($version -eq $null) {
		Log "Could not determine version. Aborting."
		return
	}

	$apppath = $caspath + $webApp + "\"
	Log "Updating ${appName} to version $version"

	# filesystem path to the new version directory
	if ($destinationFolder -eq $null){
		$versionpath = $apppath + $version
	}
	else {
		$versionpath = $apppath + $destinationFolder + "\" + $version
	}

	Log "Copying files from '${apppath}${sourceFolder}' to '$versionpath'"
    New-Item $versionpath -Type Directory -ErrorAction SilentlyContinue
	copy-item -recurse -force ($apppath + $sourceFolder + "\*") $versionpath
	
	Log "Update ${appName} done."
}

# Upgrade from CU5 to CU6 leaves some files missing from unversioned OWA folder
# that is updated & replicated during MSP updates
function FixUnversionedFolderAfterUpgrade
{
	try
	{
		$setupRegistry = Get-Item -Path HKLM:\Software\Microsoft\ExchangeServer\v15\Setup\ -ea SilentlyContinue
		if (!$setupRegistry) { Log "FixUnversionedFolderAfterUpgrade: No setupRegistry"; return }

		# C:\Program Files\Microsoft\Exchange Server\V15\ClientAccess
		$installPath = $setupRegistry.GetValue('MsiInstallPath')
		if (!$installPath) { Log "FixUnversionedFolderAfterUpgrade: No installPath"; return }

		# 15.0.995.32
		$installedFork = (@('MsiProductMajor','MsiProductMinor','MsiBuildMajor') | %{ $setupRegistry.GetValue($_) }) -join '.'
		$srcVersions = @((get-item "$installPath\ClientAccess\Owa\prem\$($installedFork).*").Name | Sort { [System.Version] $_ })
		if (!$srcVersions) { Log "FixUnversionedFolderAfterUpgrade: No srcVersions $($installedFork).*"; return }
		Log "FixUnversionedFolderAfterUpgrade: Found source versions: $srcVersions"

		$srcRoot = (Get-Item "$installPath\ClientAccess\Owa\prem\$($srcVersions[0])").FullName
		$destRoot = (Get-Item "$installPath\ClientAccess\Owa\Current2\version").FullName
		Log "FixUnversionedFolderAfterUpgrade: Recovering files from '$srcRoot' to '$destRoot' where necessary"
		foreach ($srcPath in (Get-ChildItem -File -Recurse $srcRoot).FullName)
		{
			$subPath = $srcPath.Substring($srcRoot.Length+1)
			$destPath = "$destRoot\$subPath"
			if (!(Get-Item $destPath -ea SilentlyContinue))
			{
				Log "Copy-Item '$srcPath' '$destPath'"
				$destParent = Split-Path $destPath
				if ($destParent -and !(Test-Path $destParent))
				{
					$null = New-Item $destParent -type Directory
				}
				Copy-Item -Force $srcPath $destPath
			}
		}
		Log "FixUnversionedFolderAfterUpgrade success"
	}
	catch
	{
		Log "FixUnversionedFolderAfterUpgrade failed: $_.Exception.Message"
	}
}
FixUnversionedFolderAfterUpgrade

# Add an attribute to a given XML Element
# <param name="xmlDocument">Document where the attribute will be added</param>
# <param name="xmlElement">Element where the attribute will be added</param>
# <param name="attributeName">Name of the attribute</param>
# <param name="attributeValue">Value of the attribute</param>
function AddXmlAttribute
{
    param ([System.Xml.XmlDocument] $xmlDocument, [System.Xml.XmlElement] $xmlElement, [string] $attributeName, [string] $attributeValue);


    $attribute = $xmlDocument.CreateAttribute($attributeName);
    $attribute.set_Value($attributeValue) | Out-Null
    $xmlElement.SetAttributeNode($attribute) | Out-Null
}

# Add an assembly to the owa web.config.
# <param name="xmlDocument">The xml document to work on.</param>
# <param name="assemblyName">The assembly name to add.</param>
# <param name="version">The version of the assembly.</param>
function AddOrUpdateOwaWebConfigAssembly($xmlDocument, $assemblyName, $version)
{
    $assembliesXPath = "configuration//system.web/compilation/assemblies"
    $assembliesXmlNode = $xmlDocument.SelectSingleNode($assembliesXPath);

    if ($assembliesXmlNode -eq $null) { Log "$assembliesXPath is not found in web.config"; return }

    $addXPath = $assembliesXPath + "/add[starts-with(@assembly, '" + $assemblyName + "')]";
    $addXmlNode = $xmlDocument.SelectSingleNode($addXPath);

    $attributeValue =
            if ($version -ne $null) {
                "$assemblyName,Version=$version,Culture=neutral,publicKeyToken=31bf3856ad364e35";
            }
            else
            {
                "$assemblyName,Culture=neutral,publicKeyToken=31bf3856ad364e35";
            }
    
    # Checks if the <add> node for this assembly already exists, if not create one.
    if ($addXmlNode -eq $null)
    {
       [System.Xml.XmlNode] $addXmlNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "add", $null);

        AddXmlAttribute $xmlDocument $addXmlNode "assembly" $attributeValue;
        $assembliesXmlNode.AppendChild($addXmlNode) | Out-Null
    }
	else
    {
		$addXmlNode.Attributes.RemoveNamedItem("assembly") | Out-Null
        AddXmlAttribute $xmlDocument $addXmlNode "assembly" $attributeValue;
    }
}

# UpdateOwaWebConfig
#   Update Owa's web.config file
function UpdateOwaWebConfig {
	$owaWebConfigFolder = $caspath  + "owa\"
    $owaWebConfigPath = $owaWebConfigFolder + "web.config"

	try {
		$xmlDocument = New-Object System.Xml.XmlDocument;
		$xmlDocument.Load($owaWebConfigPath);

		AddOrUpdateOwaWebConfigAssembly $xmlDocument "Microsoft.Exchange.VariantConfiguration.Core" "15.0.0.0"
		AddOrUpdateOwaWebConfigAssembly $xmlDocument "Microsoft.Search.Platform.Parallax" "3.3.0.0"
		AddOrUpdateOwaWebConfigAssembly $xmlDocument "Microsoft.Exchange.Clients.Owa2.ServerVariantConfiguration" "15.0.0.0"
		AddOrUpdateOwaWebConfigAssembly $xmlDocument "Microsoft.Exchange.VariantConfiguration.ExCore" "15.0.0.0"

		$xmlDocument.Save($owaWebConfigPath) | Out-Null
	}
	catch
	{
		Log "Error loading OWA web.config: $_.Exception.Message"
	}
}

# Update/Add an appsetting key to the owa web.config
# <param name="keyName">The key appsetting key.</param>
# <param name="newValue">The value of the appsetting key.</param>
function UpdateOwaWebConfigAppSettings($keyName, $newValue)
{
	$appSettingNodeName = "configuration//appSettings"
	$owaWebConfigFolder = $caspath  + "owa\"
    $owaWebConfigPath = $owaWebConfigFolder + "web.config"
		
	$owaWebConfigPathCheck = (Get-Item $owaWebConfigPath).FullName
	if (!$owaWebConfigPathCheck) { Log "no OWA web.config is found"; return }

    $xmlDocument = New-Object System.Xml.XmlDocument;
    $xmlDocument.Load($owaWebConfigPath);
    $xmlNode = $xmlDocument.SelectSingleNode($appSettingNodeName);
	
	if ($xmlNode -eq $null) { Log "$appSettingNodeName is not found in web.config"; return }

	$addAsNewAppSettingKey = $true
	foreach ($child in $xmlNode.ChildNodes) 
	{
		if ($child.key -eq $keyName)
		{
			Log "Updating $keyName binding redirect to $newValue"
			$child.value = $newValue
			$xmlDocument.Save($owaWebConfigPath) | Out-Null
			$addAsNewAppSettingKey = $false
            break
		}
	}

	if ($addAsNewAppSettingKey)
	{
		Log "Adding $keyName appSetting with value $newValue"
		[System.Xml.XmlNode] $addXmlNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "add", $null);
		AddXmlAttribute $xmlDocument $addXmlNode "key" $keyName;
		AddXmlAttribute $xmlDocument $addXmlNode "value" $newValue;
		$xmlNode.AppendChild($addXmlNode) | Out-Null
		$xmlDocument.Save($owaWebConfigPath) | Out-Null
	}
}

UpdateOwaWebConfigAppSettings "owin:AutomaticAppStartup" "true"

# Add HttpHandler to the web.config
# <param name="xmlDocument">Name of the assembly: eg: Microsoft.Live.Controls</param>
# <param name="verb">verb</param>
# <param name="path">path</param>
# <param name="type">type</param>
# <param name="name">name</param>
# <param name="preCondition">preCondition</param>
function AddHttpHandlerToWebConfig
{
    param ([System.Xml.XmlDocument] $xmlDocument, [string] $verb, [string] $path, [string] $type, [string] $name, [string] $preCondition = "managedHandler" );

    # <configuration>
    #   <location inheritInChildApplications="false">
    #     <system.webServer>
    #       <handlers>

    $handlersNode = $xmlDocument.SelectSingleNode("configuration/location[not(@path)]/system.webServer/handlers");

    # if handlersNode is null, fail
    if ($handlersNode -eq $null)
    {
        Log "Web.config file does notr have handlers section: $key";
        return;
    }

    # now check if we already have added the HttpHandler node
    $httpHandlerNode = $xmlDocument.SelectSingleNode("/configuration/location[not(@path)]/system.webServer/handlers/add[@path='$path']");

    # create the HttpHandler node if it doesn't already exist, else ignore it
    if ($httpHandlerNode -eq $null)
    {
        #
        # Create httpHandlerNode
        #
        $httpHandlerNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "add", $null);

        # TODO: we are assuming that this returns the child node we just appended
        $httpHandlerNode  = $handlersNode.AppendChild($httpHandlerNode);

        #
        # Add attributes
        #
        AddXmlAttribute $xmlDocument $httpHandlerNode "verb" $verb;
        AddXmlAttribute $xmlDocument $httpHandlerNode "path" $path;
        AddXmlAttribute $xmlDocument $httpHandlerNode "type" $type;
        AddXmlAttribute $xmlDocument $httpHandlerNode "name" $name;
        AddXmlAttribute $xmlDocument $httpHandlerNode "preCondition" $preCondition;
    }
    else
    {
        Log "Web.config file already had a node for: $path";
    }
}

# Add/Update managed handler to owa web.config
function UpdateOwaWebConfigHandlers
{
    $owaWebConfigFolder = $caspath  + "owa\";
    $owaWebConfigPath = $owaWebConfigFolder + "web.config";
    $owaWebConfigPathCheck = (Get-Item $owaWebConfigPath).FullName;

    if (!$owaWebConfigPathCheck) 
    {
        Log "no OWA web.config is found"; 
        return;
    }

    $xmlDocument = New-Object System.Xml.XmlDocument;
    $xmlDocument.Load($owaWebConfigPath);

    AddHttpHandlerToWebConfig $xmlDocument "POST,GET" "userbootsettings.ashx" "Microsoft.Exchange.Clients.Owa2.Server.Web.UserBootSettingsHandler, Microsoft.Exchange.Clients.Owa2.Server" "UserBootSettingsHandler"
    AddHttpHandlerToWebConfig $xmlDocument "GET" "MeetingPollHandler.ashx" "Microsoft.Exchange.Clients.Owa2.Server.Web.MeetingPollHandler, Microsoft.Exchange.Clients.Owa2.Server" "MeetingPollHandler"

    $xmlDocument.Save($owaWebConfigPath) | Out-Null
}

Log "Updating owa handlers";
UpdateOwaWebConfigHandlers

#  Update/Add an  assembly binding redirect to the owa web.config.
# <param name="assemblyName">The assembly name to add.</param>
# <param name="publicKeyToken">The assembly public key token.</param>
# <param name="oldVersion">The oldVersion of the binding redirect.</param>
# <param name="newVersion">The newVersion of the binding redirect.</param>
function UpdateOwaWebConfigBindingRedirect($assemblyName, $publicKeyToken, $oldVersion, $newVersion) 
{
	$assemblyBindingNodeName = "configuration/runtime/ns:assemblyBinding"
	$owaWebConfigFolder = $caspath  + "owa\"
    $owaWebConfigPath = $owaWebConfigFolder + "web.config"
		
	$owaWebConfigPathCheck = (Get-Item $owaWebConfigPath).FullName
	if (!$owaWebConfigPathCheck) { Log "no OWA web.config is found"; return }

    $xmlDocument = New-Object System.Xml.XmlDocument;
    $xmlDocument.Load($owaWebConfigPath);
    $nsm = New-Object System.Xml.XmlNamespaceManager $xmlDocument.NameTable
    $nsm.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1")
    $xmlNode = $xmlDocument.SelectSingleNode($assemblyBindingNodeName, $nsm);
    
    if ($xmlNode -eq $null) { Log "$assemblyBindingNodeName is not found in web.config"; return }

	$addAsNewBindingRedirect = $true 
	foreach ($dependentAssembly in $xmlNode.ChildNodes) 
	{
		$assemblyIdentity = $null
        $bindingRedirect = $null
		foreach ($child in $dependentAssembly.ChildNodes)
		{
			if ($child.LocalName -eq "assemblyIdentity")
            {
                $assemblyIdentity = $child 
            }
            elseif ($child.LocalName -eq "bindingRedirect")
            {
                $bindingRedirect = $child 
            }
		}

		if ($assemblyIdentity -ne $null -and $assemblyIdentity.Name -eq $assemblyName -and $assemblyIdentity.publicKeyToken -eq $publicKeyToken -and $bindingRedirect -ne $null)
        {
			Log "Updating $assemblyName binding redirect"
            $bindingRedirect.oldVersion = $oldVersion
            $bindingRedirect.newVersion = $newVersion
            $xmlDocument.Save($owaWebConfigPath) | Out-Null
			$addAsNewBindingRedirect = $false
            break
        }
	}

	if ($addAsNewBindingRedirect)
	{
		Log "Adding $assemblyName binding redirect"
		[System.Xml.XmlNode] $dependentAssemblyXmlNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "dependentAssembly", "urn:schemas-microsoft-com:asm.v1");
		[System.Xml.XmlNode] $assemblyIdentityXmlNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "assemblyIdentity", "urn:schemas-microsoft-com:asm.v1");
		[System.Xml.XmlNode] $bindingRedirectXmlNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "bindingRedirect", "urn:schemas-microsoft-com:asm.v1");

        AddXmlAttribute $xmlDocument $assemblyIdentityXmlNode "name" $assemblyName;
		AddXmlAttribute $xmlDocument $assemblyIdentityXmlNode "publicKeyToken" $publicKeyToken;
		AddXmlAttribute $xmlDocument $assemblyIdentityXmlNode "culture" "neutral";

		AddXmlAttribute $xmlDocument $bindingRedirectXmlNode "oldVersion" $oldVersion;
		AddXmlAttribute $xmlDocument $bindingRedirectXmlNode "newVersion" $newVersion;

        $xmlNode.AppendChild($dependentAssemblyXmlNode) | Out-Null
		$dependentAssemblyXmlNode.AppendChild($assemblyIdentityXmlNode) | Out-Null
		$dependentAssemblyXmlNode.AppendChild($bindingRedirectXmlNode) | Out-Null
		$xmlDocument.Save($owaWebConfigPath) | Out-Null
	}
}

# Enable the EnableClientsCommonModule
function EnableClientsCommonModule
{
    Log "Trying to add clients common module to Web.config file";
    
    $owaWebConfigFolder = $caspath  + "owa\"
    $owaWebConfigPath = $owaWebConfigFolder + "web.config"

    $owaWebConfigPathCheck = (Get-Item $owaWebConfigPath).FullName
    if (!$owaWebConfigPathCheck) { Log "no OWA web.config is found"; return }

    $xmlDocument = New-Object System.Xml.XmlDocument;
    $xmlDocument.Load($owaWebConfigPath);
    
    $configurationRoot = "configuration/location[not(@path)]";

    $clientsCommonModuleNode = $xmlDocument.SelectSingleNode($configurationRoot + "/system.webServer/modules/add[@name='ClientsCommonModule']");

    $clientsCommonModuleNodeType = "Microsoft.Exchange.Clients.Common.ClientsCommonModule";

    if ($clientsCommonModuleNode -eq $null)
    {
        Log "Adding clients common module to Web.config file" "Warning";
        [System.Xml.XmlNode] $clientsCommonModuleNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "add", $null);

        AddXmlAttribute $xmlDocument $clientsCommonModuleNode "name" "ClientsCommonModule";
        AddXmlAttribute $xmlDocument $clientsCommonModuleNode "type" $clientsCommonModuleNodeType;

        $modulesNode = $xmlDocument.SelectSingleNode($configurationRoot + "/system.webServer/modules");
        $owa2ModuleNode = $xmlDocument.SelectSingleNode($configurationRoot + "/system.webServer/modules/add[@name='Owa2Module']");

        if ($owa2ModuleNode -ne $null)
        {
            $modulesNode.InsertBefore($clientsCommonModuleNode, $owa2ModuleNode);
            $xmlDocument.Save($owaWebConfigPath) | Out-Null
        }
        else
        {
            Log "Owa2Module not found in web.config";
        }
    }
}

UpdateOwaWebConfigBindingRedirect "Microsoft.Owin" "31bf3856ad364e35" "0.0.0.0-2.1.0.0" "3.0.1.0"
UpdateOwaWebConfigBindingRedirect "Microsoft.Owin.Security" "31bf3856ad364e35" "0.0.0.0-2.1.0.0" "3.0.1.0"
UpdateOwaWebConfigBindingRedirect "Newtonsoft.Json" "30ad4fe6b2a6aeed" "0.0.0.0-13.0.0.0" "13.0.0.0"
EnableClientsCommonModule

#  Add HttpRequestFilteringModule to owa web.config
function AddHttpRequestFilteringModule
{
	Log "Trying to add http request filtering module to Owa Web.config file";
    
    $owaWebConfigFolder = $caspath  + "owa\"
    $owaWebConfigPath = $owaWebConfigFolder + "web.config"

    $owaWebConfigPathCheck = (Get-Item $owaWebConfigPath).FullName
    if (!$owaWebConfigPathCheck) { Log "no OWA web.config is found"; return }

    $xmlDocument = New-Object System.Xml.XmlDocument;
    $xmlDocument.Load($owaWebConfigPath);
    
    $configurationRoot = "configuration/location";

    $moduleNode = $xmlDocument.SelectSingleNode($configurationRoot + "/system.webServer/modules/add[@name='HttpRequestFilteringModule']");

    $moduleNodeType = "Microsoft.Exchange.HttpRequestFiltering.HttpRequestFilteringModule, Microsoft.Exchange.HttpRequestFiltering, Version=15.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

    if ($moduleNode -eq $null)
    {
        Log "Adding http request filtering module to Owa Web.config file";
        [System.Xml.XmlNode] $httpFilteringModuleNode = $xmlDocument.CreateNode([System.Xml.XmlNodeType]::Element, "add", $null);

        AddXmlAttribute $xmlDocument $httpFilteringModuleNode "name" "HttpRequestFilteringModule";
        AddXmlAttribute $xmlDocument $httpFilteringModuleNode "type" $moduleNodeType;

        $modulesNode = $xmlDocument.SelectSingleNode($configurationRoot + "/system.webServer/modules");
		
		# Add after CasHealthModule as it is always first module
        $casHealthModuleNode = $xmlDocument.SelectSingleNode($configurationRoot + "/system.webServer/modules/add[@name='CasHealthModule']");

        if ($casHealthModuleNode -ne $null)
        {
            $modulesNode.InsertAfter($httpFilteringModuleNode, $casHealthModuleNode);
            $xmlDocument.Save($owaWebConfigPath) | Out-Null
			Log "HttpRequestFilteringModule is added to Web.config";
        }
        elseif ($modulesNode -ne $null)
        {
			$modulesNode.PrependChild($httpFilteringModuleNode);
			$xmlDocument.Save($owaWebConfigPath) | Out-Null
            Log "HttpRequestFilteringModule is added to Web.config";
        }
		else 
		{
			Log "configuration/location/system.webServer/modules not found in web.config, HttpRequestFilteringModule is not added";
		}
    }
}

AddHttpRequestFilteringModule

function UpdateOwaWebConfigSmimeLocation()
{
    <#
        Add overrides specific to smime folder
    #>
    $owaWebConfigPath = "${caspath}\owa\web.config";

    if (-not (Test-Path -Path $owaWebConfigPath)) {
        Log "Files was not found: ${owaWebConfigPath}";
        return;
    }

    $xmlDocument = New-Object System.Xml.XmlDocument;
	$xmlDocument.Load($owaWebConfigPath);

    $xpathSmimeStaticContent = "/configuration/location[@path='smime']/system.webServer/staticContent";

    ## There can be at most one instances of <staticContent> node for each location.
    ## Checking if smime location has staticContent node already defined.
    $smimeStaticContent = $xmlDocument.SelectSingleNode($xpathSmimeStaticContent);

    if ($smimeStaticContent -eq $null)
    {
        Log "Adding staticContent node to smime location";
        $smimeLocationsWithEmptyStaticContent = $xmlDocument.ImportNode(([xml] @'
          <location inheritInChildApplications="false" path="smime">
            <system.webServer>
              <staticContent>
              </staticContent>
            </system.webServer>
          </location>
'@).DocumentElement, $true);
        $xmlDocument.DocumentElement.AppendChild($smimeLocationsWithEmptyStaticContent) | Out-Null;

        ## Now staticContent can be found
        $smimeStaticContent = $xmlDocument.SelectSingleNode($xpathSmimeStaticContent);
    }

    ## find any existing nodes for .msi or .appxbundle extensions
    $oldMimeDefinitions = $smimeStaticContent.SelectNodes("//remove[@fileExtension='.msi' or @fileExtension='.appxbundle' or @fileExtension='.crx'] | //mimeMap[@fileExtension='.msi' or @fileExtension='.appxbundle' or @fileExtension='.crx']");
    Log "Removing $($oldMimeDefinitions.Count) nodes from smime's static content";
    $oldMimeDefinitions | ForEach {
        $node = $_;
        $node.parentNode.RemoveChild($node) | Out-Null;
    }

    $newMimeDefinitions = ([xml]@'
      <staticContent>
        <remove fileExtension=".msi" />
        <mimeMap fileExtension=".msi" mimeType="application/octet-stream" />
        <remove fileExtension=".appxbundle" />
        <mimeMap fileExtension=".appxbundle" mimeType="application/octet-stream" />
        <remove fileExtension=".crx" />
        <mimeMap fileExtension=".crx" mimeType="application/x-chrome-extension" />
      </staticContent>
'@).SelectNodes('staticContent/*');

    Log "Adding $($newMimeDefinitions.Count) nodes to smime's static content";
    $newMimeDefinitions | ForEach {
         $node = $_;
         $smimeStaticContent.AppendChild($smimeStaticContent.OwnerDocument.ImportNode($node, $true)) | Out-Null;;
    }

    $xmlDocument.Save($owaWebConfigPath) | Out-Null;
}

UpdateOwaWebConfigSmimeLocation;

# Update OWA
$owaBasicVersion = (get-itemproperty -Path HKLM:\Software\Microsoft\ExchangeServer\v15\Setup\ -Name "OwaBasicVersion" -ea SilentlyContinue).OwaBasicVersion
$owaVersion = (get-itemproperty -Path HKLM:\Software\Microsoft\ExchangeServer\v15\Setup\ -Name "OwaVersion" -ea SilentlyContinue).OwaVersion

UpdateWebApp "OWA" "owa" $owaBasicVersion "Current" $null
UpdateWebApp "OWA" "owa" $owaVersion "Current2\version" "prem"
UpdateOwaWebConfig

# Update ECP
# Anonymous access has been enabled on ECP root folder by default, so it isn't necessary to enable anonymous access on the version folder explicitly
$ecpVersion = GetVersionFromDll "ECP" "ecp" "Microsoft.Exchange.Management.ControlPanel.dll"
UpdateWebApp "ECP" "ecp" $ecpVersion "Current" $null

# Remove the Exchange PS snap-in
remove-PSSnapin -Name Microsoft.Exchange.Management.PowerShell.E2010

# SIG # Begin signature block
# MIInvwYJKoZIhvcNAQcCoIInsDCCJ6wCAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCAB7X598/h+aERI
# 8qB6HmEL/m5ih7bcgj0phiIvz582zKCCDXYwggX0MIID3KADAgECAhMzAAADrzBA
# DkyjTQVBAAAAAAOvMA0GCSqGSIb3DQEBCwUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMjMxMTE2MTkwOTAwWhcNMjQxMTE0MTkwOTAwWjB0MQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMR4wHAYDVQQDExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQDOS8s1ra6f0YGtg0OhEaQa/t3Q+q1MEHhWJhqQVuO5amYXQpy8MDPNoJYk+FWA
# hePP5LxwcSge5aen+f5Q6WNPd6EDxGzotvVpNi5ve0H97S3F7C/axDfKxyNh21MG
# 0W8Sb0vxi/vorcLHOL9i+t2D6yvvDzLlEefUCbQV/zGCBjXGlYJcUj6RAzXyeNAN
# xSpKXAGd7Fh+ocGHPPphcD9LQTOJgG7Y7aYztHqBLJiQQ4eAgZNU4ac6+8LnEGAL
# go1ydC5BJEuJQjYKbNTy959HrKSu7LO3Ws0w8jw6pYdC1IMpdTkk2puTgY2PDNzB
# tLM4evG7FYer3WX+8t1UMYNTAgMBAAGjggFzMIIBbzAfBgNVHSUEGDAWBgorBgEE
# AYI3TAgBBggrBgEFBQcDAzAdBgNVHQ4EFgQURxxxNPIEPGSO8kqz+bgCAQWGXsEw
# RQYDVR0RBD4wPKQ6MDgxHjAcBgNVBAsTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEW
# MBQGA1UEBRMNMjMwMDEyKzUwMTgyNjAfBgNVHSMEGDAWgBRIbmTlUAXTgqoXNzci
# tW2oynUClTBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8vd3d3Lm1pY3Jvc29mdC5j
# b20vcGtpb3BzL2NybC9NaWNDb2RTaWdQQ0EyMDExXzIwMTEtMDctMDguY3JsMGEG
# CCsGAQUFBwEBBFUwUzBRBggrBgEFBQcwAoZFaHR0cDovL3d3dy5taWNyb3NvZnQu
# Y29tL3BraW9wcy9jZXJ0cy9NaWNDb2RTaWdQQ0EyMDExXzIwMTEtMDctMDguY3J0
# MAwGA1UdEwEB/wQCMAAwDQYJKoZIhvcNAQELBQADggIBAISxFt/zR2frTFPB45Yd
# mhZpB2nNJoOoi+qlgcTlnO4QwlYN1w/vYwbDy/oFJolD5r6FMJd0RGcgEM8q9TgQ
# 2OC7gQEmhweVJ7yuKJlQBH7P7Pg5RiqgV3cSonJ+OM4kFHbP3gPLiyzssSQdRuPY
# 1mIWoGg9i7Y4ZC8ST7WhpSyc0pns2XsUe1XsIjaUcGu7zd7gg97eCUiLRdVklPmp
# XobH9CEAWakRUGNICYN2AgjhRTC4j3KJfqMkU04R6Toyh4/Toswm1uoDcGr5laYn
# TfcX3u5WnJqJLhuPe8Uj9kGAOcyo0O1mNwDa+LhFEzB6CB32+wfJMumfr6degvLT
# e8x55urQLeTjimBQgS49BSUkhFN7ois3cZyNpnrMca5AZaC7pLI72vuqSsSlLalG
# OcZmPHZGYJqZ0BacN274OZ80Q8B11iNokns9Od348bMb5Z4fihxaBWebl8kWEi2O
# PvQImOAeq3nt7UWJBzJYLAGEpfasaA3ZQgIcEXdD+uwo6ymMzDY6UamFOfYqYWXk
# ntxDGu7ngD2ugKUuccYKJJRiiz+LAUcj90BVcSHRLQop9N8zoALr/1sJuwPrVAtx
# HNEgSW+AKBqIxYWM4Ev32l6agSUAezLMbq5f3d8x9qzT031jMDT+sUAoCw0M5wVt
# CUQcqINPuYjbS1WgJyZIiEkBMIIHejCCBWKgAwIBAgIKYQ6Q0gAAAAAAAzANBgkq
# hkiG9w0BAQsFADCBiDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24x
# EDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlv
# bjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9yaXR5
# IDIwMTEwHhcNMTEwNzA4MjA1OTA5WhcNMjYwNzA4MjEwOTA5WjB+MQswCQYDVQQG
# EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQDEx9NaWNyb3NvZnQg
# Q29kZSBTaWduaW5nIFBDQSAyMDExMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIIC
# CgKCAgEAq/D6chAcLq3YbqqCEE00uvK2WCGfQhsqa+laUKq4BjgaBEm6f8MMHt03
# a8YS2AvwOMKZBrDIOdUBFDFC04kNeWSHfpRgJGyvnkmc6Whe0t+bU7IKLMOv2akr
# rnoJr9eWWcpgGgXpZnboMlImEi/nqwhQz7NEt13YxC4Ddato88tt8zpcoRb0Rrrg
# OGSsbmQ1eKagYw8t00CT+OPeBw3VXHmlSSnnDb6gE3e+lD3v++MrWhAfTVYoonpy
# 4BI6t0le2O3tQ5GD2Xuye4Yb2T6xjF3oiU+EGvKhL1nkkDstrjNYxbc+/jLTswM9
# sbKvkjh+0p2ALPVOVpEhNSXDOW5kf1O6nA+tGSOEy/S6A4aN91/w0FK/jJSHvMAh
# dCVfGCi2zCcoOCWYOUo2z3yxkq4cI6epZuxhH2rhKEmdX4jiJV3TIUs+UsS1Vz8k
# A/DRelsv1SPjcF0PUUZ3s/gA4bysAoJf28AVs70b1FVL5zmhD+kjSbwYuER8ReTB
# w3J64HLnJN+/RpnF78IcV9uDjexNSTCnq47f7Fufr/zdsGbiwZeBe+3W7UvnSSmn
# Eyimp31ngOaKYnhfsi+E11ecXL93KCjx7W3DKI8sj0A3T8HhhUSJxAlMxdSlQy90
# lfdu+HggWCwTXWCVmj5PM4TasIgX3p5O9JawvEagbJjS4NaIjAsCAwEAAaOCAe0w
# ggHpMBAGCSsGAQQBgjcVAQQDAgEAMB0GA1UdDgQWBBRIbmTlUAXTgqoXNzcitW2o
# ynUClTAZBgkrBgEEAYI3FAIEDB4KAFMAdQBiAEMAQTALBgNVHQ8EBAMCAYYwDwYD
# VR0TAQH/BAUwAwEB/zAfBgNVHSMEGDAWgBRyLToCMZBDuRQFTuHqp8cx0SOJNDBa
# BgNVHR8EUzBRME+gTaBLhklodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtpL2Ny
# bC9wcm9kdWN0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNfMjIuY3JsMF4GCCsG
# AQUFBwEBBFIwUDBOBggrBgEFBQcwAoZCaHR0cDovL3d3dy5taWNyb3NvZnQuY29t
# L3BraS9jZXJ0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNfMjIuY3J0MIGfBgNV
# HSAEgZcwgZQwgZEGCSsGAQQBgjcuAzCBgzA/BggrBgEFBQcCARYzaHR0cDovL3d3
# dy5taWNyb3NvZnQuY29tL3BraW9wcy9kb2NzL3ByaW1hcnljcHMuaHRtMEAGCCsG
# AQUFBwICMDQeMiAdAEwAZQBnAGEAbABfAHAAbwBsAGkAYwB5AF8AcwB0AGEAdABl
# AG0AZQBuAHQALiAdMA0GCSqGSIb3DQEBCwUAA4ICAQBn8oalmOBUeRou09h0ZyKb
# C5YR4WOSmUKWfdJ5DJDBZV8uLD74w3LRbYP+vj/oCso7v0epo/Np22O/IjWll11l
# hJB9i0ZQVdgMknzSGksc8zxCi1LQsP1r4z4HLimb5j0bpdS1HXeUOeLpZMlEPXh6
# I/MTfaaQdION9MsmAkYqwooQu6SpBQyb7Wj6aC6VoCo/KmtYSWMfCWluWpiW5IP0
# wI/zRive/DvQvTXvbiWu5a8n7dDd8w6vmSiXmE0OPQvyCInWH8MyGOLwxS3OW560
# STkKxgrCxq2u5bLZ2xWIUUVYODJxJxp/sfQn+N4sOiBpmLJZiWhub6e3dMNABQam
# ASooPoI/E01mC8CzTfXhj38cbxV9Rad25UAqZaPDXVJihsMdYzaXht/a8/jyFqGa
# J+HNpZfQ7l1jQeNbB5yHPgZ3BtEGsXUfFL5hYbXw3MYbBL7fQccOKO7eZS/sl/ah
# XJbYANahRr1Z85elCUtIEJmAH9AAKcWxm6U/RXceNcbSoqKfenoi+kiVH6v7RyOA
# 9Z74v2u3S5fi63V4GuzqN5l5GEv/1rMjaHXmr/r8i+sLgOppO6/8MO0ETI7f33Vt
# Y5E90Z1WTk+/gFcioXgRMiF670EKsT/7qMykXcGhiJtXcVZOSEXAQsmbdlsKgEhr
# /Xmfwb1tbWrJUnMTDXpQzTGCGZ8wghmbAgEBMIGVMH4xCzAJBgNVBAYTAlVTMRMw
# EQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVN
# aWNyb3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNp
# Z25pbmcgUENBIDIwMTECEzMAAAOvMEAOTKNNBUEAAAAAA68wDQYJYIZIAWUDBAIB
# BQCgga4wGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEO
# MAwGCisGAQQBgjcCARUwLwYJKoZIhvcNAQkEMSIEIGqqwcsZRQgou5AfpUtDQyLo
# IcKjGuRYi9vLMNgyIswXMEIGCisGAQQBgjcCAQwxNDAyoBSAEgBNAGkAYwByAG8A
# cwBvAGYAdKEagBhodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20wDQYJKoZIhvcNAQEB
# BQAEggEASJHodX4O5/Y0RieA1pt4GG67aCJUTv5n9z8HCUSShoTqQiGa7HbtH4/9
# lMBFLAJtL8ZE09UR0TvNle9dFYb8qCdGk7zhy0l9Ij1OCxmOj/wLo/47Fx0KkhEP
# c3EzHEL0lDXy7kzDbDJLAMz0iAPiQuXX8+Jo0LnI63uvDkiXfJcey7CdF16Bju9h
# zaXKS387SR2uCAeqejb5E3rttuUlNH8weqvDni0Rqd9jj62L/E05TlA9sI+vj2os
# /xU4EUg8o4jaI5CUIpHBWFEkdcLUbCWAdNTqFhvG0Z75mNUc8Dx8aNISB3rngt/W
# vTcswYSL5+kJV2eBEpb6Mv9nRx05gqGCFykwghclBgorBgEEAYI3AwMBMYIXFTCC
# FxEGCSqGSIb3DQEHAqCCFwIwghb+AgEDMQ8wDQYJYIZIAWUDBAIBBQAwggFZBgsq
# hkiG9w0BCRABBKCCAUgEggFEMIIBQAIBAQYKKwYBBAGEWQoDATAxMA0GCWCGSAFl
# AwQCAQUABCCdNEmKfqztmxWuB9VzLEFiEt791sLq0qd4qgpV4nCM0gIGZYLomtPF
# GBMyMDIzMTIyMTA4MTUwMC40NDlaMASAAgH0oIHYpIHVMIHSMQswCQYDVQQGEwJV
# UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
# ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMS0wKwYDVQQLEyRNaWNyb3NvZnQgSXJl
# bGFuZCBPcGVyYXRpb25zIExpbWl0ZWQxJjAkBgNVBAsTHVRoYWxlcyBUU1MgRVNO
# OjhENDEtNEJGNy1CM0I3MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBT
# ZXJ2aWNloIIReDCCBycwggUPoAMCAQICEzMAAAHj372bmhxogyIAAQAAAeMwDQYJ
# KoZIhvcNAQELBQAwfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24x
# EDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlv
# bjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwHhcNMjMx
# MDEyMTkwNzI5WhcNMjUwMTEwMTkwNzI5WjCB0jELMAkGA1UEBhMCVVMxEzARBgNV
# BAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jv
# c29mdCBDb3Jwb3JhdGlvbjEtMCsGA1UECxMkTWljcm9zb2Z0IElyZWxhbmQgT3Bl
# cmF0aW9ucyBMaW1pdGVkMSYwJAYDVQQLEx1UaGFsZXMgVFNTIEVTTjo4RDQxLTRC
# RjctQjNCNzElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZTCC
# AiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAL6kDWgeRp+fxSBUD6N/yuEJ
# pXggzBeNG5KB8M9AbIWeEokJgOghlMg8JmqkNsB4Wl1NEXR7cL6vlPCsWGLMhyqm
# scQu36/8h2bx6TU4M8dVZEd6V4U+l9gpte+VF91kOI35fOqJ6eQDMwSBQ5c9ElPF
# UijTA7zV7Y5PRYrS4FL9p494TidCpBEH5N6AO5u8wNA/jKO94Zkfjgu7sLF8SUdr
# c1GRNEk2F91L3pxR+32FsuQTZi8hqtrFpEORxbySgiQBP3cH7fPleN1NynhMRf6T
# 7XC1L0PRyKy9MZ6TBWru2HeWivkxIue1nLQb/O/n0j2QVd42Zf0ArXB/Vq54gQ8J
# IvUH0cbvyWM8PomhFi6q2F7he43jhrxyvn1Xi1pwHOVsbH26YxDKTWxl20hfQLdz
# z4RVTo8cFRMdQCxlKkSnocPWqfV/4H5APSPXk0r8Cc/cMmva3g4EvupF4ErbSO0U
# NnCRv7UDxlSGiwiGkmny53mqtAZ7NLePhFtwfxp6ATIojl8JXjr3+bnQWUCDCd5O
# ap54fGeGYU8KxOohmz604BgT14e3sRWABpW+oXYSCyFQ3SZQ3/LNTVby9ENsuEh2
# UIQKWU7lv7chrBrHCDw0jM+WwOjYUS7YxMAhaSyOahpbudALvRUXpQhELFoO6tOx
# /66hzqgjSTOEY3pu46BFAgMBAAGjggFJMIIBRTAdBgNVHQ4EFgQUsa4NZr41Fbeh
# Z8Y+ep2m2YiYqQMwHwYDVR0jBBgwFoAUn6cVXQBeYl2D9OXSZacbUzUZ6XIwXwYD
# VR0fBFgwVjBUoFKgUIZOaHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9j
# cmwvTWljcm9zb2Z0JTIwVGltZS1TdGFtcCUyMFBDQSUyMDIwMTAoMSkuY3JsMGwG
# CCsGAQUFBwEBBGAwXjBcBggrBgEFBQcwAoZQaHR0cDovL3d3dy5taWNyb3NvZnQu
# Y29tL3BraW9wcy9jZXJ0cy9NaWNyb3NvZnQlMjBUaW1lLVN0YW1wJTIwUENBJTIw
# MjAxMCgxKS5jcnQwDAYDVR0TAQH/BAIwADAWBgNVHSUBAf8EDDAKBggrBgEFBQcD
# CDAOBgNVHQ8BAf8EBAMCB4AwDQYJKoZIhvcNAQELBQADggIBALe+my6p1NPMEW1t
# 70a8Y2hGxj6siDSulGAs4UxmkfzxMAic4j0+GTPbHxk193mQ0FRPa9dtbRbaezV0
# GLkEsUWTGF2tP6WsDdl5/lD4wUQ76ArFOencCpK5svE0sO0FyhrJHZxMLCOclvd6
# vAIPOkZAYihBH/RXcxzbiliOCr//3w7REnsLuOp/7vlXJAsGzmJesBP/0ERqxjKu
# dPWuBGz/qdRlJtOl5nv9NZkyLig4D5hy9p2Ec1zaotiLiHnJ9mlsJEcUDhYj8PnY
# nJjjsCxv+yJzao2aUHiIQzMbFq+M08c8uBEf+s37YbZQ7XAFxwe2EVJAUwpWjmtJ
# 3b3zSWTMmFWunFr2aLk6vVeS0u1MyEfEv+0bDk+N3jmsCwbLkM9FaDi7q2HtUn3z
# 6k7AnETc28dAvLf/ioqUrVYTwBrbRH4XVFEvaIQ+i7esDQicWW1dCDA/J3xOoCEC
# V68611jriajfdVg8o0Wp+FCg5CAUtslgOFuiYULgcxnqzkmP2i58ZEa0rm4LZymH
# BzsIMU0yMmuVmAkYxbdEDi5XqlZIupPpqmD6/fLjD4ub0SEEttOpg0np0ra/MNCf
# v/tVhJtz5wgiEIKX+s4akawLfY+16xDB64Nm0HoGs/Gy823ulIm4GyrUcpNZxnXv
# E6OZMjI/V1AgSAg8U/heMWuZTWVUMIIHcTCCBVmgAwIBAgITMwAAABXF52ueAptJ
# mQAAAAAAFTANBgkqhkiG9w0BAQsFADCBiDELMAkGA1UEBhMCVVMxEzARBgNVBAgT
# Cldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29m
# dCBDb3Jwb3JhdGlvbjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJvb3QgQ2VydGlmaWNh
# dGUgQXV0aG9yaXR5IDIwMTAwHhcNMjEwOTMwMTgyMjI1WhcNMzAwOTMwMTgzMjI1
# WjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
# UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYDVQQD
# Ex1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDCCAiIwDQYJKoZIhvcNAQEB
# BQADggIPADCCAgoCggIBAOThpkzntHIhC3miy9ckeb0O1YLT/e6cBwfSqWxOdcjK
# NVf2AX9sSuDivbk+F2Az/1xPx2b3lVNxWuJ+Slr+uDZnhUYjDLWNE893MsAQGOhg
# fWpSg0S3po5GawcU88V29YZQ3MFEyHFcUTE3oAo4bo3t1w/YJlN8OWECesSq/XJp
# rx2rrPY2vjUmZNqYO7oaezOtgFt+jBAcnVL+tuhiJdxqD89d9P6OU8/W7IVWTe/d
# vI2k45GPsjksUZzpcGkNyjYtcI4xyDUoveO0hyTD4MmPfrVUj9z6BVWYbWg7mka9
# 7aSueik3rMvrg0XnRm7KMtXAhjBcTyziYrLNueKNiOSWrAFKu75xqRdbZ2De+JKR
# Hh09/SDPc31BmkZ1zcRfNN0Sidb9pSB9fvzZnkXftnIv231fgLrbqn427DZM9itu
# qBJR6L8FA6PRc6ZNN3SUHDSCD/AQ8rdHGO2n6Jl8P0zbr17C89XYcz1DTsEzOUyO
# ArxCaC4Q6oRRRuLRvWoYWmEBc8pnol7XKHYC4jMYctenIPDC+hIK12NvDMk2ZItb
# oKaDIV1fMHSRlJTYuVD5C4lh8zYGNRiER9vcG9H9stQcxWv2XFJRXRLbJbqvUAV6
# bMURHXLvjflSxIUXk8A8FdsaN8cIFRg/eKtFtvUeh17aj54WcmnGrnu3tz5q4i6t
# AgMBAAGjggHdMIIB2TASBgkrBgEEAYI3FQEEBQIDAQABMCMGCSsGAQQBgjcVAgQW
# BBQqp1L+ZMSavoKRPEY1Kc8Q/y8E7jAdBgNVHQ4EFgQUn6cVXQBeYl2D9OXSZacb
# UzUZ6XIwXAYDVR0gBFUwUzBRBgwrBgEEAYI3TIN9AQEwQTA/BggrBgEFBQcCARYz
# aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9Eb2NzL1JlcG9zaXRvcnku
# aHRtMBMGA1UdJQQMMAoGCCsGAQUFBwMIMBkGCSsGAQQBgjcUAgQMHgoAUwB1AGIA
# QwBBMAsGA1UdDwQEAwIBhjAPBgNVHRMBAf8EBTADAQH/MB8GA1UdIwQYMBaAFNX2
# VsuP6KJcYmjRPZSQW9fOmhjEMFYGA1UdHwRPME0wS6BJoEeGRWh0dHA6Ly9jcmwu
# bWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1Jvb0NlckF1dF8yMDEw
# LTA2LTIzLmNybDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0dHA6Ly93
# d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2VyQXV0XzIwMTAtMDYt
# MjMuY3J0MA0GCSqGSIb3DQEBCwUAA4ICAQCdVX38Kq3hLB9nATEkW+Geckv8qW/q
# XBS2Pk5HZHixBpOXPTEztTnXwnE2P9pkbHzQdTltuw8x5MKP+2zRoZQYIu7pZmc6
# U03dmLq2HnjYNi6cqYJWAAOwBb6J6Gngugnue99qb74py27YP0h1AdkY3m2CDPVt
# I1TkeFN1JFe53Z/zjj3G82jfZfakVqr3lbYoVSfQJL1AoL8ZthISEV09J+BAljis
# 9/kpicO8F7BUhUKz/AyeixmJ5/ALaoHCgRlCGVJ1ijbCHcNhcy4sa3tuPywJeBTp
# kbKpW99Jo3QMvOyRgNI95ko+ZjtPu4b6MhrZlvSP9pEB9s7GdP32THJvEKt1MMU0
# sHrYUP4KWN1APMdUbZ1jdEgssU5HLcEUBHG/ZPkkvnNtyo4JvbMBV0lUZNlz138e
# W0QBjloZkWsNn6Qo3GcZKCS6OEuabvshVGtqRRFHqfG3rsjoiV5PndLQTHa1V1QJ
# sWkBRH58oWFsc/4Ku+xBZj1p/cvBQUl+fpO+y/g75LcVv7TOPqUxUYS8vwLBgqJ7
# Fx0ViY1w/ue10CgaiQuPNtq6TPmb/wrpNPgkNWcr4A245oyZ1uEi6vAnQj0llOZ0
# dFtq0Z4+7X6gMTN9vMvpe784cETRkPHIqzqKOghif9lwY1NNje6CbaUFEMFxBmoQ
# tB1VM1izoXBm8qGCAtQwggI9AgEBMIIBAKGB2KSB1TCB0jELMAkGA1UEBhMCVVMx
# EzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoT
# FU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEtMCsGA1UECxMkTWljcm9zb2Z0IElyZWxh
# bmQgT3BlcmF0aW9ucyBMaW1pdGVkMSYwJAYDVQQLEx1UaGFsZXMgVFNTIEVTTjo4
# RDQxLTRCRjctQjNCNzElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2Vy
# dmljZaIjCgEBMAcGBSsOAwIaAxUAPYiXu8ORQ4hvKcuE7GK0COgxWnqggYMwgYCk
# fjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
# UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYDVQQD
# Ex1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDANBgkqhkiG9w0BAQUFAAIF
# AOkuD8UwIhgPMjAyMzEyMjEwOTEzNDFaGA8yMDIzMTIyMjA5MTM0MVowdDA6Bgor
# BgEEAYRZCgQBMSwwKjAKAgUA6S4PxQIBADAHAgEAAgIHxjAHAgEAAgIRUzAKAgUA
# 6S9hRQIBADA2BgorBgEEAYRZCgQCMSgwJjAMBgorBgEEAYRZCgMCoAowCAIBAAID
# B6EgoQowCAIBAAIDAYagMA0GCSqGSIb3DQEBBQUAA4GBAFazv1AxHrSI37eUSuJt
# ts6rl3+KFs9jvWYSzOmWf6u6/JVmazGzqrSz9ISVGvh8Oi41O0rS2lXzPrFomDBh
# qsWrOAok7WE9Bgb/1A6JHEielVJUnl0Q38u/Z+cOx1TSW+L/pYTVVzRakFVToZzZ
# 97nCOEKO2T+I4DyOU1MPfOgrMYIEDTCCBAkCAQEwgZMwfDELMAkGA1UEBhMCVVMx
# EzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoT
# FU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0IFRpbWUt
# U3RhbXAgUENBIDIwMTACEzMAAAHj372bmhxogyIAAQAAAeMwDQYJYIZIAWUDBAIB
# BQCgggFKMBoGCSqGSIb3DQEJAzENBgsqhkiG9w0BCRABBDAvBgkqhkiG9w0BCQQx
# IgQgQ+Qwvxz/CInftaWQkvUG7VGTm5t8vdhP6b3nwXlw4JswgfoGCyqGSIb3DQEJ
# EAIvMYHqMIHnMIHkMIG9BCAz1COr5bD+ZPdEgQjWvcIWuDJcQbdgq8Ndj0xyMuYm
# KjCBmDCBgKR+MHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
# DgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24x
# JjAkBgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEwAhMzAAAB49+9
# m5ocaIMiAAEAAAHjMCIEIHhm3hCgJLYnGEVwdZvCZdAHvJfiO8NDCwTqi/zTDzy+
# MA0GCSqGSIb3DQEBCwUABIICACOUnitQqm5A+Ti7/XZ+5Hxq+VhhUgvL+fZ8rEIQ
# GIjA+QyUu6xau+chDYBNeR3jH0YDqtjxwcxlgU6T84w/6JsGey7cgJO4ZfULvdPN
# PYfG5jy+1zRaGqBIaDJnQbnq/e0dxXMUvqcwyr1ovCpadGUhG+WZcNL9PY1qtEaJ
# +NeQ6eLOUs6SYB05HTBMuaP/GBPRQbzgK3m6CDs06K+cObJDyLK044hSyTVpbQjJ
# /NyMRtgHnxY2LQ7ylubZDlhlFYTVlDthEKE/pAwQFRTuT2Q0s9pLsbmy7SOpV9n4
# BI7CA7GmOOEa5E+jA8QwNzqHBBS++mafmVXXmGE37sQ7z72BxPJAJfiGdTn6Zynq
# EV/w4VJAaEuPrh3u2CQlt4JoOqnFKksUeAgtGpk7w3iEQ8UY481Z3qSa0nPjiy9z
# HxzQn9S46bW5pj8EBoNg5CUC6M6W3ifKLBhH4L3POgJ+Fr9gsGdNBkpvDAqn5Fz7
# 2ywAkdRa3m3iws+lSMLr+q6VrkdsNfDdlTlGKJG1X+XDqHFs8xHMfM8XAeE3tfc/
# Hk9AayrCkyw4P/sG0dbXStsDFoFoUGsm8Exz5CTEgcJnMM+Vmo1cl98NaTL7oikA
# 6lcOe/DxhxzJFVL8c1DvsXXkwWBY8fPM8o9qX+mF9GKQZoayRqwLMNpB227xPXeH
# JO1m
# SIG # End signature block
