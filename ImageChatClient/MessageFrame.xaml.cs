using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageChatClient
{
    /// <summary>
    /// Interaction logic for MessageFrame.xaml
    /// </summary>
    public partial class MessageFrame : UserControl
    {
        public MessageFrame(string u,string c)
        {
            InitializeComponent();
            messageUser.Content = u+":";
            messageBody.Content = c;
        }
    }
}
