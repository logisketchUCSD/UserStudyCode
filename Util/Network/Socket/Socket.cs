namespace Socket
{
    /// <summary>
    /// Allows Send Receive methods
    /// </summary>
    public class Socket
    {
        public const string END_STRING = "done.";

        /// <summary>
        /// Receive str
        /// </summary>
        /// <param name="str">String received</param>
        /// <returns>false on error or done</returns>
        public static bool Receive(System.Net.Sockets.Socket socket, out string str)
        {
            try
            {
                byte[] bufferSize = new byte[4];
                int receivedData = socket.Receive(bufferSize);

                byte[] buffer = new byte[System.BitConverter.ToInt32(bufferSize, 0)];
                receivedData = socket.Receive(buffer);


                str = System.Text.Encoding.ASCII.GetString(buffer);
                return !str.Equals(Socket.END_STRING);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                System.Console.Error.WriteLine("Error: " + e.Message + "\n\n" + e.StackTrace + "\n\n");
                str = "";
                return false;
            }
        }

        public static bool ReceiveASCII(System.Net.Sockets.Socket socket, out string res)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int receivedData = socket.Receive(buffer);
                
                string str = System.Text.Encoding.ASCII.GetString(buffer);

                int i = 0;
                char c;
                while (true)
                {
                    c = str[i];
                    if (c.Equals('\n') || c.Equals('\0'))
                        break;
                    ++i;
                }

                int sizeOfMessage = System.Convert.ToInt32(str.Substring(0, i));
                int remaining = sizeOfMessage + i + 1 - receivedData;

                res = str.Substring(i + 1, receivedData - i - 1);

                while (remaining > 0)
                {
                    receivedData = socket.Receive(buffer);
                    str = System.Text.Encoding.ASCII.GetString(buffer);
                    remaining -= receivedData;
                    res += str.Substring(0, receivedData);
                }
                res = res.TrimEnd(new char[] { '\0' });

                return !res.Equals(Socket.END_STRING);
                /*

                //trim initial size off the top (I think one ReceiveASCII will always get it all... it should)
                //that is, value(first line) + 4 = receivedData
                str = str.TrimStart(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\n' });
                
                //trim end
                str = str.TrimEnd(new char[] { '\0' });
                if (str.Length == 0)
                    System.Console.WriteLine("uh oh2");
                
                return !str.Equals(Socket.END_STRING);
                */
            }
            catch (System.Net.Sockets.SocketException e)
            {
                System.Console.Error.WriteLine("Error: " + e.Message + "\n\n" + e.StackTrace + "\n\n");
                res = "";
                return false;
            }
        }

        public static bool SendASCII(System.Net.Sockets.Socket socket, string str)
        {
            try
            {
                //System.Console.WriteLine("sending: {0}", str.Length);

                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(str);
                int sent = socket.Send(buffer);

                //System.Console.WriteLine("sent: {0}", sent);
                //System.Console.WriteLine("first: {0}", str.Substring(0, 5));

                return !str.Equals(Socket.END_STRING);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                System.Console.Error.WriteLine("Error: " + e.Message + "\n\n" + e.StackTrace + "\n\n");
                return false;
            }
        }


        /// <summary>
        /// Send str 
        /// </summary>
        /// <param name="str">String to send</param>
        /// <returns>false on error or done</returns>
        public static bool Send(System.Net.Sockets.Socket socket, string str)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(str);
                byte[] bufferSize = System.BitConverter.GetBytes(buffer.Length);

                socket.Send(bufferSize);
                socket.Send(buffer);

                return !str.Equals(Socket.END_STRING);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                System.Console.Error.WriteLine("Error: " + e.Message + "\n\n" + e.StackTrace + "\n\n");
                return false;
            }
        }
    }
}
