namespace Janda.CTF
{
    public interface ITelnetService
    {        
        void Connect(TelnetAddress address);
        void Connect(string host, int port);        
        /// <summary>
        /// Connect using host and port given in TelnetAttribute 
        /// </summary>
        void Connect();
        void Disconnect();
        string Read();
        string[] ReadLines();
        void WriteLine(string command);
        void Write(string command);
        public void WriteBytes(byte[] bytes);
    }
}
