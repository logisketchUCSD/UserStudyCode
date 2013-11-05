using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {

        private IPAddress serverIP, clientIP;
        private IPEndPoint serverEndPoint, clientEndPoint;
        private System.Net.Sockets.Socket serverSocket, clientSocket;
        private int port;

        private const bool DEBUG = true;

        private const string END_STRING = "done.";

        /// <summary>
        /// 
        /// </summary>
        public Server() : this(IPAddress.Loopback, IPAddress.Loopback, 1234)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="clientIP"></param>
        /// <param name="port"></param>
        public Server(IPAddress serverIP, IPAddress clientIP, int port)
        {
            debug("Starting server...\n");

            this.serverIP = serverIP;
            this.clientIP = clientIP;
            this.port = port;

            this.serverEndPoint = new IPEndPoint(this.serverIP, this.port);
            this.clientEndPoint = new IPEndPoint(this.clientIP, this.port);

            this.serverSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            this.serverSocket.Bind(serverEndPoint);
            this.serverSocket.Listen(0);

            debug("Waiting for client...\n");

            this.clientSocket = serverSocket.Accept();
            
            debug("Client connected...\n");

            //add();
            //echo();
        }

        /// <summary>
        /// Basic chat app
        /// </summary>
        public void startChat()
        {
            string str;
            while (Receive(out str))
            {
                Console.WriteLine(str);
                str = Console.ReadLine();
                if (!Send(str))
                    break;
            }
        }

        /// <summary>
        /// Basic echo app
        /// </summary>
        private void echo()
        {
            string str;
            while (Receive(out str))
            {
                debug("Sending " + str + " back...\n");
                Send(str);
            }
        }

        /// <summary>
        /// Basic add app
        /// </summary>
        private void add()
        {
            string str;
            while (Receive(out str))
            {
                debug("Adding " + str);
                string[] p = str.Split();
                int len = p.Length;
                int i;
                ulong total = 0;
                for (i = 0; i < len; ++i)
                {
                    if(p[i].Length > 0)
                        total += Convert.ToUInt64(p[i]);
                }
                debug("Sending " + total.ToString());
                Send(total.ToString());
            }
        }

        private bool Send(string str)
        {
            return Socket.Socket.Send(clientSocket, str);
        }

        private bool Receive(out string str)
        {
            return Socket.Socket.Receive(clientSocket, out str);
        }

        private void debug(string str)
        {
            if (DEBUG)
                Console.WriteLine(str);
        }

        static void Main(string[] args)
        {
            Server server = new Server(IPAddress.Any, IPAddress.Any, 1234);
            server.startChat();
        }
    }
}
