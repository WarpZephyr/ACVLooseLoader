using System.Diagnostics;
using System.Reflection;
using BinderHandler.Hashes;
using libps3;
using SoulsFormats;
using SFBinder = SoulsFormats.Binder;

namespace ACVLooseLoader
{
    internal class Program
    {
        static readonly string ExecutingPath = Assembly.GetExecutingAssembly().Location;

        const int GameTypeCount = 2;

        static readonly Dictionary<GameType, string> DictionaryNames = new(2)
        {
            { GameType.ArmoredCoreV, "dict-acv.txt" },
            { GameType.ArmoredCoreVD, "dict-acvd.txt" }
        };

        static readonly Logger Logger = new Logger();

        static async Task Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                ConsoleModeManager consoleModeManager = new ConsoleModeManager();
                
                // Prevent program from freezing when window is clicked on
                consoleModeManager.LockConsole();

                // Set up logger for console
                Logger.RegisterConsoleWriter();

                try
                {
                    // Wrap functionality in try catch so user errors can be caught and show a friendlier message without a stacktrace
                    await ProcessArgumentsAsync(args);
                }
                catch (UserErrorException ex)
                {
                    Pause(ex.Message);
                }
                finally
                {
                    // Release lock on quick edit in case the user called from the command line
                    consoleModeManager.UnlockConsole();
                    Logger.Dispose();
                }
#if !DEBUG
            }
            catch (Exception ex)
            {
                Pause(ex.Message);
            }
#endif
        }

        static async Task ProcessArgumentsAsync(string[] args)
        {
            // Show usage
            if (args.Length < 1)
            {
                throw new UserErrorException("This program has no GUI.\n" +
                "Please drag and drop EBOOT.BIN or default.xex from game files or pass it as an argument.\n" +
                "This is used to find your game files.");
            }

            LooseLoaderConfig config = new LooseLoaderConfig();
            LogInfo("Looking for program resource folder...");

            // Determine where program folder is
            string? executingFolder = Path.GetDirectoryName(ExecutingPath);

            // Get program resource folder path and config path
            string resDir;
            string configPath;
            if (string.IsNullOrEmpty(executingFolder))
            {
                resDir = string.Empty;
                configPath = string.Empty;
                LogWarn($"Warning: Could not find folder program is in using execution path: {ExecutingPath}.");
            }
            else
            {
                resDir = Path.Combine(executingFolder, "res");
                configPath = Path.Combine(executingFolder, "config.txt");
            }

            // Load config
            if (configPath != string.Empty && File.Exists(configPath))
            {
                config.Parse(configPath);
                LogInfo("Loaded config.");
            }
            else
            {
                LogWarn("Warning: Could not find config file, defaulting settings.");
            }

            // Check if we need to log to a log file
            if (config.LogToFile)
            {
                if (!string.IsNullOrEmpty(executingFolder))
                {
                    Logger.RegisterFileWriter(Path.Combine(executingFolder, "log.txt"));
                }
                else
                {
                    LogWarn($"Warning: Could not setup logging to log file as program folder could not be found.");
                }
            }

            Dictionary<GameType, BinderHashDictionary> loadedDictionaries = new Dictionary<GameType, BinderHashDictionary>(Math.Min(GameTypeCount, args.Length));
            foreach (string arg in args)
            {
                LogInfo("Processing next argument...");

                // Get and clean path (things such as slashes can mess stuff up)
                string rootDir = PathHelper.CleanPath(arg);

                PlatformType platform;
                GameType game;

                // Determine platform and root
                if (!config.UseDefaultPlatform)
                {
                    LogInfo("Determining root file path and platform...");
                    platform = DeterminePlatform(ref rootDir);
                }
                else
                {
                    LogInfo("Using default platform and determining root path...");
                    platform = config.DefaultPlatform;
                    DetermineRoot(ref rootDir, platform);
                }

                // Determine game
                if (!config.UseDefaultGame)
                {
                    LogInfo("Determining game...");
                    game = DetermineGameType(platform, rootDir);
                }
                else
                {
                    LogInfo("Using default game.");
                    game = config.DefaultGame;
                }

                LogInfo($"Determined platform as {platform} and game as {game}.");

                // Whether or not to skip every step that requires the bind directory
                if (!(config.SkipMainArchiveUnpack && config.SkipBootBinderUnpack && config.SkipMapUnpack && config.SkipScriptUnpack))
                {
                    // Get the bind folder
                    string bindDir = Path.Combine(rootDir, "bind");
                    if (!Directory.Exists(bindDir))
                    {
                        throw new UserErrorException("Error: Could not find bind folder path, you are missing files or an incorrect path was provided.");
                    }

                    // Find dictionary
                    if (!loadedDictionaries.TryGetValue(game, out BinderHashDictionary? dictionary))
                    {
                        // Only check for dictionary in resources if the program resource folder was found
                        if (resDir != string.Empty)
                        {
                            // Try to find dictionary in resources
                            string dictionaryPath = Path.Combine(resDir, DictionaryNames[game]);
                            if (File.Exists(dictionaryPath))
                            {
                                dictionary = BinderHashDictionary.FromPath(dictionaryPath, false);
                                loadedDictionaries.Add(game, dictionary);
                            }
                        }
                    }

                    // Unpack main archives
                    if (!config.SkipMainArchiveUnpack)
                    {
                        // Unpack if a dictionary was found
                        if (dictionary is not null)
                        {
                            if (game == GameType.ArmoredCoreV)
                            {
                                await UnpackDvdBinderAsync(rootDir, bindDir, "dvdbnd5.bhd", "dvdbnd.bdt", dictionary, config.SkipHiddenMainArchiveUnpack, config.SkipExistingFiles, config.SkipUnknownFiles);
                            }
                            else if (game == GameType.ArmoredCoreVD)
                            {
                                await UnpackDvdBinderAsync(rootDir, bindDir, "dvdbnd5_layer0.bhd", "dvdbnd_layer0.bdt", dictionary, config.SkipHiddenMainArchiveUnpack, config.SkipExistingFiles, config.SkipUnknownFiles);
                                if (platform == PlatformType.Xbox360)
                                {
                                    await UnpackDvdBinderAsync(rootDir, bindDir, "dvdbnd5_layer1.bhd", "dvdbnd_layer1.bdt", dictionary, config.SkipHiddenMainArchiveUnpack, config.SkipExistingFiles, config.SkipUnknownFiles);
                                }
                            }
                        }
                        else
                        {
                            LogWarn($"Warning: No dictionary found for {game}, assuming game files are unpacked and continuing.");
                        }
                    }
                    else
                    {
                        LogInfo("Skipping unpacking main archives.");
                    }

                    // Hide headers
                    if (config.HideHeaders)
                    {
                        RenameHeader(bindDir, platform, game);
                    }
                    else
                    {
                        LogInfo("Skipping hiding main archive headers.");
                    }

                    // Unpack boot.bnd and boot_2nd.bnd
                    if (!config.SkipBootBinderUnpack)
                    {
                        LogInfo("Unpacking boot binders...");
                        BinderHelper.MassUnpackBinders(bindDir, rootDir, "boot*", false, true, config.SkipExistingFiles);
                        LogInfo("Unpacked boot binders.");
                    }
                    else
                    {
                        LogInfo("Skipping unpacking boot binders.");
                    }

                    // Unpack scripts
                    if (!config.SkipScriptUnpack)
                    {
                        // Get and check the script binder header and data file paths
                        string scriptHeaderPath = GetScriptBindPath("script.bhd", [rootDir, resDir], platform);
                        string scriptDataPath = GetScriptBindPath("script.bdt", [rootDir, resDir], platform);

                        // Unpack script.bhd and script.bdt
                        LogInfo("Checking scripts...");
                        UnpackScripts(rootDir, scriptHeaderPath, scriptDataPath, platform, config.SkipExistingFiles);
                    }
                    else
                    {
                        LogInfo("Skipping unpacking scripts.");
                    }

                    // Unpack maps
                    if (!config.SkipMapUnpack)
                    {
                        LogInfo("Attempting to unpack maps...");

                        // Get the bind/mission/ folder
                        string missionBindDir = Path.Combine(bindDir, "mission");
                        if (!Directory.Exists(missionBindDir))
                        {
                            throw new UserErrorException(
                                "Error: Could not find mission binder path, game was not unpacked correctly or is missing files.\n" +
                                "Make sure this tool has the dictionary file in the program resources folder to unpack the game.\n" +
                                "Tools such as DVDUnbinder can manually unpack the game.\n" +
                                "Once unpacked, move the unpacked files into:\n" +
                                "- The \"/PS3_GAME/USRDIR/\" folder on PS3 for disc.\n" +
                                "- The \"/USRDIR/\" folder on PS3 for digital.\n" +
                                "- The game folder directly on Xbox 360.");
                        }

                        BinderHelper.MassUnpackBinders(missionBindDir, rootDir, "*.bnd", false, true, config.SkipExistingFiles);
                        LogInfo("Unpacked maps.");
                    }
                }
                else
                {
                    LogInfo("Every step requiring bind folder skipped.");
                }

                // Pack map resources
                if (game == GameType.ArmoredCoreV)
                {
                    if (!config.SkipMapResourcePack)
                    {
                        // Pack map models and textures
                        LogInfo("Packing models and textures in each map...");
                        PackAcvMapResources(Path.Combine(rootDir, "model", "map"), config.SkipExistingFiles);
                        LogInfo("Packed models and textures in each map.");
                    }
                    else
                    {
                        LogInfo("Skipping packing map resources.");
                    }
                }

                // Apply FMOD crash fix
                if (platform == PlatformType.PS3)
                {
                    if (config.ApplyFmodCrashFix)
                    {
                        ApplyFmodCrashFix(rootDir);
                    }
                    else
                    {
                        LogInfo("Skipping applying FMOD crash fix.");
                    }
                }

                LogInfo("Finished processing argument.");
            }

            if (config.PauseOnFinish)
            {
                Pause("Finished.");
            }
            else
            {
                LogInfo("Finished.");
            }
        }

        #region Helpers

        static void DetermineRoot(ref string root, PlatformType platform)
        {
            // Only set if path was a file we are looking for
            static void SetRoot(ref string root, string file)
            {
                if (root != file)
                {
                    root = PathHelper.GetDirectoryName(file, $"Error: Could not get root game folder from path: {root}");
                }
            }

            // Check possible executables
            // If an executable is found set the root directory
            static bool CheckFile(ref string root, string file, PlatformType platform)
            {
                if (File.Exists(file))
                {
                    string name = Path.GetFileName(file);
                    if (platform == PlatformType.PS3)
                    {
                        if (name.Equals("EBOOT.BIN", StringComparison.InvariantCultureIgnoreCase))
                        {
                            SetRoot(ref root, file);
                            return true;
                        }
                        else if (name.EndsWith(".elf", StringComparison.InvariantCultureIgnoreCase))
                        {
                            SetRoot(ref root, file);
                            return true;
                        }
                    }
                    else if (platform == PlatformType.Xbox360)
                    {
                        if (name.EndsWith(".xex", StringComparison.InvariantCultureIgnoreCase))
                        {
                            SetRoot(ref root, file);
                            return true;
                        }
                    }
                }

                return false;
            }

            // Get the possibly directories,
            // Then check the possible executables inside of them
            static bool CheckFolderFile(ref string root, string folder, PlatformType platform)
            {
                if (platform == PlatformType.PS3)
                {
                    if (CheckFile(ref root, Path.Combine(folder, "EBOOT.BIN"), platform))
                    {
                        return true;
                    }

                    if (CheckFile(ref root, Path.Combine(folder, "EBOOT.elf"), platform))
                    {
                        return true;
                    }
                }
                else if (platform == PlatformType.Xbox360)
                {

                    if (CheckFile(ref root, Path.Combine(root, "default.xex"), platform))
                    {
                        return true;
                    }
                }

                return false;
            }

            // Do search
            if (CheckFile(ref root, root, platform))
            {
                return;
            }
            else if (Directory.Exists(root))
            {
                if (CheckFolderFile(ref root, Path.Combine(root, "PS3_GAME", "USRDIR"), platform))
                {
                    return;
                }
                else if (CheckFolderFile(ref root, Path.Combine(root, "USRDIR"), platform))
                {
                    return;
                }
                else if (CheckFolderFile(ref root, root, platform))
                {
                    return;
                }
            }

            throw new UserErrorException($"Cannot determine root path from {nameof(PlatformType)} {platform} and path: {root}");
        }

        static PlatformType DeterminePlatform(ref string root)
        {
            // Only set if path was a file we are looking for
            static void SetRoot(ref string root, string file)
            {
                if (root != file)
                {
                    root = PathHelper.GetDirectoryName(file, $"Error: Could not get root game folder from path: {root}");
                }
            }

            // Check possible executables
            // If an executable is found set the root directory
            static bool CheckFile(ref string root, string file, out PlatformType platform)
            {
                if (File.Exists(file))
                {
                    string name = Path.GetFileName(file);
                    if (name.Equals("EBOOT.BIN", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetRoot(ref root, file);
                        platform = PlatformType.PS3;
                        return true;
                    }
                    else if (name.EndsWith(".xex", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetRoot(ref root, file);
                        platform = PlatformType.Xbox360;
                        return true;
                    }
                    // Less likely
                    else if (name.EndsWith(".elf", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetRoot(ref root, file);
                        platform = PlatformType.PS3;
                        return true;
                    }
                }

                platform = default;
                return false;
            }

            // Get the possibly directories,
            // Then check the possible executables inside of them
            static bool CheckFolderFile(ref string root, string folder, out PlatformType platform)
            {
                if (CheckFile(ref root, Path.Combine(folder, "EBOOT.BIN"), out platform))
                {
                    return true;
                }

                if (CheckFile(ref root, Path.Combine(folder, "EBOOT.elf"), out platform))
                {
                    return true;
                }

                if (CheckFile(ref root, Path.Combine(root, "default.xex"), out platform))
                {
                    return true;
                }

                return false;
            }

            // Do search
            if (CheckFile(ref root, root, out PlatformType platform))
            {
                return platform;
            }
            else if (Directory.Exists(root))
            {
                if (CheckFolderFile(ref root, Path.Combine(root, "PS3_GAME", "USRDIR"), out platform))
                {
                    return platform;
                }
                else if (CheckFolderFile(ref root, Path.Combine(root, "USRDIR"), out platform))
                {
                    return platform;
                }
                else if (CheckFolderFile(ref root, root, out platform))
                {
                    return platform;
                }
            }

            throw new UserErrorException($"Cannot determine {nameof(PlatformType)} from path: {root}");
        }

        static GameType DetermineGameType(PlatformType platform, string rootDir)
        {
            // No inline since both uses appear in the same scope
            GameType game;

            if (platform == PlatformType.PS3)
            {
                LogInfo("Attempting to determine game by PARAM.SFO...");

                // Get the USRDIR folder
                if (rootDir.EndsWith("USRDIR"))
                {
                    // Get the PS3_GAME folder (disc) or root game folder (digital).
                    string? parentDir = Path.GetDirectoryName(rootDir);
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        // Determine which game we are loose loading for by PARAM.SFO
                        string sfoPath = Path.Combine(parentDir, "PARAM.SFO");
                        if (File.Exists(sfoPath)
                            && PARAMSFO.IsRead(sfoPath, out PARAMSFO? sfo)
                            && TryDetermineGameBySFO(sfo, out game))
                        {
                            return game;
                        }
                    }
                }

                LogWarn("Warning: PARAM.SFO could not be found or was invalid.");
            }

            // Determine which game we are loose loading for by files
            LogInfo("Attempting to determine game by checking files...");
            if (TryDetermineGameByFiles(rootDir, out game))
            {
                return game;
            }

            throw new UserErrorException($"Game could not be determined from {nameof(PlatformType)} {platform} and path: {rootDir}");
        }

        static bool TryDetermineGameByFiles(string rootDir, out GameType game)
        {
            string bindDir = Path.Combine(rootDir, "bind");
            string acvPath = Path.Combine(bindDir, "dvdbnd.bdt");
            if (File.Exists(acvPath))
            {
                game = GameType.ArmoredCoreV;
                return true;
            }

            string acvdPath = Path.Combine(bindDir, "dvdbnd_layer0.bdt");
            if (File.Exists(acvdPath))
            {
                game = GameType.ArmoredCoreVD;
                return true;
            }

            game = default;
            return false;
        }

        static bool TryDetermineGameBySFO(PARAMSFO sfo, out GameType game)
        {
            // Try to find the title name
            if (sfo.Parameters.TryGetValue("TITLE", out PARAMSFO.Parameter? parameter))
            {
                switch (parameter.Data)
                {
                    case "ARMORED CORE V":
                        game = GameType.ArmoredCoreV;
                        return true;
                    case "Armored Core Verdict Day":
                        game = GameType.ArmoredCoreVD;
                        return true;
                }
            }

            // Try to find the title ID
            if (sfo.Parameters.TryGetValue("TITLE_ID", out parameter))
            {
                switch (parameter.Data)
                {
                    case "BLKS20356":
                    case "BLAS50448":
                    case "BLJM60378":
                    case "BLUS30516":
                    case "BLES01440":
                        game = GameType.ArmoredCoreV;
                        return true;
                    case "BLKS20441":
                    case "BLAS50618":
                    case "BLJM61014":
                    case "BLJM61020":
                    case "BLUS31194":
                    case "BLES01898":
                    case "NPUB31245":
                    case "NPEB01428":
                        game = GameType.ArmoredCoreVD;
                        return true;
                }
            }

            game = default;
            return false;
        }

        static async Task UnpackDvdBinderAsync(string rootDir, string bindDir, string bhdName, string bdtName, BinderHashDictionary dictionary, bool skipHidden, bool skipExisting, bool skipUnknown)
        {
            string bhdPath = Path.Combine(bindDir, bhdName);
            if (!File.Exists(bhdPath))
            {
                bhdPath = Path.Combine(bindDir, '-' + bhdName);
                if (File.Exists(bhdPath))
                {
                    if (skipHidden)
                    {
                        LogInfo($"Skipping unpacking hidden {bhdName}");
                        return;
                    }

                    LogWarn($"Warning: Found hidden {bhdName}, game files from {bhdName} may already be unpacked.");
                }
                else
                {
                    LogWarn($"Warning: Could not find {bhdName}, assuming game files from {bhdName} are unpacked already.");
                    return;
                }
                
            }

            string bdtPath = Path.Combine(bindDir, bdtName);
            if (!File.Exists(bdtPath))
            {
                LogWarn($"Warning: Could not find {bdtName}, assuming game files from {bhdName} are unpacked already.");
                return;
            }

            LogInfo($"Unpacking game files from {bhdName} archive...");

            var binder = BinderHandler.Binder.FromPathToBinderHeader5(bhdPath, null, BHD5.Game.DarkSouls1, dictionary);
            binder.SkipExistingFiles = skipExisting;
            binder.SkipUnknownFiles = skipUnknown;

            using (var pb = new ConsoleProgressBar())
            {
                await binder.UnpackDataFromPathAsync(bdtPath, rootDir, pb, new CancellationTokenSource().Token);
            }

            Logger.WriteTo("Console", " Done.\n");
            LogInfo($"Unpacked game files from {bhdName} archive.");
        }

        static void PackAcvMapResources(string dir, bool skipExisting)
        {
            foreach (var directory in Directory.EnumerateDirectories(dir, "m*", SearchOption.TopDirectoryOnly))
            {
                LogInfo($"Packing map models and textures in {Path.GetFileNameWithoutExtension(directory)}...");
                PackAcvMap(directory, skipExisting);
            }
        }

        static void PackAcvMap(string dir, bool skipExisting)
        {
            static void SetAcvMapBinderInfo(BND3 binder)
            {
                binder.Version = "JP100";
                binder.Compression = DCX.Type.None;
                binder.Format = SFBinder.Format.IDs | SFBinder.Format.Names1 | SFBinder.Format.Compression;
                binder.BitBigEndian = true;
                binder.BigEndian = true;
                binder.Unk18 = 0;

                for (int i = 0; i < binder.Files.Count; i++)
                {
                    binder.Files[i].Flags = SFBinder.FileFlags.Flag1;
                    binder.Files[i].ID = i;
                }
            }

            string mapID = Path.GetFileName(dir);
            string modelBNDPath = Path.Combine(dir, $"{mapID}_m.dcx.bnd");
            if (!(skipExisting && File.Exists(modelBNDPath)))
            {
                var modelBND = BinderHelper.PackFilesIntoBinder3(dir, [".flv", ".hmd", ".smd", ".mlb"], false);
                SetAcvMapBinderInfo(modelBND);
                modelBND.Write();
            }
            else
            {
                LogInfo($"Skipping packing existing model files for {mapID}");
            }

            string textureBNDPath = Path.Combine(dir, $"{mapID}_htdcx.bnd");
            if (!(skipExisting && File.Exists(modelBNDPath)))
            {
                var textureBND = BinderHelper.PackFilesIntoBinder3(dir, ".tpf.dcx", "_l.tpf.dcx", false);
                SetAcvMapBinderInfo(textureBND);
                textureBND.Write();
            }
            else
            {
                LogInfo($"Skipping packing existing texture files for {mapID}");
            }
        }

        static string GetScriptBindPath(string name, Span<string> roots, PlatformType platform)
        {
            foreach (string root in roots)
            {
                string scriptPath = Path.Combine(root, "bind", name);
                if (File.Exists(scriptPath))
                {
                    LogInfo($"Found {name} path");
                    return scriptPath;
                }
                else if (platform == PlatformType.PS3)
                {
                    string scriptSdatPath = scriptPath + ".sdat";
                    if (File.Exists(scriptSdatPath))
                    {
                        LogInfo($"Found encrypted {name} path");
                        return scriptSdatPath;
                    }
                }
            }

            LogWarn($"Warning: Could not find {name} path, you may be missing files.");
            return string.Empty;
        }

        static void UnpackScripts(string usrDir, string headerPath, string dataPath, PlatformType platform, bool skipExisting)
        {
            string aiScriptDir = Path.Combine(usrDir, "airesource", "script");
            string sceneScriptDir = Path.Combine(usrDir, "scene");

            if (string.IsNullOrEmpty(headerPath) || string.IsNullOrEmpty(dataPath))
            {
                LogWarn("Warning: Scripts header or data file path could not be determined, skipping script unpacking.");
            }

            // Check for PS3 SDAT encryption
            if (platform == PlatformType.PS3)
            {
                LogInfo("Checking scripts for encryption...");
                if (headerPath.EndsWith(".sdat") && NPD.Is(headerPath))
                {
                    LogInfo("Decrypting scripts header file...");

                    string headerSdatPath = headerPath;
                    headerPath = headerPath.Replace(".sdat", string.Empty); // Replace path with removed .sdat
                    EDAT.DecryptSdatFile(headerSdatPath, headerPath);
                    LogInfo("Decrypted scripts header file.");
                }

                if (dataPath.EndsWith(".sdat") && NPD.Is(dataPath))
                {
                    LogInfo("Decrypting scripts data file...");

                    string dataSdatPath = dataPath;
                    dataPath = dataPath.Replace(".sdat", string.Empty); // Replace path with removed .sdat
                    EDAT.DecryptSdatFile(dataSdatPath, dataPath);
                    LogInfo("Decrypted scripts data file.");
                }
            }

            // Check header file
            if (!BXF3.IsHeader(headerPath))
            {
                throw new UserErrorException($"Script header file is not a BHF3: {headerPath}");
            }

            // Data file is not always guaranteed to have a "BDF3" header and so on.
            // So checking that one is skipped.

            LogInfo("Unpacking scripts...");
            BXF3 binder = BXF3.Read(headerPath, dataPath);
            foreach (var file in binder.Files)
            {
                string name = file.Name.ToLowerInvariant();
                if (name.EndsWith("scene.lc"))
                {
                    string path = Path.Combine(sceneScriptDir, name);
                    if (skipExisting && File.Exists(path))
                    {
                        continue;
                    }

                    string? dir = PathHelper.GetDirectoryName(path, $"Error: Could not get folder name for scene script: {name}");
                    Directory.CreateDirectory(dir);

                    File.WriteAllBytes(path, file.Bytes);
                }
                else
                {
                    string path = Path.Combine(aiScriptDir, name);
                    if (skipExisting && File.Exists(path))
                    {
                        continue;
                    }

                    Directory.CreateDirectory(PathHelper.GetDirectoryName(path, $"Error: Could not get folder name for AI script: {name}"));
                    File.WriteAllBytes(path, file.Bytes);
                }
            }

            LogInfo("Unpacked scripts.");
        }

        static void RenameHeader(string bindDir, PlatformType platform, GameType game)
        {
            static void Rename(string bindDir, string name)
            {
                string path = Path.Combine(bindDir, name);
                if (!File.Exists(path))
                {
                    return;
                }

                if (!name.StartsWith('-'))
                {
                    LogInfo($"Renaming {name} to ensure game does not find it...");

                    string newPath = Path.Combine(bindDir, $"-{name}");
                    File.Move(path, newPath);
                    LogInfo($"Renamed {name} to -{name}");
                }
            }

            if (game == GameType.ArmoredCoreV)
            {
                Rename(bindDir, "dvdbnd5.bhd");
            }
            else if(game == GameType.ArmoredCoreVD)
            {
                Rename(bindDir, "dvdbnd5_layer0.bhd");
                if (platform == PlatformType.Xbox360)
                {
                    Rename(bindDir, "dvdbnd5_layer1.bhd");
                }
            }
        }

        static void ApplyFmodCrashFix(string usrDir)
        {
            string soundDir = Path.Combine(usrDir, "sound");
            string seWeaponPath = Path.Combine(soundDir, "se_weapon.fsb");
            FileInfo soundFI = new FileInfo(seWeaponPath);

            int expandLength = 20_000_000;
            if (soundFI.Exists && soundFI.Length < expandLength)
            {
                LogInfo("Expanding se_weapon.fsb to fix fmod crash...");
                Expand(seWeaponPath, expandLength);
                LogInfo("Expanded se_weapon.fsb");
            }
        }

        #endregion

        #region Utilities

        static void Expand(string path, int length, int chunkSize = 65536)
        {
            using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read, chunkSize);

            int totalChunks = length / chunkSize;
            for (int i = 0; i < totalChunks; i++)
            {
                fs.Write(new byte[chunkSize], 0, chunkSize);
                length -= chunkSize;
            }

            fs.Write(new byte[length], 0, length);
        }

        static void Pause(string message)
        {
            Log(message);
            Pause();
        }

        static void Pause()
        {
            // Discard each key from the buffer so we pause when we want to using Console.ReadKey
            // While there are characters in the input stream 
            while (Console.KeyAvailable)
            {
                // Read them and ignore them
                Console.ReadKey(true); // true hides input
            }

            // Now read the next available character to pause
            Console.ReadKey(true); // true hides input
        }

        static void Log(string str)
            => Logger.WriteLine(str);

        [Conditional("DEBUG")]
        static void LogDebug(string str)
            => Logger.WriteDebugLine(str);

        static void LogInfo(string str)
            => Logger.WriteInfoLine(str);

        static void LogWarn(string str)
            => Logger.WriteWarnLine(str);

        static void LogError(string str)
            => Logger.WriteErrorLine(str);

        #endregion
    }
}
