using System.Collections.Generic;
using System.Text;

namespace Janda.CTF
{
    static class ByteArrayExtensions
    {

        private static char[] ToHexCharArray(this byte[] bytes)
        {
            int count = bytes.Length;
            char[] hex = new char[count * 2];
            byte b;

            for (int y = 0, x = 0; y < count; ++y, ++x)
            {
                b = ((byte)(bytes[y] >> 4));

                hex[x] = (char)(b > 9
                    ? b + 0x37
                    : b + 0x30);

                b = ((byte)(bytes[y] & 0xF));

                hex[++x] = (char)(b > 9
                    ? b + 0x37
                    : b + 0x30);
            }

            return hex;
        }

        /// <summary>
        /// Convert byte array to hex dump on the left and ascii characters on the right
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <param name="indentation">Left indentation</param>
        /// <param name="asciiSeparator">Space between hex bytes and ascii characters</param>
        /// <param name="bytesPerLine">Number of bytes per line</param>
        /// <returns>Hex dump string</returns>
        internal static string ToHexDump(this byte[] bytes, string message = "", string indentation = "", string asciiSeparator = "   ", int bytesPerLine = 16, bool cleanAscii = true, char nonPrintableChar = '.', int showStrings = 8, bool stringsOnly = false)
        {
            if (bytes == null)
                return string.Empty;

            var result = new StringBuilder(bytes.Length * 8);

            var hex = bytes.ToHexCharArray();

            result.Append(message);
            result.AppendLine().Append(indentation);

            var words = new List<string>();
            var ascii = new StringBuilder();
            var text = new StringBuilder();

            int lineStart = result.Length;

            for (int i = 0, j = 0; i < hex.Length; i += 2, j++)
            {
                result.Append(hex[i]).Append(hex[i + 1]);

                char c = (char)bytes[j];

                if (!cleanAscii)
                    ascii.Append(!char.IsControl(c) ? c : nonPrintableChar);
                else
                    ascii.Append(bytes[j] <= 0x7F && bytes[j] > 0x20 ? c : nonPrintableChar);

                if (bytes[j] <= 0x7F && bytes[j] > 0x20)
                {
                    text.Append(c);
                }
                else
                {
                    if (showStrings > 0 && text.Length >= showStrings)
                        words.Add(text.ToString());

                    text = new StringBuilder();
                }

                if (i < hex.Length - 2)
                {
                    result.Append(' ');

                    if ((i / 2 + 1) % bytesPerLine == 0)
                    {
                        if (stringsOnly && words.Count == 0)
                            result.Remove(lineStart, result.Length - lineStart);
                        else
                        {
                            result.Append(asciiSeparator).Append(ascii).Append(asciiSeparator).Append(string.Join(' ', words));
                            result.AppendLine().Append(indentation);
                        }
                        
                        ascii = new StringBuilder();
                        words = new List<string>();

                        lineStart = result.Length;
                    }
                }
            }

            if (ascii.Length > 0)
            {
                result.Append(new string(' ', (bytesPerLine - ascii.Length) * 3 + 1));
                result.Append(asciiSeparator).Append(ascii);

                if (showStrings > 0 && text.Length >= showStrings)
                    result
                         .Append(new string(' ', bytesPerLine - ascii.Length))
                         .Append(asciiSeparator).Append(text);
            }

            return result.ToString();
        }
    }
}
