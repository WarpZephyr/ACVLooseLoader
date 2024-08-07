﻿using SoulsFormats;

namespace ACVLooseLoader
{
    public static class BinderHelper
    {
        public static void MassUnpackBinders(string from, string to, string wildcard = "*.bnd", bool recursiveSearch = false, bool lowerCaseNames = true, bool skipExisting = false)
        {
            if (!Directory.Exists(from))
            {
                throw new DirectoryNotFoundException($"Directory to unpack binders from must exist: {from}");
            }

            if (File.Exists(to))
            {
                throw new InvalidOperationException($"Path to unpack binders to must be a directory, not a file: {to}");
            }

            var searchOption = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            Directory.CreateDirectory(to);
            foreach (var binderPath in Directory.EnumerateFiles(from, wildcard, searchOption))
            {
                if (BND3.IsRead(binderPath, out BND3 binder))
                {
                    UnpackBinderTo(binder, to, lowerCaseNames, skipExisting);
                }
            }
        }

        public static void UnpackBinderTo(IBinder binder, string to, bool lowerCaseName = true, bool skipExisting = false)
        {
            foreach (var file in binder.Files)
            {
                string name = file.Name;
                if (lowerCaseName)
                {
                    name = name.ToLowerInvariant();
                }

                string path = Path.Combine(to, name);
                if (skipExisting && File.Exists(path))
                {
                    continue;
                }

                Directory.CreateDirectory(PathHelper.GetDirectoryName(path, $"Error: Could not get directory name while unpacking for: {path}"));
                File.WriteAllBytes(path, file.Bytes);
            }
        }

        public static BND3 PackFilesIntoBinder3(string dir, Span<string> extensions, bool recursiveSearch = false)
        {
            var binder = new BND3();
            var searchOption = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            int nameIndex = dir.Length + 1;
            foreach (var file in Directory.EnumerateFiles(dir, "*", searchOption))
            {
                if (StringHelper.AnyStringMatch(file.EndsWith, extensions))
                {
                    var bfile = new BinderFile();
                    bfile.Name = file[nameIndex..];
                    bfile.Bytes = File.ReadAllBytes(file);
                    binder.Files.Add(bfile);
                }
            }
            return binder;
        }

        public static BND3 PackFilesIntoBinder3(string dir, string extension, bool recursiveSearch = false)
        {
            var binder = new BND3();
            var searchOption = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            int nameIndex = dir.Length + 1;
            foreach (var file in Directory.EnumerateFiles(dir, "*", searchOption))
            {
                if (file.EndsWith(extension))
                {
                    var bfile = new BinderFile();
                    bfile.Name = file[nameIndex..];
                    bfile.Bytes = File.ReadAllBytes(file);
                    binder.Files.Add(bfile);
                }
            }
            return binder;
        }

        public static BND3 PackFilesIntoBinder3(string dir, string extension, string excludeExtension, bool recursiveSearch = false)
        {
            var binder = new BND3();
            var searchOption = recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            int nameIndex = dir.Length + 1;
            foreach (var file in Directory.EnumerateFiles(dir, "*", searchOption))
            {
                if (!file.EndsWith(excludeExtension) && file.EndsWith(extension))
                {
                    var bfile = new BinderFile();
                    bfile.Name = file[nameIndex..];
                    bfile.Bytes = File.ReadAllBytes(file);
                    binder.Files.Add(bfile);
                }
            }
            return binder;
        }
    }
}
