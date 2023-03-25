using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NatterLite_OA.Core.Models;

namespace NatterLite_OA.Core.RepositoryInterfaces
{
    public interface IChatRepository
    {
        Task AddAsync(Chat chat);
        Task AddMessageAsync(string chatId, Message message);
        Task<Chat> GetByIdAsync(string chatId);
        Task UpDateLastVisitedAsync(Guid chatId, string change,bool fullychanged);
    }
}
