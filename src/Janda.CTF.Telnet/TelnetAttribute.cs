using System;

namespace Janda.CTF
{
    public class TelnetAttribute : Attribute
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
