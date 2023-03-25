using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.Models;
using NatterLite_OA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace NatterLite_OA.Infrastructure.Repositories
{
    public class ChatRepository:IChatRepository
    {
        private readonly ApplicationContext db;
        public ChatRepository(
             ApplicationContext context
            )
        {
            db = context;
        }

        public async Task AddAsync(Chat chat)
        {
            await db.Chats.AddAsync(chat);
            await db.SaveChangesAsync();
        }

        public async Task AddMessageAsync(string chatId, Message message)
        {
            Chat chat = await db.Chats
                            .FirstOrDefaultAsync(c => c.Id.ToString() == chatId);
            chat.Messages.Add(message);
            await db.SaveChangesAsync();
        }

        public async Task<Chat> GetByIdAsync(string chatId)
        {
            Chat chat = await db.Chats
                .Include(c => c.Messages)
                    .Include(c => c.Users)
                        .ThenInclude(u => u.BlackList)
                            .FirstOrDefaultAsync(c => c.Id.ToString() == chatId);
            return chat;
        }

        public async Task UpDateLastVisitedAsync(Guid chatId, string change, bool fullychanged)
        {
            Chat chat = await db.Chats
                            .FirstOrDefaultAsync(c => c.Id == chatId);
            if (fullychanged)
                chat.LastVisitedBy = change;
            else
                chat.LastVisitedBy += change;

            await db.SaveChangesAsync();
        }
    }
}
