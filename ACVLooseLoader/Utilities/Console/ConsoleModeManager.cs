using WindowsUtilities;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static WindowsUtilities.ConsoleModeWin;

namespace ACVLooseLoader
{
    /// <summary>
    /// Manages the console mode for windows.
    /// </summary>
    public class ConsoleModeManager
    {
        /// <summary>
        /// The previous mode of the console prior to locking.
        /// </summary>
        private ConsoleModeWinFlags PreviousMode;

        /// <summary>
        /// Whether or not the previous mode has been retrieved yet.
        /// </summary>
        private bool PreviousModeRetrieved;

        /// <summary>
        /// Create a new <see cref="ConsoleModeManager"/>.
        /// </summary>
        public ConsoleModeManager()
        {
            PreviousMode = default;
            PreviousModeRetrieved = false;
        }

        // Prevents users from freezing the program when clicking on the console in windows.
        /// <summary>
        /// Lock several things on the console and save the previous mode.
        /// </summary>
        public void LockConsole()
        {
            PreviousModeRetrieved = GetConsoleMode(out PreviousMode);
            if (PreviousModeRetrieved)
            {
                SetConsoleMode(PreviousMode & ~(ConsoleModeWinFlags.EnableQuickEditMode | ConsoleModeWinFlags.EnableExtendedFlags));
            }
        }

        // Restores the previous state for users calling the program from a terminal.
        /// <summary>
        /// Unlock quick edit on the console and restore the previous mode if successfully locked prior.
        /// </summary>
        public void UnlockConsole()
        {
            if (PreviousModeRetrieved)
            {
                // Invert to false if the console mode actually got set
                PreviousModeRetrieved = !SetConsoleMode(PreviousMode);
            }
        }
    }
}
