# ACVLooseLoader
Automatically loose loads a Armored Core V PS3 game copy.  
This allows Armored Core V to be run out of its main game archive and make it more moddable.  
It also allows patches to be made to the game's files to help it run in RPCS3.  

Drag and drop the EBOOT.BIN file from the game folder into the program exe.  
The path to this will be used to find the needed files.  

# Requirements for use
The game must be removed from any ISO, zip, 7z, or Rar it is in.  

The script files in PS3_GAME/USRDIR/bind/ must be decrypted first.  
The script files in question are script.bhd.sdat and script.bdt.sdat,  
They use PS3 encryption and can be decrypted by tools such as TrueAncestor Edat Rebuilder.  

Once decrypted, or once a decrypted copy of the script files is found,  
Remove any extra extensions and make sure the names are script.bhd and script.bdt,  
Then place them in PS3_GAME/USRDIR/bind/ or this program's "res" folder.  

The game will also crash without a quick fix to se_weapon.fsb in PS3_GAME/USRDIR/sound/  
If you have the fix, place it in the program "res" folder for it to be automatically applied.  
Otherwise, place it in PS3_GAME/USRDIR/sound/ to replace the old one, I recommend backing the old one up anyways.  

Make sure the .NET 8.0 runtime is installed or the terminal window will immediately close:  
https://dotnet.microsoft.com/en-us/download/dotnet/8.0  

Select ".NET Desktop Runtime" if you are on Windows and don't mind extra support for other .NET 8.0 programs using UI.  
Select ".NET Runtime" otherwise.  

Most users will need the x64 installer.  
I'm not sure if this program works on other operating systems or CPU architectures.  
I have only tested it on Windows x64.  

Do not click on the terminal window while it runs, it broke for some reason and I'm not sure why yet.

# Building
If you want to build the project you should clone it with these commands in git bash in a folder of your choosing:  
```
git clone https://github.com/WarpZephyr/ACVLooseLoader.git  
git clone https://github.com/WarpZephyr/BinderHandler.git  
git clone https://github.com/WarpZephyr/SoulsFormatsExtended.git  
```
Then build ACVLooseLoader.  