using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class SynchronousSocketListener
    {

        public static void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a new connection on thread " + Thread.CurrentThread.ManagedThreadId);
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    new Thread(() =>
                    {
                        try
                        {
                            Console.WriteLine("New connection accepted on thread " + Thread.CurrentThread.ManagedThreadId);
                            do
                            {
                                int bytesRec = handler.Receive(bytes);
                                string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                                // Show the data on the console.  
                                Console.WriteLine("Text received on thread {0}: {1}", Thread.CurrentThread.ManagedThreadId, data);

                                // Echo the data back to the client.  
                                byte[] msg = Encoding.ASCII.GetBytes(data);

                                handler.Send(msg);

                                if (data.IndexOf("<EXIT>") > -1)
                                {
                                    break;
                                }

                            } while (true);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex.Message);
                        }
                    }).Start();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}
