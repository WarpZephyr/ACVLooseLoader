namespace ACVLooseLoader
{
    public class TextChannelWriter : IDisposable
    {
        private readonly Dictionary<string, TextWriter> _writers = [];
        private Action<string>? OnWrite;
        private Action<string>? OnWriteLine;
        private bool disposedValue;

        public TextChannel CreateChannel(bool muted = false)
        {
            return new TextChannel(this, muted);
        }

        public bool RegisterWriter(string name, TextWriter writer)
        {
            bool result = false;
            if (_writers.TryAdd(name, writer))
            {
                result = true;
                OnWrite += writer.Write;
                OnWriteLine += writer.WriteLine;
            }
            return result;
        }

        public bool UnregisterWriter(string name)
        {
            bool result = false;
            if (_writers.TryGetValue(name, out TextWriter? writer))
            {
                result = true;
                OnWrite -= writer.Write;
                OnWriteLine -= writer.WriteLine;
                _writers.Remove(name);
            }
            return result;
        }

        public bool HasWriter(string name)
            => _writers.ContainsKey(name);

        public void Write(string value)
        {
            OnWrite?.Invoke(value);
        }

        public void WriteLine(string value)
        {
            OnWriteLine?.Invoke(value);
        }

        public void WriteTo(string writerName, string value)
        {
            if (_writers.TryGetValue(writerName, out TextWriter? wrtier))
            {
                wrtier.Write(value);
            }
        }

        public void WriteLineTo(string writerName, string value)
        {
            if (_writers.TryGetValue(writerName, out TextWriter? writer))
            {
                writer.WriteLine(value);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnWrite = null;
                    OnWriteLine = null;
                    _writers.Clear();
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
