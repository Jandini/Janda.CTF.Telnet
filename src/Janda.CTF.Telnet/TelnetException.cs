using System;

namespace Janda.CTF
{
    public class TelnetException : Exception
    {
        public TelnetException(string message)
            : base(message)
        {

        }
    }
}
