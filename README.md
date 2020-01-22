# Xerxes Distributed C++ Build System

## *Overview*

The Xerxes build system is an open source distributed build system for C++ projects running on Microsoft Windows.  The purpose of Xerxes is to speed up large C++ compilations by distributing the compilation of C++ files across multiple computers, processors, and processor cores, similar to Linux's distcc.  The system was designed to mimic the IncrediBuild build system.

The sytem works with MSBuild and runs in a Visual Studio environment on Windows.  The software package contains a graphical user interface and three applications: client application, server application, and build coordinator.  The applications run over TCP/IP on a local network and can be configured to run over the internet.  Each participating computer needs an installation of the latest visual C++ redistributable.

## *Screenshots*

![Build Gui 0](/buildgui_screenshot0.png "Build Gui 0")
![Build Gui 1](/buildgui_screenshot1.png "Build Gui 1")

## *Future Additions*

There are a few things that need to be added to Xerxes to make it more usable
* I started working on a Visual STudio plugin for this software but as of yet was unable to make it fully functional.  The visual studio plugin would definitely help a lot of people out, and in fact, is probably one of the main reasons why someone would use Xerxes over other systems in the first place.
* The installer for this software does not work correctly.  In the future we need to add a nice installer.  
* We will neeed to update the GUI, as it's not very modern.  
* We need to release this software as an executable and provide a website.

 
