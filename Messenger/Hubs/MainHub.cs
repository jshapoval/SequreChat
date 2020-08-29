using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Messenger.EF;
using Messenger.Entities;
using Messenger.Entities.Models;
using Messenger.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Hubs
{
    [Authorize]
    public class MainHub : Hub
    {
        private readonly MessengerDbContext _context;
        private readonly DialogService _dialogService;

        private static readonly HashSet<string> ConnectedUsers = new HashSet<string>();

        public MainHub(DialogService dialogService, MessengerDbContext context)
        {
            _context = context;
            _dialogService = dialogService;
        }

        public async Task SendMessage(int dialogId, string data, string ownerEncKey, string recipientEncKey)
        {
            var currentUserId = Context.UserIdentifier;

            var dialog = await _context.Dialogs
                .Include(x => x.Participants).FirstOrDefaultAsync(x =>
                    x.Id.Equals(dialogId) && x.Status == DialogStatus.Active);

            if (dialog == null)
                return;

            var currentUser = dialog.Participants.FirstOrDefault(x => x.MessengerUserId.Equals(currentUserId));

            if (currentUser == null)
                return;

            var anotherUser = dialog.Participants.FirstOrDefault(x =>
                !x.MessengerUserId.Equals(currentUser.MessengerUserId));

            var message = new Message
            {
                Body = data,
                OwnerRcaEncKey = ownerEncKey,
                RecipientRcaEncKey = recipientEncKey,
                Date = DateTime.UtcNow,
                OwnerId = currentUserId
            };

            await _dialogService.WriteMessage(dialog, message);

            dialog.LastActivityUtc = DateTime.UtcNow;
            _context.Dialogs.Update(dialog);
            await _context.SaveChangesAsync();

            await Clients.Users(dialog.Participants.Select(x => x.MessengerUserId).ToList())
                .SendAsync("Notify", message);
        }

        public async Task OnDialogCreated()
        {
            await CheckDialogsAwaitingActivation(Context.UserIdentifier);
        }

        public override async Task OnConnectedAsync()
        {
            var currentUserId = Context.UserIdentifier;

            ConnectedUsers.Add(currentUserId);

            await CheckDialogsAwaitingActivation(currentUserId);

            await base.OnConnectedAsync();
        }

        private async Task CheckDialogsAwaitingActivation(string currentUserId)
        {
            var dialogsAwaitingActivation = await _context.Dialogs
                .Include(x => x.Participants)
                .Where(x => x.Status == DialogStatus.AwaitingKeyExchange)
                .Where(x =>
                    x.Participants.Any(p => p.MessengerUserId.Equals(currentUserId)) &&
                    ConnectedUsers.Contains(x.Participants.First(p => !p.MessengerUserId.Equals(currentUserId))
                        .MessengerUserId)).ToArrayAsync();

            foreach (var dialog in dialogsAwaitingActivation)
            {
                await Clients.User(currentUserId).SendAsync("InitKeyExchange",
                    dialog.Id, null, false);
            }
        }

        public async Task KeyExchange(int dialogId, string key)
        {
            var currentUserId = Context.UserIdentifier;

            var dialogAwaitingActivation = await _context.Dialogs
                .Include(x => x.Participants).FirstOrDefaultAsync(x =>
                    x.Id.Equals(dialogId) && x.Status == DialogStatus.AwaitingKeyExchange);

            if (dialogAwaitingActivation == null)
                return;

            var currentUser = dialogAwaitingActivation.Participants.FirstOrDefault(x => x.MessengerUserId.Equals(currentUserId));

            if(currentUser == null)
                return;

            var anotherUser = dialogAwaitingActivation.Participants.FirstOrDefault(x =>
                !x.MessengerUserId.Equals(currentUser.MessengerUserId));

            anotherUser.KeyReceived = true;

            var keyExchangeCompleted = dialogAwaitingActivation.Participants.All(x => x.KeyReceived);

            await Clients.User(anotherUser.MessengerUserId).SendAsync("InitKeyExchange", dialogId, key, keyExchangeCompleted);

            _context.Participants.Update(anotherUser);

            if (keyExchangeCompleted)
            {
                dialogAwaitingActivation.Status = DialogStatus.Active;
                _context.Dialogs.Update(dialogAwaitingActivation);
            }

            await _context.SaveChangesAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUsers.Remove(Context.UserIdentifier);
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
