using System.Diagnostics;

namespace ACVLooseLoader
{
    public class Logger : IDisposable
    {
        private readonly TextChannelWriter _channelWriter;
        private readonly TextChannel _debugChannel;
        private readonly TextChannel _infoChannel;
        private readonly TextChannel _warnChannel;
        private readonly TextChannel _errorChannel;
        private bool disposedValue;

        public Logger()
        {
            _channelWriter = new TextChannelWriter();
            _debugChannel = _channelWriter.CreateChannel();
            _infoChannel = _channelWriter.CreateChannel();
            _warnChannel = _channelWriter.CreateChannel();
            _errorChannel = _channelWriter.CreateChannel();
        }

        public void RegisterConsoleWriter()
        {
            if (!_channelWriter.HasWriter("Console"))
            {
                _channelWriter.RegisterWriter("Console", Console.Out);
            }
        }

        public void RegisterFileWriter(string path)
        {
            if (!_channelWriter.HasWriter(path))
            {
                var sw = new StreamWriter(path, true);
                sw.AutoFlush = true;
                sw.WriteLine($"[File Logger started {DateTime.Now:MM/dd/yyyy-hh:mm:ss}]");
                _channelWriter.RegisterWriter(path, sw);
            }
        }

        #region Channel Writes

        [Conditional("DEBUG")]
        public void WriteDebug(string value)
        {
            _debugChannel.Write(value);
        }

        [Conditional("DEBUG")]
        public void WriteDebugLine(string value)
        {
            _debugChannel.WriteLine(value);
        }

        public void WriteInfo(string value)
        {
            _infoChannel.Write(value);
        }

        public void WriteInfoLine(string value)
        {
            _infoChannel.WriteLine(value);
        }

        public void WriteWarn(string value)
        {
            _warnChannel.Write(value);
        }

        public void WriteWarnLine(string value)
        {
            _warnChannel.WriteLine(value);
        }

        public void WriteError(string value)
        {
            _errorChannel.Write(value);
        }

        public void WriteErrorLine(string value)
        {
            _errorChannel.WriteLine(value);
        }

        #endregion

        #region Global Writes

        public void Write(string value)
        {
            _channelWriter.Write(value);
        }

        public void WriteLine(string value)
        {
            _channelWriter.WriteLine(value);
        }

        public void WriteTo(string channelName, string value)
        {
            _channelWriter.WriteTo(channelName, value);
        }

        public void WriteLineTo(string channelName, string value)
        {
            _channelWriter.WriteLineTo(channelName, value);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _channelWriter.Dispose();
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
