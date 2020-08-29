using AutoMapper;
using Messenger.EF;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Entities;
using Messenger.Entities.Models;

namespace Messenger.Repositories
{
    public class DialogService
    {
        MessengerDbContext db;        

        public DialogService(MessengerDbContext db, IMapper mapper)
        {
            this.db = db;            
        }
        public async Task<Dialog> GetDialogBetweenUsers(string[] users)
        {
            string dipper = users[0], mabel = users[1];

            var user1 = db.Users.Find(dipper);
            var user2 = db.Users.Find(mabel);

            if (user1 != null && user2 != null)
            {
                var dialog = db.Dialogs
                    .Include(x => x.Participants)
                    .Include(x => x.Messages).FirstOrDefault(d =>
                        d.Participants.Any(p => p.MessengerUserId == user1.Id) &&
                        d.Participants.Any(p => p.MessengerUserId == user2.Id));

                if (dialog == null)
                {
                    await db.Entry(user1).Reference(x => x.Avatar).LoadAsync();
                    await db.Entry(user2).Reference(x => x.Avatar).LoadAsync();

                    dialog = new Dialog()
                    {
                        Participants = new List<Participant>()
                        {
                            new Participant() {MessengerUserId = user1.Id},
                            new Participant() {MessengerUserId = user2.Id}
                        }
                    };

                    db.Dialogs.Add(dialog);
                    await db.SaveChangesAsync();
                }

                return dialog;
            }
            return null;
        }
        public async Task MarkMessagesAsRead(List<Message> messages)
        {
            foreach (var m in messages)
            {
                var msg = await db.Messages.FindAsync(m.Id);
                msg.Read = true;
            }

            await db.SaveChangesAsync();
        }

        public async Task<ICollection<Message>> GetMessages(Dialog dialog)
        {
            await db.Entry(dialog).Collection(i => i.Messages).LoadAsync();
            return dialog.Messages;
        }
        public async Task<List<Dialog>> GetByUserId(string userId, int count, int offset, bool last)
        {
            return await db.Dialogs
                .Where(x => x.Status != DialogStatus.Canceled)
                .Include(x => x.Participants).ThenInclude(x => x.MessengerUser).ThenInclude(x => x.Avatar)
                .Where(d => d.Participants.Any(p => p.MessengerUserId == userId))
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }
        public async Task WriteMessage(Dialog dialog, Message message)
        {
            if (dialog != null)
            {
                await db.Entry(dialog).Collection(r => r.Messages).LoadAsync();
                dialog.Messages.Add(message);
                await db.SaveChangesAsync();
            }
        }
    }
}
