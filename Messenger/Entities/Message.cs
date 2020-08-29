using System;
using System.ComponentModel.DataAnnotations;

namespace Messenger.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int DialogId { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        [StringLength(maximumLength: 345, MinimumLength = 345)]
        public string OwnerRcaEncKey { get; set; }

        [Required]
        [StringLength(maximumLength: 345, MinimumLength = 345)]
        public string RecipientRcaEncKey { get; set; }
        
        public DateTime Date { get; set; }

        public bool Read { get; set; }
    }
}
