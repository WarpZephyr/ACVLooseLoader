namespace ACVLooseLoader
{
    /// <summary>
    /// A parser for simple configuration files.
    /// </summary>
    public class ConfigParser
    {
        /// <summary>
        /// A dictionary containing read property to value pairs.
        /// </summary>
        public Dictionary<string, string> ValueDictionary = [];

        /// <summary>
        /// Parse a config from the given path.
        /// </summary>
        /// <param name="path">A file path to the config to parse.</param>
        public void Parse(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string parsedLine = ParseLine(line);
                if (!string.IsNullOrEmpty(parsedLine))
                {
                    ParseValue(parsedLine);
                }
            }
        }

        /// <summary>
        /// Parse a single line in a config and clean it of unnecessary things.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>The parsed line.</returns>
        private static string ParseLine(string line)
        {
            string parsedLine = line;
            int commentIndex = line.IndexOf("//");
            if (commentIndex == 0)
            {
                return string.Empty;
            }
            else if (commentIndex > 0)
            {
                parsedLine = parsedLine[..commentIndex];
            }

            return parsedLine.Trim();
        }

        /// <summary>
        /// Parse a property and value pair from a single line for a config.
        /// </summary>
        /// <param name="parsedLine">The line to parse the property and value of.</param>
        private void ParseValue(string parsedLine)
        {
            string[] strs = parsedLine.Split('=', StringSplitOptions.TrimEntries);

            if (strs.Length > 1)
            {
                ValueDictionary.Add(strs[0], strs[1]);
            }
        }
    }
}
