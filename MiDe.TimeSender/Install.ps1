# Define service name and executable path
$serviceName = "TimeSender"
$executablePath = "./Mide.TimeSender.exe"

# Check if the user has administrative rights
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires administrative privileges. Please run it as an administrator."
    Start-Process powershell -Verb RunAs -ArgumentList ("-File `"$PSCommandPath`"")
    Exit
}

# Create the service
sc.exe create $serviceName binPath= $executablePath DisplayName= $serviceName start= auto obj= ".\LocalSystem" password= ""

# Configure service to restart on failure
sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

# Start the service
Start-Service $serviceName

Write-Host "Service '$serviceName' has been created and started successfully. Press any key to exit."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")