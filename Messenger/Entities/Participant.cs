using System.ComponentModel.DataAnnotations;

namespace Messenger.Entities
{
    public class Participant
    {
        public int DialogId { get; set; }

        [Required]
        public string MessengerUserId { get; set; }
        public MessengerUser MessengerUser { get; set; }
        public bool KeyReceived { get; set; }
    }
}
