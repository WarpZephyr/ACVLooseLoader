namespace ACVLooseLoader
{
    public static class FileHelper
    {
        public static void BackupFile(string path)
        {
            string backupPath = path + ".bak";
            if (!File.Exists(backupPath))
            {
                File.Move(path, backupPath);
            }
        }
    }
}
