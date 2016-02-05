# PROJECT INFORMATION #

## About ##

The software has solely been developed for educational purposes.

Everything is developed in Visual Studio using C# .NET. You can disagree with this, as it's not the 'optimal' solution for the intended usage, but of course the client can be written in c++ and I'm actually intending to do that in the nearest future, but for now you can rely on the C# client. 

Note: this is a BETA version (some things might be broken or not yet work at all or as expected). The original version was also a beta version, but as I have re-written alot of the code and still heavlity in the need of testing, it's still considered a beta version. Much of the functionality might still be broken.

The purpose of this software is to show how easy it is in reality for anyone to install a rootkit on Windows computer (with .NET 3.5 Client profile installed), which by the way would not be detected by any firewall or antivirus software, as it uses HTTP (port 80) to "phone home" to a central C&C (Command-and-Control) server, which controls the "infected" computer or computers.

The software has now been released as open-source. Anyone who wants to build upon this is welcome to do so. 

## Videos (demo) ##
**Restless Honey Seeker 2016.2.4**
[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/QeAcLpp2t5Y/0.jpg)](http://www.youtube.com/watch?v=QeAcLpp2t5Y)

**C# Restless RAT (Remote Access Tool using RESTful services)**
[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/K1roV3RT0Ak/0.jpg)](http://www.youtube.com/watch?v=K1roV3RT0Ak)

Note: This software is not intended for causing any harm to anything or anyone. I - the author of this software - is not responsible for any usage whatsoever.

## Configuration ##

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/01_04_EnableNuGetPackageRestore.PNG?raw=true "Enable NuGet Package Restore")

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/02_04_ConfigureNuGetRestorePackages.PNG?raw=true "Configure NuGet Restore Packages")

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/03_04_ConfiguringNuGetPackages.PNG?raw=true "Configuring NuGet Packages")

![alt text](https://raw.githubusercontent.com/bnji/RestlessHoneySeeker/master/Screenshots/04_04_NuGetPackageFinished.PNG?raw=true "NuGet Package Finished")

 ~~The software has solely been developed for educational purposes. The client is developed in Visual Studio using C# .NET. On the server side the software is built with PHP, jQuery, HTML & CSS. Note: this is a BETA version (some things might be broken or not yet work at all or as expected).
The purpose of this software is to show how easy it is in reality for anyone to install a rootkit on Windows computer (with .NET 3.5 Client profile installed), which by the way would not be detected by any firewall or antivirus software, as it uses HTTP (port 80) to "phone home" to a central C&C (Command-and-Control) server, which controls the "infected" computer or computers.
The software will soon be released as open-source, so anyone who wants to build upon this is welcome to do so. Some 3rd party libraries have been used and will be disclosed in the README file when the project has been released. This software is not intended for causing any harm to anything or anyone.~~


### Resources & links ###

* http://haishibai.blogspot.dk/2010/01/tiy-try-out-windows-7-uac-using-c-part_26.html
* http://www.codeproject.com/Articles/19004/A-Simple-C-Global-Low-Level-Keyboard-Hook
* http://support.microsoft.com/kb/318804
* http://www.pinvoke.net/default.aspx/user32.sendinput
* http://support.microsoft.com/kb/304283
* http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki
* https://github.com/chrisdone/jquery-console

##3rd party libraries##
* SharpZipLib
* Json.NET (http://www.newtonsoft.com/json)
* WebCameraControl (http://www.codeproject.com/Articles/125478/Versatile-WebCam-C-library)
* SQLite (https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
* ILMerg (http://research.microsoft.com/en-us/people/mbarnett/ILMerge.aspx)
* RestSharp (http://restsharp.org)
* DotNetZip (https://github.com/haf/DotNetZip.Semverd)
