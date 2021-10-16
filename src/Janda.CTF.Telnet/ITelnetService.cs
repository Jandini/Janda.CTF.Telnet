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

        bool IsConnected { get; }
        string Read();
        string[] ReadLines();
        void WriteLine(string value);
        void Write(string value);
        void WriteBytes(byte[] bytes);
    }
}
