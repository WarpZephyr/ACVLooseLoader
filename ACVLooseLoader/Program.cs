using SoulsFormats;
using SoulsFormats.Other.PlayStation3;
using System.Reflection;
using System.Runtime.CompilerServices;
using SFBinder = SoulsFormats.Binder;

namespace ACVLooseLoader
{
    internal class Program
    {
        static readonly string ExecutingPath = Assembly.GetExecutingAssembly().Location;

        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Usage();
                return;
            }

            var executingFolder = Path.GetDirectoryName(ExecutingPath);
            if (string.IsNullOrEmpty(executingFolder))
            {
                Error($"Error: Could not find folder for program using executing path: {ExecutingPath}");
                return;
            }

            string path = args[0];
            if (!path.EndsWith("EBOOT.BIN", StringComparison.InvariantCultureIgnoreCase))
            {
                Error($"Error: File is not named EBOOT.BIN: {path}");
                return;
            }

            if (!File.Exists(path))
            {
                Error($"Error: EBOOT.BIN file does not exist: {path}");
                return;
            }

            string? usrDir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(usrDir) || !usrDir.EndsWith("USRDIR"))
            {
                Error($"Error: USRDIR folder could not be found for path: {path}");
                return;
            }

            string? ps3GameDir = Path.GetDirectoryName(usrDir);
            if (string.IsNullOrEmpty(ps3GameDir) || !ps3GameDir.EndsWith("PS3_GAME"))
            {
                Error($"Error: PS3_GAME folder could not be found for path: {path}");
                return;
            }

            string paramSFOPath = Path.Combine(ps3GameDir, "PARAM.SFO");
            if (!File.Exists(paramSFOPath))
            {
                Error($"Error: PARAM.SFO could not be found in PS3_GAME folder: {ps3GameDir}");
                return;
            }

            try
            {
                if (PARAMSFO.IsRead(paramSFOPath, out PARAMSFO sfo))
                {
                    if (!IsAcvSFO(sfo))
                    {
                        Error($"Error: Could not identify PARAM.SFO game as Armored Core V: {paramSFOPath}");
                        return;
                    }
                }
                else
                {
                    Error($"Error: PARAM.SFO file was not a valid PARAM.SFO: {paramSFOPath}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Error($"Error: PARAM.SFO checking failed: {paramSFOPath}\n{ex}");
                return;
            }

            string bindDir = Path.Combine(usrDir, "bind");
            if (!Directory.Exists(bindDir))
            {
                Error("Error: Could not find bind folder path, please unpack the game using DVDUnbinder or another tool first,\n" +
                    "Then move the unpacked files into the \"PS3_GAME/USRDIR/\" folder.");
                return;
            }

            string resDir = Path.Combine(executingFolder, "res");
            string dictionaryPath = Path.Combine(resDir, "dict-acv.txt");

            try
            {
                if (File.Exists(dictionaryPath))
                {
                    // Unpack game files
                    await UnpackGameAsync(usrDir, bindDir, dictionaryPath);
                }
                else
                {
                    Log("Warning: Cannot find dictionary for game file names in program resources, assuming game is unpacked and continuing.");
                }
            }
            catch (Exception ex)
            {
                Error($"Error: Failed trying to unpack game files:\n{ex}");
            }

            string missionPath = Path.Combine(bindDir, "mission");
            if (!Directory.Exists(missionPath))
            {
                Error("Error: Could not find mission binder path, game has not unpacked correctly,\n" +
                    "Make sure this tool has the dictionary file in resources to unpack the game,\n" +
                    "Or use tools such as DVDUnbinder to manually unpack the game,\n" +
                    "Then move the unpacked files into the \"PS3_GAME/USRDIR/\" folder.");
                return;
            }

            string scriptHeaderPath = Path.Combine(bindDir, "script.bhd");
            string scriptHeaderSdatPath = scriptHeaderPath + ".sdat";
            if (!File.Exists(scriptHeaderPath))
            {
                string scriptHeaderResPath = Path.Combine(resDir, "script.bhd");
                if (File.Exists(scriptHeaderResPath))
                {
                    Log("Found decrypted script header file in program resources...");
                    scriptHeaderPath = scriptHeaderResPath;
                }
                else
                {
                    if (File.Exists(scriptHeaderSdatPath))
                    {
                        Error("Error: Scripts are still encrypted, could not find decrypted script.bhd.\n" +
                            "Please decrypt it first with a tool such as TrueAncestor Edat Rebuilder or find a decrypted copy.\n" +
                            "Place script.bhd into the \"PS3_GAME/USRDIR/bind/\" folder or the program \"res\" folder.");
                        return;
                    }

                    Error($"Error: Could not find scripts header file script.bhd or its encrypted counterpart script.bhd.sdat.\n You may be missing files.");
                    return;
                }
            }

            string scriptDataPath = Path.Combine(bindDir, "script.bdt");
            string scriptDataSdatPath = scriptDataPath + ".sdat";
            if (!File.Exists(scriptDataPath))
            {
                string scriptDataResPath = Path.Combine(resDir, "script.bdt");
                if (File.Exists(scriptDataResPath))
                {
                    Log("Found decrypted script data file in program resources...");
                    scriptDataPath = scriptDataResPath;
                }
                else
                {
                    if (File.Exists(scriptDataSdatPath))
                    {
                        Error("Error: Scripts are still encrypted, could not find decrypted script.bdt.\n" +
                            "Please decrypt it first with a tool such as TrueAncestor Edat Rebuilder or find a decrypted copy.\n" +
                            "Place script.bdt into the \"PS3_GAME/USRDIR/bind/\" folder or the program \"res\" folder.");
                        return;
                    }

                    Error($"Error: Could not find scripts data file script.bdt or its encrypted counterpart script.bdt.sdat.\n You may be missing files.");
                    return;
                }
            }

            string dvdbhdPath = Path.Combine(bindDir, "dvdbnd5.bhd");
            bool needsRename = File.Exists(dvdbhdPath);

            try
            {
                // Unpack maps
                Log("Unpacking maps...");
                BinderHelper.MassUnpackBinders(missionPath, usrDir, false, true);

                // Pack map models and textures
                Log("Packing models and textures in each map...");
                PackAcvMapResources(Path.Combine(usrDir, "model", "map"));

                // Unpack boot.bnd and boot_2nd.bnd
                Log("Unpacking boot binders...");
                BinderHelper.MassUnpackBinders(bindDir, usrDir, false, true);

                // Unpack script.bhd and script.bdt
                Log("Unpacking scripts...");
                UnpackScripts(usrDir, scriptHeaderPath, scriptDataPath);

                if (needsRename)
                {
                    Log("Renaming dvdbnd5.bhd to ensure game does not find it...");
                    RenameHeader(bindDir, dvdbhdPath);
                }
            }
            catch (Exception ex)
            {
                Error($"Error: An error has occurred while loose loading:\n{ex}");
                return;
            }

            Log("Finished.");
            Log("Make sure you apply the sound fix for se_weapon.fsb in:\n" +
                "[YOUR RPCS3 FOLDER]/dev_hdd0/game/[YOUR GAME REGION CODE]/USRDIR/sound/\n" +
                "An example path might look like:\n" +
                "RPCS3/dev_hdd0/game/BLUS30516/USRDIR/sound/");
            Pause();
        }

        static async Task UnpackGameAsync(string usrDir, string bindDir, string dictionaryPath)
        {
            string bhdPath = Path.Combine(bindDir, "dvdbnd5.bhd");
            if (!File.Exists(bhdPath))
            {
                Log("Could not find dvdbnd5.bhd, assuming game files are unpacked already...");
                return;
            }

            string bdtPath = Path.Combine(bindDir, "dvdbnd.bdt");
            if (!File.Exists(bdtPath))
            {
                Log("Could not find dvdbnd.bdt, assuming game files are unpacked already...");
                return;
            }

            Log("Attempting to unpack game files from main archive...");

            int maxProgress = Console.WindowWidth - 1;
            int lastProgress = 0;
            void ReportProgress(double value)
            {
                int nextProgress = (int)Math.Ceiling(value * maxProgress);
                if (nextProgress > lastProgress)
                {
                    for (int i = lastProgress; i < nextProgress; i++)
                    {
                        if (i == 0)
                            Console.Write('[');
                        else if (i == maxProgress - 1)
                            Console.Write(']');
                        else
                            Console.Write('=');
                    }
                    lastProgress = nextProgress;
                }
            }

            var cts = new CancellationTokenSource();
            await BinderHandler.Binder.UnpackFromPathsAsync(bhdPath, bdtPath, dictionaryPath, usrDir, BHD5.Game.DarkSouls1, null, new Progress<double>(ReportProgress), cts.Token);
        }

        static bool IsAcvSFO(PARAMSFO sfo)
        {
            // Try to find the title name
            if (sfo.Parameters.TryGetValue("TITLE", out PARAMSFO.Parameter? parameter))
            {
                switch (parameter.Data)
                {
                    case "ARMORED CORE V":
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
                        return true;
                }
            }

            return false;
        }

        static void PackAcvMapResources(string dir)
        {
            foreach (var directory in Directory.EnumerateDirectories(dir, "m*", SearchOption.TopDirectoryOnly))
            {
                Log($"Packing map models and textures in {Path.GetFileNameWithoutExtension(directory)}...");
                PackAcvMap(directory);
            }
        }

        static void PackAcvMap(string dir)
        {
            var modelBND = BinderHelper.PackFilesIntoBinder3(dir, [".flv", ".hmd", ".smd", ".mlb"], false);
            var textureBND = BinderHelper.PackFilesIntoBinder3(dir, false, ".tpf.dcx");
            SetAcvMapBinderInfo(modelBND);
            SetAcvMapBinderInfo(textureBND);
            string mapID = dir.Split(Path.DirectorySeparatorChar).Last();
            modelBND.Write(Path.Combine(dir, $"{mapID}_m.dcx.bnd"));
            textureBND.Write(Path.Combine(dir, $"{mapID}_htdcx.bnd"));
        }

        static void SetAcvMapBinderInfo(BND3 binder)
        {
            binder.Version = "JP100";
            binder.Compression = DCX.Type.None;
            binder.Format = SFBinder.Format.IDs | SFBinder.Format.Names1 | SFBinder.Format.Compression;
            binder.BitBigEndian = true;
            binder.BigEndian = true;
            binder.Unk18 = 0;

            int fileID = 0;
            for (int i = 0; i < binder.Files.Count; i++)
            {
                binder.Files[i].Flags = SFBinder.FileFlags.Flag1;
                binder.Files[i].ID = fileID;
                fileID++;
            }
        }

        static void UnpackScripts(string usrDir, string headerPath, string dataPath)
        {
            string aiScriptDir = Path.Combine(usrDir, "airesource", "script");
            string sceneScriptDir = Path.Combine(usrDir, "scene");

            if (File.Exists(aiScriptDir))
            {
                throw new InvalidOperationException($"AI Script path must not be a file: {aiScriptDir}");
            }

            if (File.Exists(sceneScriptDir))
            {
                throw new InvalidOperationException($"Scene Script path must not be a file: {sceneScriptDir}");
            }

            if (!BXF3.IsHeader(headerPath))
            {
                throw new InvalidDataException($"Script header file is not a BHF3: {headerPath}");
            }

            if (!BXF3.IsData(dataPath))
            {
                throw new InvalidDataException($"Script data file is not a BDF3: {dataPath}");
            }

            BXF3 binder = BXF3.Read(headerPath, dataPath);
            foreach (var file in binder.Files)
            {
                string name = file.Name.ToLowerInvariant();
                if (name.EndsWith("scene.lc"))
                {
                    string path = Path.Combine(sceneScriptDir, name);
                    Directory.CreateDirectory(PathHandler.GetDirectoryName(path, $"Error: Could not get folder name for scene script: {name}"));
                    File.WriteAllBytes(path, file.Bytes);
                }
                else
                {
                    string path = Path.Combine(aiScriptDir, name);
                    Directory.CreateDirectory(PathHandler.GetDirectoryName(path, $"Error: Could not get folder name for AI script: {name}"));
                    File.WriteAllBytes(path, file.Bytes);
                }
            }
        }

        static void RenameHeader(string bindDir, string headerPath)
        {
            if (!File.Exists(headerPath))
            {
                return;
            }

            string newName = "-dvdbnd5.bhd";
            string newPath = Path.Combine(bindDir, newName);
            while (File.Exists(newPath))
            {
                newName = '-' + newName;
                newPath = Path.Combine(newPath, newName);
            }

            File.Move(headerPath, newPath);
        }

        static void Usage()
        {
            Log("This program has no GUI.\n" +
                "Please drag and drop EBOOT.BIN from game files or pass it as an argument.\n" +
                "This is used to find your game files.");
            Pause();
        }

        static void Error(string str)
        {
            Log(str);
            Pause();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Log(string str)
            => Console.WriteLine(str);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Pause()
            => Console.ReadKey();
    }
}
