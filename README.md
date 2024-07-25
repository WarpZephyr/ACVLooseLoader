# ACVLooseLoader
Automatically loose loads an Armored Core V or Armored Core Verdict Day game.  
It does so for PS3 and Xbox 360.  

# Requirements for use
The game files must be removed from any XISO, ISO, zip, 7z, or rar they are in.   

Make sure the .NET 8.0 runtime is installed or the terminal window will immediately close:  
https://dotnet.microsoft.com/en-us/download/dotnet/8.0  

Select ".NET Desktop Runtime" if you are on Windows and don't mind extra support for other .NET 8.0 programs using UI.  
Select ".NET Runtime" otherwise.  

Most users will need the x64 installer.  
I'm not sure if this program works on other operating systems or CPU architectures.  
This program has only been tested on Windows x64.  

On Windows, clicking on the terminal window breaks the program due to quick edit mode.  
There is now code to call into windows APIs to lock it while in progress.  
I'm not sure if other platforms have similar issues.  
Try not to click the window while the program works, at least until it is fully finished.  

# Steps The Tool Takes
The tool takes several steps to automatically loose load, these can also be done manually.  
The term "root" will refer to the directory where game files begin.  
Here are the steps it takes:  
- Unpack the main archives.  
- Unpack the boot binders and move their contents to root.  
- Unpack the scripts and move their contents to specific locations in root.  
- Unpack the mission binders and move their contents to root.  
- Pack map models and textures into binders for Armored Core V.  
- Rename the main archive header files to hide them from the game.  
- Apply a fix to the FMOD crash for PS3 versions.  

# Notes For Loose Loading
Unpacked files moved around seem to always need to be in lowercase for loose load.  
The tool will automatically lowercase file names when unpacking.  

The main archives are the .bhd and .bdt pairs in the bind folder.  
The .bhd is a header describing the files in the .bdt, which is the data.  
There can be multiple pairs but only ACVD on Xbox 360 uses multiple.  
Script .bhd and .bdt are different and are not the same kind of archive.  

Scripts on PS3 are SDAT encryption and therefore need to be decrypted.  
Scripts need to be moved to specific locations.  
Scripts ending with "scene" need to be moved into the "scene" folder in root.  
Everything else is currently believed to go into the "/airesource/script" folder in root.  

The main archive header files are renamed to hide them from the game.  
If the game cannot find them, it will automatically start loose loading the files.  
The tool simply adds a dash to the beginning of the file name.  
Map models are normally packed into a "_m.dcx.bnd" file, and textures into a "_htdcx.bnd" file.  
ACV has all of those loose in the main archive.  
Loose load requires that they be packed in those specific binders.  
The tool currently leaves the loose files in even if they aren't used.  
The "_l.tpf.dcx" is not packed into map textures and is a separate file containing smaller versions of textures.  

ACV and ACVD will crash on PS3 without a quick fix to an FMOD file called se_weapon.fsb.  
The loose loader will automatically expand its file size by 20,000,000 null bytes which appears to fix the issue.  
The crash can be seen in the TTY log of RPCS3, however you will need to check in the log file to see Japanese text.  
The crash location reported in the normal log is not reliable as it is simply caused by the DL2Panic function which crashes on purpose.  
The crash can be triggered by just about any weapon sound playing in a level, then leaving the level.  

Tools such as extract-xiso can extract Xbox 360 ISOs if needed.  
Tools such as winrar and 7zip can open zip, rar or 7z files if needed.  
Tools such as 7zip can open normal ISOs if needed.  
Tools such as DVDUnbinder can currently unpack ACV and ACVD game files manually from the main archives.  
Tools such as PowerRename from PowerToys can mass rename on windows to lowercase file names.  
Tools such as TrueAncestor Edat Rebuilder can decrypt the PS3 SDAT encrypted scripts manually.  
Tools such as Yabber and WitchyBND can unpack BNDs and various other FromSoftware file formats.  
Yabber or WitchyBND also work for the decrypted scripts, but not the main archives as they are special.  
Tools such as Procmon on windows can see what files the game requests possibly.  
The RPCS3 and Xenia logs may also be useful.  

The main archives have no file names, and instead have hashes of file paths.  
The game will still build paths to request files so names can be discovered that way.  
Unknown files will be placed in the "_unknown" folder in the root folder by the tool.  
The names of the unknown files will be their hashes.  
The FromSoftwareHashTool program can hash file names.  
The hashing function can be found in that tool.  
Slashes used in file paths are always /  
Names are always lowercase.  
The paths always begin with a /  
The paths always start in the root folder of the game files.  
The dictionaries in the tool's resource folder has discovered file names.  
Newly discovered names can be placed there.  
Hash collisions, where multiple names give the same hash, are very common.  
The hashes are only 32-bit, so be very careful of this when checking.  

Several paths in the game have aliases the game resolves to get the full path.  
Aliases can also be used for aliases.  
The files "acv.ini" and "acv2.ini" in the system folder are usually the most useful for checking this.  

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