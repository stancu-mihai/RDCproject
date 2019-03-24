using Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class SynchronousSocketListener
    {
        private static readonly List<Socket> broadcastList = new List<Socket>();
        private static readonly Queue<Transmission> logQueue = new Queue<Transmission>();

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
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                // Start the Logger thread
                new Thread(() =>
                {
                    LoggerThreadTasks();
                }).Start();

                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a new connection on thread " + Thread.CurrentThread.ManagedThreadId);
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    // When the connection is accepted, add it to broadcast list
                    broadcastList.Add(handler);
                    new Thread(() =>
                    {
                        ThreadTasks(handler, bytes);
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

        public static void LoggerThreadTasks()
        {
            try
            {
                do
                {
                    // Prepare to create or append to log file
                    FileInfo file = new FileInfo("c:\\Logs\\");
                    file.Directory.Create(); // If the directory already exists, this method does nothing.
                    FileStream fs = new FileStream("c:\\Logs\\chatlog.txt", FileMode.Append, FileAccess.Write);
                    using (StreamWriter logStream = new StreamWriter(fs))
                        // If the queue is not empty
                        if (logQueue.Count != 0)
                        {
                            // Simulate some long request
                            Thread.Sleep(3000);
                            Transmission t = logQueue.Dequeue();
                            // Write to log
                            logStream.WriteLine("Received message from '{0}' on thread '{1}' with the message '{2}' at {3}",
                            t.nickname, Thread.CurrentThread.ManagedThreadId, t.message, t.time);
                        }
                        else
                        {
                            // Don't use too much CPU
                            Thread.Sleep(1000);
                        }
                } while (true);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public static void ThreadTasks(Socket socket, byte[] bytes)
        {
            try
            {
                Console.WriteLine("New connection accepted on thread " + Thread.CurrentThread.ManagedThreadId);
                do
                {
                    int bytesRec = socket.Receive(bytes);
                    string serializedData = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (serializedData.IndexOf("<EXIT>") > -1)
                    {
                        Console.WriteLine("Client on thread {0} disconnected", Thread.CurrentThread.ManagedThreadId);
                        broadcastList.Remove(socket);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        break;
                    }

                    // Show the data on the console.  
                    Transmission t = JsonConvert.DeserializeObject<Transmission>(serializedData);
                    Console.WriteLine("Received message from '{0}' on thread '{1}' with the message '{2}' at {3}",
                        t.nickname, Thread.CurrentThread.ManagedThreadId, t.message, t.time);

                    // Broadcast the message to all clients
                    byte[] msg = Encoding.ASCII.GetBytes(serializedData);
                    foreach (Socket socketToBroadcast in broadcastList)
                    {
                        socketToBroadcast.Send(msg);
                    }

                    // Add the transmission to log queue
                    logQueue.Enqueue(t);
                } while (true);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}
