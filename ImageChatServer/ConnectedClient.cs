using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MessageLibrary;

namespace ImageChatServer
{
    class ConnectedClient
    {
        public TcpClient connection;
        List<Message> parentMessageQueue;
        public bool keepAliveWarned = false;
        public bool isAlive = true;
        public string username;
        public List<string> messages;
        public bool hasMessages = false;

        public void TcpListener() {
        
        } 

        public ConnectedClient(TcpListener server,List<Message> pmq) {
            parentMessageQueue = pmq;
            connection = server.AcceptTcpClient();
            messages = new List<string>();
            if (connection.Connected) {
                Message m = new Message();
                m.sendingUser = "SERVER";
                m.textContent = "YOU HAVE CONNECTED SUCCESSFULLY.";
                Message.SendMessage(connection,m);
            }
            if (this.connection.GetStream().DataAvailable)
            {
                this.username = Message.ReceiveMessage(connection).sendingUser;
            }
            Listen();
            KeepAlive();
        }
        async void KeepAlive() {
            while (connection.Connected) {
                await Task.Delay(10000);
                //Issue a warning if we haven't been issued one.
                if (!keepAliveWarned&&connection.Connected)
                {
                    keepAliveWarned = true;
                    Message ka = new Message();
                    ka.isKeepAliveMessage = true;
                    try
                    {
                        Message.SendMessage(connection, ka);
                    }
                    catch (System.IO.IOException e)
                    {
                        NotifyDisconnect();
                    }
                }
                //If we have been issued a warning, we have timed out.
                else if (keepAliveWarned)
                {
                    NotifyDisconnect();
                }
            }
        }

        void NotifyDisconnect() {
            Message m = Message.DisconnectMessage(username);
            parentMessageQueue.Add(m);
            isAlive = false;
            connection.Close();
        }

        async void Listen() {
            while (connection.Connected)
            {
                if (connection.GetStream().DataAvailable)
                {
                    Message m = Message.ReceiveMessage(connection);
                    //Disconnects and timeouts are handled here, as opposed to the server's loop.
                    if (m.isKeepAliveMessage)
                    {
                        keepAliveWarned = false;
                        m.textContent = " is alive.";
                    }
                    if (m.isDisconnectMessage) {
                        //We don't call NotifyDisconnect since we don't need two messages.
                        isAlive = false;
                        connection.Close();
                    }
                    parentMessageQueue.Add(m);
                }
                await Task.Delay(1);
            }
        }
    }

}
