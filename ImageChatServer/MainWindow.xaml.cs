using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using MessageLibrary;
using System.Drawing;
using System.IO;

namespace ImageChatServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TcpListener server;
        TcpClient newConnection;
        List<ConnectedClient> users;
        ConnectedClient user;
        List<Message> messageQueue;
        async void RunServer()
        {
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9001);
            users = new List<ConnectedClient>();
            messageQueue = new List<Message>();
            server.Start();
            while (true) {
                await Task.Delay(1);
                status.Content = "Waiting";
                //Check for new connections.
                if (server.Pending())
                {
                    AddNewUser();
                }
                //Check for new messages.
                if (messageQueue.Count>0) {
                    Message m = messageQueue[messageQueue.Count - 1];
                    messageQueue.Remove(m);
                    Dispatcher.Invoke(DispatcherPriority.Background,
                              new Action(() => log.Text += m.sendingUser + m.textContent + "\n"));
                    if (m.imageContent!=null) {
                        Bitmap b = m.imageContent;
                        displayImage.Source = ConvertFromBitmap(b);
                    }

                    if (m.isDisconnectMessage) {
                        users.RemoveAll(u=>u.isAlive==false);
                    }
                    if (m.isKeepAliveMessage) {
                        continue;
                    }
                    BroadCastToUsers(m);
                }
            }
        }

        public ImageSource ConvertFromBitmap(Bitmap b) {
            MemoryStream ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }
        public void AddNewUser() {
            status.Content = "Pending Connection";
            user = new ConnectedClient(server, messageQueue);
            users.Add(user);
            Dispatcher.Invoke(DispatcherPriority.Background,
                  new Action(() => log.Text += user.username + " Connected\n"));
        }

        public void BroadCastToUsers(Message m) {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].username != m.sendingUser)
                {
                    Message.SendMessage(users[i].connection, m);
                }
            }
        }

        public void IncrementClicks(Object sender, RoutedEventArgs e)
        {
            buttonClicks.Content += "!";
        }

        public MainWindow()
        {
            InitializeComponent();
            RunServer();
        }
    }
}
