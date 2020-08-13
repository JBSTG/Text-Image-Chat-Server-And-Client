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
        public static Message ReceiveMessage(TcpClient connection) {
            NetworkStream ns = connection.GetStream();
            IFormatter ifo = new BinaryFormatter();
            Message m = (Message)ifo.Deserialize(ns);
            return m;
        }

        public static void SendMessage(TcpClient connection, Message m) {
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

        public bool isUserMessage;
        public bool isServerMessage;
        public bool isConnectingMessage;
        public bool isKeepAliveMessage;
        public bool isDisconnectMessage;
        public string sendingUser;
        public string textContent;
        public Bitmap imageContent = null;
    }
}
