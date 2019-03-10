cd "C:\AgentService"

:: ENABLE FILE AND PRINTER SHARING
netsh advfirewall firewall set rule group="File and Printer Sharing" new enable=Yes

:: OPEN PORTS
netsh advfirewall firewall add rule name="Trex In 56112" dir=in action=allow protocol=TCP localport=56112
netsh advfirewall firewall add rule name="Trex Out 56112" dir=out action=allow protocol=TCP localport=56112

sc stop TrexService
sc delete TrexService
sc create TrexService start= auto binpath= c:\Trex\TrexService.exe obj= .\John password= Network15

::Set to restart on failure.
sc failure TrexService reset= 0 actions= restart/1000

:: Start
net start TrexService

services.msc

