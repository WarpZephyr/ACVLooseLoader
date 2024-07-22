namespace ACVLooseLoader
{
    public class TextChannel
    {
        public TextChannelWriter Writer { get; set; }
        public bool Muted { get; set; }

        public TextChannel(TextChannelWriter writer, bool muted = false)
        {
            Writer = writer;
            Muted = muted;
        }

        public void Write(string value)
        {
            if (!Muted)
            {
                Writer.Write(value);
            }
        }

        public void WriteLine(string value)
        {
            if (!Muted)
            {
                Writer.WriteLine(value);
            }
        }
    }
}
