using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.Models;

namespace NatterLite_OA.WebUI.SignalR
{
    public class ChatHub : Hub
    {
        private readonly IUserRepository userRepository;
        private readonly IChatRepository chatRepository;
        public ChatHub(
            IUserRepository _userRepository,
            IChatRepository _chatRepository
            )
        {
            userRepository = _userRepository;
            chatRepository = _chatRepository;
        }
        public async Task Send(string message, string reciever, string chatId)
        {
            if (!userRepository.GetByUniqueNameAsync(reciever).Result
                .BlackList.Exists(u => u.UserName == Context.UserIdentifier))
            {
                Message mes = new Message();
                mes.SenderUserName = Context.User.Identity.Name;
                mes.Text = message;
                mes.Time = DateTime.Now;
                await chatRepository.AddMessageAsync(chatId,mes);
                await Clients.User(Context.UserIdentifier).SendAsync("Receive", message, chatId, "fromCurrentUser");
                await Clients.User(reciever).SendAsync("Receive", message, chatId, "NotfromCurrentUser");
            }
        }
    }
}
