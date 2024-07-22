using System.Runtime.InteropServices;

namespace WindowsUtilities
{
    /// <summary>
    /// Native Windows ConsoleMode function imports.
    /// </summary>
    internal static partial class ConsoleModeWinNative
    {
        public const uint ENABLE_PROCESSED_INPUT = 0x0001;
        public const uint ENABLE_LINE_INPUT = 0x0002;
        public const uint ENABLE_ECHO_INPUT = 0x0004;
        public const uint ENABLE_WINDOW_INPUT = 0x0008;
        public const uint ENABLE_MOUSE_INPUT = 0x0010;
        public const uint ENABLE_INSERT_MODE = 0x0020;
        public const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        public const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        public const uint ENABLE_AUTO_POSITION = 0x0100;
        public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;
        public const uint STD_INPUT_HANDLE = unchecked((uint)-10);
        public const uint STD_OUTPUT_HANDLE = unchecked((uint)-11);
        public const uint STD_ERROR_HANDLE = unchecked((uint)-12);

        /// <summary>
        /// Retrieves the window handle used by the console associated with the calling process.
        /// </summary>
        /// <returns>The return value is a handle to the window used by the console associated with the calling process or <see cref="null"/> if there is no such associated console.</returns>
        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial IntPtr GetConsoleWindow();

        /// <summary>
        /// Retrieves the current input mode of a console's input buffer or the current output mode of a console screen buffer.
        /// </summary>
        /// <param name="hConsoleHandle">A handle to the console input buffer or the console screen buffer. The handle must have the GENERIC_READ access right. For more information, see Console Buffer Security and Access Rights.</param>
        /// <param name="lpMode">A variable that receives the current mode of the specified buffer.</param>
        /// <returns>If the function succeeds, the return value is <see cref="true"/>. Otherwise the return value is <see cref="false"/>.</returns>
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        /// <summary>
        /// Sets the input mode of a console's input buffer or the output mode of a console screen buffer.
        /// </summary>
        /// <param name="hConsoleHandle">A handle to the console input buffer or a console screen buffer. The handle must have the GENERIC_READ access right. For more information, see Console Buffer Security and Access Rights.</param>
        /// <param name="dwMode">The input or output mode to be set.</param>
        /// <returns>If the function succeeds, the return value is <see cref="true"/>. Otherwise the return value is <see cref="false"/>.</returns>
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>
        /// Retrieves a handle to the specified standard device (standard input, standard output, or standard error).
        /// </summary>
        /// <param name="nStdHandle">The standard device. Can be either: STD_INPUT_HANDLE, STD_OUTPUT_HANDLE, or STD_ERROR_HANDLE.</param>
        /// <returns></returns>
        [LibraryImport("Kernel32.dll", SetLastError = true)]
        public static partial IntPtr GetStdHandle(uint nStdHandle);
    }
}
