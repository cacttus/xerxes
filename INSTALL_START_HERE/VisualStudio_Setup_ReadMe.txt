SEtting up for visual studio

1) Tools -> External tools -> Add
	Command: C:\p4\dev\proteus\bin\BuildGui.exe
	Arguments: /build
	Initial Directory: C:\p4\dev\proteus\bin\

Add again
	Command: C:\p4\dev\proteus\bin\BuildGui.exe
	Arguments: /cancelbuild
	Initial Directory: C:\p4\dev\proteus\bin\
	
	
Assigning VS keyboard shortcuts
	Tools -> Options -> Environment -> Keyboard
	
	Find Tools.ExternalCommands
	Use the correct command in order from 1...x.  MS should fix this crap and show you teh command id.
	I used ExternalCommand 9 and 10