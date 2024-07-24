using System.Runtime.CompilerServices;

namespace ACVLooseLoader
{
    /// <summary>
    /// The configuration for the loose loader.
    /// </summary>
    public class LooseLoaderConfig
    {
        /// <summary>
        /// Whether or not to log to a log file.
        /// </summary>
        public bool LogToFile { get; set; } = true;

        /// <summary>
        /// Whether or not to pause when finished.
        /// </summary>
        public bool PauseOnFinish { get; set; } = true;

        /// <summary>
        /// Whether or not to skip already existing files while unpacking.
        /// </summary>
        public bool SkipExistingFiles { get; set; } = false;

        /// <summary>
        /// Whether or not to skip unknown files while unpacking.
        /// </summary>
        public bool SkipUnknownFiles { get; set; } = false;

        /// <summary>
        /// Whether or not to skip unpacking main archives.
        /// </summary>
        public bool SkipMainArchiveUnpack { get; set; } = false;

        /// <summary>
        /// Whether or not to skip unpacking main archives previously hidden by this tool.
        /// </summary>
        public bool SkipHiddenMainArchiveUnpack { get; set; } = false;

        /// <summary>
        /// Whether or not to skip unpacking maps.
        /// </summary>
        public bool SkipMapUnpack { get; set; } = false;

        /// <summary>
        /// Whether or not to skip unpacking boot binders.
        /// </summary>
        public bool SkipBootBinderUnpack { get; set; } = false;

        /// <summary>
        /// Whether or not to skip unpacking scripts.
        /// </summary>
        public bool SkipScriptUnpack { get; set; } = false;

        /// <summary>
        /// Whether or not to skip packing map resources for Armored Core V.
        /// </summary>
        public bool SkipMapResourcePack { get; set; } = false;

        /// <summary>
        /// Hide main archive headers for loose load.
        /// </summary>
        public bool HideHeaders { get; set; } = true;

        /// <summary>
        /// Applies a fix for FMOD crashes on PS3.
        /// </summary>
        public bool ApplyFmodCrashFix { get; set; } = true;

        /// <summary>
        /// The path provided will be treated as the root folder and the default platform will be used.
        /// </summary>
        public bool UseManualPath { get; set; } = false;

        /// <summary>
        /// Whether or not to use the default platform.
        /// </summary>
        public bool UseDefaultPlatform { get; set; } = false;

        /// <summary>
        /// Whether or not to use the default game.
        /// </summary>
        public bool UseDefaultGame { get; set; } = false;

        /// <summary>
        /// The default platform to use.
        /// </summary>
        public PlatformType DefaultPlatform { get; set; } = PlatformType.PS3;

        /// <summary>
        /// The default game to use.
        /// </summary>
        public GameType DefaultGame { get; set; } = GameType.ArmoredCoreV;

        /// <summary>
        /// Parse a config from the given path.
        /// </summary>
        /// <param name="path">A file path to the config to parse.</param>
        public void Parse(string path)
        {
            ConfigParser parser = new ConfigParser();
            parser.Parse(path);

            LogToFile = SearchBoolProperty(parser, LogToFile);
            PauseOnFinish = SearchBoolProperty(parser, PauseOnFinish);
            SkipExistingFiles = SearchBoolProperty(parser, SkipExistingFiles);
            SkipUnknownFiles = SearchBoolProperty(parser, SkipUnknownFiles);
            SkipMainArchiveUnpack = SearchBoolProperty(parser, SkipMainArchiveUnpack);
            SkipHiddenMainArchiveUnpack = SearchBoolProperty(parser, SkipHiddenMainArchiveUnpack);
            SkipScriptUnpack = SearchBoolProperty(parser, SkipScriptUnpack);
            SkipMapUnpack = SearchBoolProperty(parser, SkipMapUnpack);
            SkipBootBinderUnpack = SearchBoolProperty(parser, SkipBootBinderUnpack);
            SkipMainArchiveUnpack = SearchBoolProperty(parser, SkipMainArchiveUnpack);
            SkipMapResourcePack = SearchBoolProperty(parser, SkipMapResourcePack);
            HideHeaders = SearchBoolProperty(parser, HideHeaders);
            ApplyFmodCrashFix = SearchBoolProperty(parser, ApplyFmodCrashFix);
            UseManualPath = SearchBoolProperty(parser, UseManualPath);
            UseDefaultPlatform = SearchBoolProperty(parser, UseDefaultPlatform);
            UseDefaultGame = SearchBoolProperty(parser, UseDefaultGame);
            DefaultPlatform = SearchEnumProperty(parser, DefaultPlatform);
            DefaultGame = SearchEnumProperty(parser, DefaultGame);
        }

        /// <summary>
        /// Search for a <see cref="bool"/> property.
        /// </summary>
        /// <param name="parser">The config parser.</param>
        /// <param name="property">The property being set for defaulting and automatic name retrieval.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns>The property value if found, or the property as a default.</returns>
        private static bool SearchBoolProperty(ConfigParser parser, bool property, [CallerArgumentExpression(nameof(property))] string propertyName = "")
        {
            if (parser.ValueDictionary.TryGetValue(propertyName, out string? str))
            {
                if (bool.TryParse(str, out bool result))
                {
                    return result;
                }
            }

            return property;
        }

        /// <summary>
        /// Search for an <see cref="Enum"/> property.
        /// </summary>
        /// <typeparam name="TEnum">The type of <see cref="Enum"/> being searched for.</typeparam>
        /// <param name="parser">The config parser.</param>
        /// <param name="property">The property being set for defaulting and automatic name retrieval.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns>The property value if found, or the property as a default.</returns>
        private static TEnum SearchEnumProperty<TEnum>(ConfigParser parser, TEnum property, [CallerArgumentExpression(nameof(property))] string propertyName = "") where TEnum : Enum
        {
            if (parser.ValueDictionary.TryGetValue(propertyName, out string? str))
            {
                if (Enum.TryParse(typeof(TEnum), str, out object? result))
                {
                    return (TEnum)result;
                }
            }

            return property;
        }

        /// <summary>
        /// Search for a <see cref="string"/> property.
        /// </summary>
        /// <param name="parser">The config parser.</param>
        /// <param name="property">The property being set for defaulting and automatic name retrieval.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns>The property value if found, or the property as a default.</returns>
        private static string SearchStringProperty(ConfigParser parser, string property, [CallerArgumentExpression(nameof(property))] string propertyName = "")
        {
            if (parser.ValueDictionary.TryGetValue(propertyName, out string? str))
            {
                if (str != null)
                {
                    return str;
                }
            }

            return property;
        }
    }
}
