﻿// Found at:
// https://stackoverflow.com/a/31193455
// https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54

using System.Text;
using System.Threading.Channels;

namespace ACVLooseLoader
{
    /// <summary>
    /// An ASCII progress bar
    /// </summary>
    public class ConsoleProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 10;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposedValue = false;
        private int animationIndex = 0;

        public ConsoleProgressBar()
        {
            timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object? state)
        {
            lock (timer)
            {
                if (disposedValue) return;

                UpdateTextByProgress();

                ResetTimer();
            }
        }

        private void UpdateTextByProgress()
        {
            if (disposedValue) return;

            int progressBlockCount = (int)(currentProgress * blockCount);
            int percent = (int)(currentProgress * 100);
            string text = string.Format("[{0}{1}] {2,3}% {3}",
                new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                percent,
                animation[animationIndex++ % animation.Length]);
            UpdateText(text);
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.AsSpan(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UpdateTextByProgress();

                    lock (timer)
                    {
                        timer.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
