using Classes;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Socket ClientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        private const int PORT = 100;
        // Establish the remote endpoint for the socket.  
        // This example uses port 11000 on the local computer.  
        private static IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        private static IPAddress ipAddress = ipHostInfo.AddressList[0];
        private static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

        public MainWindow()
        {
            InitializeComponent();
            StartClient();
            ClientSocket.BeginReceive(new byte[] { 0 }, 0, 0, 0, callbackReceive, null);
        }

        public void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                ClientSocket.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}", ClientSocket.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Classes.Transmission t = new Classes.Transmission(nickname.Text, message.Text, DateTime.Now);
            SendTransmission(t);
        }

        private void SendTransmission(Classes.Transmission t)
        {
            string serialized = JsonConvert.SerializeObject(t);
            byte[] buffer = Encoding.ASCII.GetBytes(serialized);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Release the socket.  
            byte[] msg = Encoding.ASCII.GetBytes("<EXIT>");
            int bytesSent = ClientSocket.Send(msg);
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Classes.Transmission deserialized = JsonConvert.DeserializeObject<Classes.Transmission>(text);
            // Call the main thread to update UI
            this.Dispatcher.Invoke(() =>
            {
                chat.AppendText(deserialized.nickname + ": " + deserialized.message + "\n");
                chat.ScrollToEnd();
            });
            // Rearm
            ClientSocket.BeginReceive(new byte[] { 0 }, 0, 0, 0, callbackReceive, null);
        }

        void callbackReceive(IAsyncResult ar)
        {
            ReceiveResponse();
        }
    }
}
