using System;

namespace Messenger.Common.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }

        public int DialogId { get; set; }

        public string OwnerId { get; set; }

        public string Body { get; set; }

        public string OwnerRcaEncKey { get; set; }

        public string RecipientRcaEncKey { get; set; }

        public DateTime Date { get; set; }

        public bool Read { get; set; }
    }
}
