namespace Janda.CTF
{
    static class StringExtensions
    {
        internal static string ToPlural(this string value, int count)
        {
            if (count > 1 || count == 0)
                return value + "s";

            return value;
        }
    }
}
