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
using Microsoft.Win32;

namespace ImageChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient client = null;
        string username = "";
        bool setupComplete = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        async void ListenForMessages() {
            while (client.Connected) {
                await Task.Delay(1);
                if (client.Connected)
                {
                    if (client.GetStream().DataAvailable)
                    {
                        Message m = await Task.Run(()=>Message.ReceiveMessage(client));
                        
                        if (m.isConnectingMessage)
                        {
                            Message response = new Message();
                            response.sendingUser = username;
                            response.isConnectingMessage = true;
                            await Message.SendMessage(client, response);
                        }

                        if (m.isSetupCompleteMessage)
                        {
                            setupComplete = true;
                            chatNameDisplay.Text = username;
                        }

                        if (m.isKeepAliveMessage)
                        {
                            Message ka = new Message();
                            ka.isKeepAliveMessage = true;
                            ka.sendingUser = signInUserName.Text;
                            await Task.Run(()=>Message.SendMessage(client, ka));
                            continue;
                        }
                        if (m.imageContent != null)
                        {
                            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                            img.Width = 400;
                            img.HorizontalAlignment = HorizontalAlignment.Left;
                            ImageSource i = await Task.Run(()=>ConvertFromBitmap(m.imageContent));
                            img.Source = i;
                            chatMessageLog.Children.Add(img);
                        }
                        chatMessageLog.Children.Add(new MessageFrame(m.sendingUser,m.textContent));

                    }
                }
            }

        }

        public async void ConnectToServer(Object sender, RoutedEventArgs evt) {
            await Task.Delay(1);
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
                chatInterface.Visibility = Visibility.Visible;
                signInInterface.Visibility = Visibility.Collapsed;
                ListenForMessages();
            }
        }

        public void SendMessage(Object sender, RoutedEventArgs evt) {
            (sender as Button).Dispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(()=> {
                if (client.Connected && setupComplete)
                {
                    Message m = new Message();
                    m.isUserMessage = true;
                    m.textContent = chatMessageEntry.Text;

                    if (chatDisplayImage.Source != null)
                    {
                        Bitmap bmp = new Bitmap(chatDisplayImage.Source.ToString().Substring(8));
                        m.imageContent = bmp;
                    }

                    m.sendingUser = signInUserName.Text;
                    NetworkStream ns = client.GetStream();
                    IFormatter ifo = new BinaryFormatter();
                    ifo.Serialize(ns, m);

                    chatDisplayImage.Source = null;
                    chatMessageEntry.Text = "";
                }
            }));
        }

        public async void Disconnect(Object sender,CancelEventArgs e) {
            Message dc = Message.DisconnectMessage(username);
            try {
                await Task.Run(()=>Message.SendMessage(client, dc));
                client.Close();
            }
            catch (Exception ex) {
                client.Close();
            }

        }

        public void AttachImage(Object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Directory.GetCurrentDirectory();
            ofd.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == true) {
                var p = ofd.FileName;
                chatDisplayImage.Source = (ImageSource)(new ImageSourceConverter()).ConvertFromString(p);
            }
        }

        public async Task<ImageSource> ConvertFromBitmap(Bitmap b)
        {
            await Task.Delay(1);
            MemoryStream ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

    }
}
