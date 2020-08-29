using System;
using System.ComponentModel.DataAnnotations;

namespace Messenger.Entities
{
    public class File
    {
        public Guid Id { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        public string OwnerId { get; set; }
    }
}