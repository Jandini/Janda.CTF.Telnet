using System.Net.Sockets;

namespace Janda.CTF
{
    public interface ITelnetService
    {
        ITelnetService Connect(TcpClient client);

        /// <summary>
        /// Connect to the specified port on the specified host given in telnet address object.
        /// If the connection is already established then it is automatically closed before making a new one.
        /// </summary>
        /// <param name="address">Telnet address object</param>
        ITelnetService Connect(TelnetAddress address);

        /// <summary>
        /// Connect to the specified port on the specified host given in parameters.
        /// If the connection is already established then it is automatically closed before making a new one.
        /// </summary>
        /// <param name="host">The DNS name of the remote host to which you intend to connect.</param>
        /// <param name="port">The port number of the remote host to which you intend to connect.</param>
        ITelnetService Connect(string host, int port);

        /// <summary>
        /// Connect to the specified port on the specified host given in telnet attribute.
        /// The caller method must be decorated with TelnetAttribute.
        /// If the connection is already established then it is automatically closed before making a new one.
        /// </summary>
        ITelnetService Connect();


        void Disconnect();

        bool IsConnected { get; }
        string Read();
        string[] ReadLines();
        void WriteLine(string value);
        void Write(string value);
        void WriteBytes(byte[] bytes);



        /// <summary>
        /// Write string value line and read response
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Response text</returns>
        string Send(string value);

        /// <summary>
        /// Write object line and read the response
        /// </summary>
        /// <param name="command">Object that supports ToString()</param>
        /// <returns>Response text</returns>
        string Send(object value);     

        /// <summary>
        /// Write anything that can do .ToString()
        /// </summary>
        /// <param name="value"></param>
        void WriteLine(object value);

        /// <summary>
        /// Set internal delay time. Default value is 100ms. 
        /// </summary>
        /// <param name="milliseconds"></param>
        void SetDelay(int milliseconds);

        /// <summary>
        /// Set end of line marker. Default is \n
        /// </summary>
        /// <param name="eol"></param>
        void SetEol(string eol);

    }
}
