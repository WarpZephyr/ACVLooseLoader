namespace ACVLooseLoader
{
    internal static class PathHandler
    {
        internal static string GetDirectoryName(string path, string? errorMessage = null)
        {
            
            string? directory = Path.GetDirectoryName(path) ?? path;
            if (string.IsNullOrEmpty(directory))
            {
                errorMessage ??= $"Provided path did not contain directory information: {path}";
                throw new ArgumentException(errorMessage);
            }

            return directory;
        }

        internal static string CorrectDirectorySeparatorChar(string path) => path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        internal static string TrimLeadingDirectorySeparators(string path) => path.TrimStart('\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        internal static string TrimTrailingDirectorySeparators(string path) => path.TrimEnd('\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        internal static string TrimDirectorySeparators(string path) => path.Trim('\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        internal static string CleanPath(string path) => CorrectDirectorySeparatorChar(TrimLeadingDirectorySeparators(path));
        internal static string[] CleanPaths(string[] paths)
        {
            string[] cleanedPaths = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                cleanedPaths[i] = CleanPath(paths[i]);
            }
            return cleanedPaths;
        }

        internal static void CreateDirectoryOnCondition(string directoryPath, bool condition)
        {
            if (condition)
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        internal static string Combine(string path1, string path2) => Path.Combine(CleanPath(path1), CleanPath(path2));
        internal static string Combine(string path1, string path2, string path3) => Path.Combine(CleanPath(path1), CleanPath(path2), CleanPath(path3));
        internal static string Combine(string path1, string path2, string path3, string path4) => Path.Combine(CleanPath(path1), CleanPath(path2), CleanPath(path3), CleanPath(path4));
        internal static string Combine(params string[] paths) => Path.Combine(CleanPaths(paths));

        internal static string GetWithExtensionDot(string str)
        {
            ArgumentNullException.ThrowIfNull(str, nameof(str));
            if (!str.StartsWith('.'))
            {
                return '.' + str;
            }
            return str;
        }

        internal static string GetWithoutExtensionDot(string str)
        {
            ArgumentNullException.ThrowIfNull(str, nameof(str));
            return str.TrimStart('.');
        }

        internal static string GetExtensions(string path)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            int index = path.IndexOf('.');
            if (index > -1)
            {
                return path[index..];
            }
            return string.Empty;
        }

        internal static string GetWithoutExtensions(string path)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            int index = path.IndexOf('.');
            if (index > -1)
            {
                return path[..index];
            }
            return path;
        }

        internal static string GetFileNameWithoutExtensions(string path) => GetWithoutExtensions(Path.GetFileName(path));

        internal static string GetRelativePath(string path, string relativeFrom)
        {
            if (string.IsNullOrEmpty(relativeFrom))
            {
                return path;
            }

            StringExceptionHandler.ThrowIfNotStartsWith(path, relativeFrom, nameof(path), nameof(relativeFrom));
            return path[relativeFrom.Length..];
        }

        internal static string GetDirectoryNameWithoutPath(string path) => Path.GetFileName(TrimTrailingDirectorySeparators(path));

        internal static string GetRelativeDirectory(IList<string> paths)
        {
            // If there are no paths return immediately.
            if (paths == null || paths.Count < 1)
            {
                return string.Empty;
            }

            // Get the directory of the first path and return immediately if null or empty.
            string? relativeDirectory = Path.GetDirectoryName(paths[0]);
            if (string.IsNullOrEmpty(relativeDirectory))
            {
                return string.Empty;
            }

            // Process each path.
            foreach (string path in paths)
            {
                // Return if the path is null, empty, or nothing.
                if (string.IsNullOrWhiteSpace(path))
                {
                    return string.Empty;
                }

                // Get the directory of this path and return if it is null or empty.
                string? pathDirectory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(pathDirectory))
                {
                    return string.Empty;
                }

                // If the current highest relative directory is not equal to the current one, but it starts with the current one, the current one is higher.
                if (!relativeDirectory.Equals(pathDirectory) && relativeDirectory.StartsWith(pathDirectory))
                {
                    relativeDirectory = pathDirectory;
                }
            }

            // Return the result.
            return relativeDirectory;
        }
    }
}
