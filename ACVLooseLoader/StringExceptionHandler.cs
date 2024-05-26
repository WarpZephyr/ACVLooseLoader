using System.Runtime.CompilerServices;

namespace ACVLooseLoader
{
    internal static class StringExceptionHandler
    {
        internal static void ThrowIfNotStartsWith(string? value, string? start, [CallerArgumentExpression(nameof(value))] string? valueParamName = null, [CallerArgumentExpression(nameof(start))] string? startParamName = null)
        {
            if (value == null && start != null)
            {
                throw new ArgumentNullException(nameof(valueParamName));
            }

            if (value != null && start == null)
            {
                throw new ArgumentNullException(nameof(startParamName));
            }

            if (value != null && start != null && !value.StartsWith(start))
            {
                throw new ArgumentException($"{valueParamName ?? "String"} does not begin with {startParamName ?? "start"} string.", nameof(valueParamName));
            }
        }
    }
}
