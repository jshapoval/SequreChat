using System;
using System.Collections.Generic;
using System.Text;
using Messenger.Common.DTOs;
using Xamarin.Forms;

namespace Messenger.Client
{
    public class MessagePanel
    {
        public string MessageText { get; set; }

        public MessageDTO Message { get; set; }

        public bool IsMine { get; set; }

        public DateTime SentUtc { get; set; }

        public TextAlignment MessagePanelHorizontalAlignment
        {
            get { return IsMine ? TextAlignment.End : TextAlignment.Start; }
        }

        public Thickness MessagePanelMargin
        {
            get { return IsMine ? new Thickness(20, 5, 5, 0) : new Thickness(5, 5, 20, 0); }
        }

        public string SentString
        {
            get { return SentUtc.ToLocalTime().ToString("g"); }
        }

        public MessagePanel() { }

        public MessagePanel(MessageDTO message, string text, bool isMine, DateTime sentUtc)
        {
            MessageText = text;
            IsMine = isMine;
            SentUtc = sentUtc;
        }
    }
}
