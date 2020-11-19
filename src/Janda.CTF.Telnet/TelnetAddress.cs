namespace Janda.CTF
{
    public class TelnetAddress
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public TelnetAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}
