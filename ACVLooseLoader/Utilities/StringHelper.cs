namespace ACVLooseLoader
{
    public static class StringHelper
    {
        public static bool AnyStringMatch(Func<string, bool> search, Span<string> strs)
        {
            foreach (string str in strs)
            {
                if (search(str))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
