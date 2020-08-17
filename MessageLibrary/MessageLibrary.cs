using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Drawing;

namespace MessageLibrary
{
    [Serializable]
    public class Message
    {
        public async static Task<Message> ReceiveMessage(TcpClient connection) {
            await Task.Delay(1);
            IFormatter ifo = new BinaryFormatter();
            NetworkStream ns = connection.GetStream();
            Message m = (Message)ifo.Deserialize(ns);
            return m;
        }

        public async static Task SendMessage(TcpClient connection, Message m) {
            await Task.Delay(1);
            NetworkStream ns = connection.GetStream();
            IFormatter ifo = new BinaryFormatter();
            ifo.Serialize(ns, m);
        }

        public static Message DisconnectMessage(string username) {
            Message dc = new Message();
            dc.isDisconnectMessage = true;
            dc.sendingUser = username;
            dc.textContent = " disconnected.";
            return dc;
        }

        public static Message SetupCompleteMessage() {
            Message sec = new Message();
            sec.textContent = "You are now connected.";
            sec.sendingUser = "SERVER";
            sec.isSetupCompleteMessage = true;
            return sec;
        }
        public bool isUserMessage;
        public bool isServerMessage;
        public bool isConnectingMessage;
        public bool isKeepAliveMessage;
        public bool isDisconnectMessage;
        public bool isSetupCompleteMessage;
        public string sendingUser;
        public string textContent;
        public Bitmap imageContent = null;
    }
}
