# Xerxes Distributed C++ Build System

## *Overview*

The Xerxes build system is an open source distributed build system for C++ projects running on Microsoft Windows.  The purpose of Xerxes is to speed up large C++ compilations by distributing the compilation of C++ files across multiple computers, similar to Linux's distcc.  The system was designed to mimic the IncrediBuild build system.  

The sytem works with MSBuild and is created to run in a Visual Studio environment on Windows.  The software package contains a graphical user interface and three applications: client application, server application, and build coordinator.  The applications run over TCP/IP on a local network and can be configured to run over the internet.

## *Screenshots*

![Build Gui 0](/buildgui_screenshot0.png "Build Gui 0")
![Build Gui 1](/buildgui_screenshot1.png "Build Gui 1")

## *Contents*

buildgui - The client GUI.
agent_service - The build agent to place on other computers
Proteus - The framework
Romulus - The coordinator layer.
Spartan - The agent service DLL.
