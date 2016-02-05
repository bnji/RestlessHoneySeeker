# PROJECT INFORMATION #

## About ##

Restless Honey Seeker (RHS) is a Remote Access Toolkit (RAT) developed in .NET for for **educational purposes only**.

**This software is not intended for causing any harm to anything or anyone. I - the original author of this software - is not responsible for any usage or damage caused.**

The software has been developed using Visual Studio .NET (C#, JavaScript, HTMl & CSS) and is open source. Anyone is welcome to fork this project or contribute to it. Some things might be broken, features might not be implemented or are buggy, and therefor it might not work as intended/expected.

## Updates ##

**Version 3: A lot of improvements (refactoring), GUI replaced with a web-based console (terminal).**
Version 2: ASP.NET server with improved GUI and functionality.
Version 1: PHP server and C#.NET client.

## History ##

I was inspired after a lot of discussion and media coverage regarding the privacy of people and governments spying on people. This was back in 2012/13. I was studying and had some spare time to kill, so why not just create something. The software (RHS) was initially developed in PHP (Server) and C#.NET (Client). Later I decided to develop the server in ASP.NET. Since then I've tried to continuously update/maintain the project whenever I can.

## Purpose ##
The purpose of this software is to show how easy it is for anyone with a bit of knowledge, time and patience... and a huge amount of coffee, to remotely control a Windows computer with Microsoft .NET Framework 4 Client Profile installed.

**NOTE:** 'Clever' antivirus & -malware programs and/or firewalls should detect RHS or its HTTP requests (and hence block it or notice the user to take an action), but I've noticed most of them actually allow RHS continue running in the background uninterrupted making HTTP requests waiting for a command from the server. One reason might be (is), that on most (commercial) computers, outgoing traffic isn't filtered or monitored as strictly as ingoing traffic. RHS uses port 80 to "phone home" to a C&C (Command-and-Control) server, which controls the "infected" computer (client) or computers (clients).



## Videos (demo) ##
**Restless Honey Seeker 2016.2.4**

[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/QeAcLpp2t5Y/0.jpg)](http://www.youtube.com/watch?v=QeAcLpp2t5Y)

**C# Restless RAT (Remote Access Tool using RESTful services)**

[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/K1roV3RT0Ak/0.jpg)](http://www.youtube.com/watch?v=K1roV3RT0Ak)

## Configuration ##

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/01_04_EnableNuGetPackageRestore.PNG?raw=true "Enable NuGet Package Restore")

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/02_04_ConfigureNuGetRestorePackages.PNG?raw=true "Configure NuGet Restore Packages")

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/03_04_ConfiguringNuGetPackages.PNG?raw=true "Configuring NuGet Packages")

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/04_04_NuGetPackageFinished.PNG?raw=true "NuGet Package Finished")

### Resources & links ###

* http://haishibai.blogspot.dk/2010/01/tiy-try-out-windows-7-uac-using-c-part_26.html
* http://www.codeproject.com/Articles/19004/A-Simple-C-Global-Low-Level-Keyboard-Hook
* http://support.microsoft.com/kb/318804
* http://www.pinvoke.net/default.aspx/user32.sendinput
* http://support.microsoft.com/kb/304283
* http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki
* https://github.com/chrisdone/jquery-console

##3rd party libraries##
* Json.NET (http://www.newtonsoft.com/json)
* WebCameraControl (http://www.codeproject.com/Articles/125478/Versatile-WebCam-C-library)
* SQLite (https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
* ILMerg (http://research.microsoft.com/en-us/people/mbarnett/ILMerge.aspx)
* RestSharp (http://restsharp.org)
* DotNetZip (https://github.com/haf/DotNetZip.Semverd)