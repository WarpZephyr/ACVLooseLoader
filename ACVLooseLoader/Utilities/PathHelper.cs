namespace ACVLooseLoader
{
    internal static class PathHelper
    {
        internal static string GetDirectoryName(string path, string errorMessage)
        {
            string? directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                throw new UserErrorException(errorMessage);
            }
            return directory;
        }

        internal static string CorrectDirectorySeparatorChar(string path) => path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        internal static string TrimLeadingDirectorySeparators(string path) => path.TrimStart('\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        internal static string TrimTrailingDirectorySeparators(string path) => path.TrimEnd('\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        internal static string TrimDirectorySeparators(string path) => path.Trim('\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        internal static string CleanPath(string path) => CorrectDirectorySeparatorChar(TrimDirectorySeparators(path));
        internal static string[] CleanPaths(string[] paths)
        {
            string[] cleanedPaths = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                cleanedPaths[i] = CleanPath(paths[i]);
            }
            return cleanedPaths;
        }
    }
}
