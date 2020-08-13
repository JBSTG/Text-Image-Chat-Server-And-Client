using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Windows.Threading;
using System.Drawing;
using MessageLibrary;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ImageChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient client = null;
        string username = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        void ListenForMessages() {
            if (client!=null&&client.Connected) {
                if (client.GetStream().DataAvailable) {
                    Message m = Message.ReceiveMessage(client);
                    if (m.isKeepAliveMessage) {
                        Message ka = new Message();
                        ka.isKeepAliveMessage = true;
                        ka.sendingUser = signInUserName.Text;
                        Message.SendMessage(client,ka);
                    }
                    Dispatcher.Invoke(DispatcherPriority.Background,
                          new Action(() => chatMessageLog.Text += m.sendingUser + ": " + m.textContent + "\n"));
                }
            }
        }

        public async void ConnectToServer(Object sender, RoutedEventArgs evt) {
            try
            {
                client = new TcpClient("127.0.0.1", 9001);
            }
            catch (Exception e)
            {
                chatMessageEntry.Text = e.Message;
            }
            if (client != null && client.Connected)
            {
                username = signInUserName.Text;
                Message m = new Message();
                m.textContent = username;

                Bitmap bmp = new Bitmap(displayImage.Source.ToString().Substring(8));
                m.imageContent = bmp;
                MessageBox.Show("!");
                m.isConnectingMessage = true;
                m.sendingUser = username;
                Message.SendMessage(client,m);
                chatInterface.Visibility = Visibility.Visible;
                signInInterface.Visibility = Visibility.Collapsed;
                chatNameDisplay.Text = m.textContent;

                while (client.Connected) {
                    await Task.Delay(1);
                    ListenForMessages();
                }
            }
        }

        public void SendMessage(Object sender, RoutedEventArgs evt) {
            if (client.Connected) {
                Message m = new Message();
                m.isUserMessage = true;
                m.textContent = chatMessageEntry.Text;

                m.sendingUser = signInUserName.Text;
                NetworkStream ns = client.GetStream();
                IFormatter ifo = new BinaryFormatter();
                ifo.Serialize(ns, m);
                chatMessageEntry.Text = "";
            }
        }

        public void Disconnect(Object sender,CancelEventArgs e) {
            Message dc = Message.DisconnectMessage(username);
            Message.SendMessage(client,dc);
            client.Close();
        }

    }
}
