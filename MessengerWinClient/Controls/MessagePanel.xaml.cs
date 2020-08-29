using Messenger.Common.DTOs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MessengerWinClient.Controls
{
    public partial class MessagePanel : UserControl
    {
        public string MessageText { get; set; }

        public MessageDTO Message { get; set; }

        public bool IsMine { get; set; }

        public DateTime SentUtc { get; set; }

        public HorizontalAlignment MessagePanelHorizontalAlignment
        {
            get { return IsMine ? HorizontalAlignment.Right : HorizontalAlignment.Left; }
        }

        public string MessagePanelMargin
        {
            get { return IsMine ? "20,5,5,0" : "5,5,20,0"; }
        }

        public string SentString
        {
            get { return SentUtc.ToLocalTime().ToString("g"); }
        }

        public MessagePanel() { }

        public MessagePanel(MessageDTO message, string text, bool isMine, DateTime sentUtc)
        {
            InitializeComponent();
            DataContext = this;
            MessageText = text;
            IsMine = isMine;
            SentUtc = sentUtc;
        }
    }
}
