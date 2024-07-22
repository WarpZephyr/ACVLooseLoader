# ACVLooseLoader
Automatically loose loads an Armored Core V or Armored Core Verdict Day game.  
It does so for PS3 and Xbox 360, however Xbox 360 is a bit experimental right now.  

The game will also crash on PS3 without a quick fix to an FMOD file called se_weapon.fsb.  
The loose loader will automatically expand its file size which appears to fix the issue.  

# Requirements for use
The game files must be removed from any XISO, ISO, zip, 7z, or rar they are in.   

Make sure the .NET 8.0 runtime is installed or the terminal window will immediately close:  
https://dotnet.microsoft.com/en-us/download/dotnet/8.0  

Select ".NET Desktop Runtime" if you are on Windows and don't mind extra support for other .NET 8.0 programs using UI.  
Select ".NET Runtime" otherwise.  

Most users will need the x64 installer.  
I'm not sure if this program works on other operating systems or CPU architectures.  
This program has only been tested on Windows x64.  

On Windows, clicking on the terminal window breaks the program do to quick edit mode.  
There is now code to call into windows APIs to lock it while in progress.  
I'm not sure if other platforms have similar issues.  
Try not to click the window while the program works, at least until it is fully finished.  

# Building
If you want to build the project you should clone it with these commands in git bash in a folder of your choosing:  
```
git clone https://github.com/WarpZephyr/ACVLooseLoader.git  
git clone https://github.com/WarpZephyr/BinderHandler.git  
git clone https://github.com/WarpZephyr/SoulsFormatsExtended.git  
git clone https://github.com/WarpZephyr/BinaryMemory.git  
git clone https://github.com/WarpZephyr/libps3.git  
```
Then build ACVLooseLoader.  
Dependencies are subject to possibly change if improvements are made or they are better standardized.