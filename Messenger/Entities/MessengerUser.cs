using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Messenger.Entities
{
    public class MessengerUser : IdentityUser
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Avatar Avatar { get; set; }

        [ForeignKey("OwnerId")]
        public ICollection<File> Files { get; set; }
        
        [NotMapped]
        public string FullName { get { return $"{FName} {LName}"; } }

        public MessengerUser()
        {
            Files = new List<File>();
        }
    }    
}
