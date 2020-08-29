using System;
using System.Collections.Generic;
using Messenger.Entities.Models;

namespace Messenger.Entities
{
    public class Dialog
    {
        public int Id { get; set; }
        public DialogStatus Status { get; set; }
        public virtual ICollection<Participant> Participants { get; set; }
        public ICollection<Message> Messages { get; set; }
        public DateTime LastActivityUtc { get; set; }

        public Dialog()
        {
            Messages = new List<Message>();
            Participants = new List<Participant>();
        }
    }
}
