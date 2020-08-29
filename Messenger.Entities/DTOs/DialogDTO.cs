using Messenger.Entities.Models;
using System;
using System.Collections.Generic;

namespace Messenger.Common.DTOs
{
    public class DialogDTO
    {
        public int Id { get; set; }
        public DialogStatus Status { get; set; }
        public DateTime LastActivityUtc { get; set; }
        public virtual ICollection<ParticipantDTO> Participants { get; set; }
    }
}
