cd "C:\AgentService"


:: ENABLE FILE AND PRINTER SHARING
netsh advfirewall firewall set rule group="File and Printer Sharing" new enable=Yes

:: OPEN PORTS
netsh advfirewall firewall add rule name="Spartan Client In 58484" dir=in action=allow protocol=TCP localport=58484
netsh advfirewall firewall add rule name="Spartan Client Out 58485" dir=out action=allow protocol=TCP localport=58485


::UNINSTALL
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil /u C:\AgentService\AgentService.exe


::INSTALL
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil C:\AgentService\AgentService.exe 

::Set to restart on failure.
sc failure AgentService reset= 0 actions= restart/2000

:: Start
net start AgentService

services.msc
