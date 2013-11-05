using System;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public class Client
    {
        private IPAddress serverIP;
        private int port;

        private IPEndPoint serverEnd;

        private System.Net.Sockets.Socket socket;

        private const string END_STRING = "done.";

        /// <summary>
        /// Default Constructor:
        /// serverIP is loopback (localhost)
        /// port is 1234
        /// </summary>
        public Client():this(IPAddress.Loopback, 1234)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverIP">Server IP Address</param>
        /// <param name="port">Port number</param>
        public Client(IPAddress serverIP, int port)
        {
            debug("Trying to connect to server...\n");

            this.serverIP = serverIP;
            this.port = port;
            this.serverEnd = new IPEndPoint(this.serverIP, this.port);

            try
            {
                this.socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                this.socket.Connect(this.serverEnd);
                debug("Connected to server...\n");
            }
            catch (SocketException e)
            {
                Console.Error.WriteLine("Could not connect... {0} {1}", e.StackTrace, e.Message);
            }
        }

        /// <summary>
        /// Prints str iff DEBUG == true (i.e., if we were build as a DEBUG target)
        /// </summary>
        /// <param name="str">String to print</param>
        private void debug(string str)
        {
			#if DEBUG
                Console.WriteLine(str);
			#endif
        }

        public static long counter = 0;

        /// <summary>
        /// Send str to the server
        /// </summary>
        /// <param name="str">String to send to server</param>
        /// <returns>false on error or done</returns>
        public bool Send(string str)
        {
            debug(counter.ToString());
            counter++;
            return Socket.Socket.Send(socket, str);
        }

        /// <summary>
        /// Receive str from the server
        /// </summary>
        /// <param name="str">String received from server</param>
        /// <returns>false on error or done</returns>
        public bool Receive(out string str)
        {
            return Socket.Socket.Receive(socket, out str);
        }

        public bool ReceiveASCII(out string str)
        {
            debug("receive");
            bool t = Socket.Socket.ReceiveASCII(socket, out str);
            debug("done receiving");
            return t;
        }

        public bool SendASCII(string str)
        {
            debug("sending");
            bool t = Socket.Socket.SendASCII(socket, str);
            debug("done sending");
            return t;
        }

        /// <summary>
        /// Basic chat app
        /// </summary>
        public void startChat()
        {
            string str = "Begin chat:" ;
            do
            {
                Console.WriteLine(str);
                str = Console.ReadLine();
                if (!Send(str))
                    break;
            } while (Receive(out str));
        }

        static void Main(string[] args)
        {
            Client client = new Client(IPAddress.Parse("134.173.43.9"), 1234);
            client.startChat();
        }
    }
}
