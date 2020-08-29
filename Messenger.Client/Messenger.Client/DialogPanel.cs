using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Messenger.Common.DTOs;
using Messenger.Entities.Models;
using Xamarin.Forms;

namespace Messenger.Client
{
    public class DialogPanel 
    {
        public DialogDTO Dialog { get; set; }
        public ParticipantDTO Interlocutor { get; set; }
        public DialogInfo Info { get; set; }
        public bool HasUnreadMessages { get; set; }

        public DialogStatus Status
        {
            get => Dialog.Status;
            set => Dialog.Status = value;
        }

        public DialogPanel() { }

        public DialogPanel(AuthObject authObject, DialogDTO dialogDTO)
        {
            Dialog = dialogDTO;
            Interlocutor = dialogDTO.Participants.First(x => x.Id != authObject.User.Id);
        }

        public Uri ImageUrl => new Uri($"{Common.Common.Host}/file/get/{Interlocutor.ImageId}");
    }
}
