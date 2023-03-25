using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.ServiceInterfaces;
using NatterLite_OA.Core.Models;
using NatterLite_OA.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using NatterLite_OA.WebUI.Filters;

namespace NatterLite_OA.WebUI.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(IsBannedFilter))]
    public class ChatController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IChatRepository chatRepository;
        public ChatController(
            IUserRepository _userRepository,
            IChatRepository _chatRepository
            )
        {
            userRepository = _userRepository;
            chatRepository = _chatRepository;
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(string userId)
        {

            var currentUser = await userRepository.GetByUniqueNameAsync(User.Identity.Name);
            var companionUser = await userRepository.GetByIdAsync(userId);

            if (currentUser.Chats.Count != 0)
            {
                foreach (Chat currentUserChat in currentUser.Chats)
                {
                    if (currentUserChat.Users.Count == 2 &&
                        currentUserChat.Users.Any(u => u.Id == companionUser.Id))
                    {
                        return RedirectToAction("ChatMenu", "Chat");
                    }
                }
            }
            Chat chat = new Chat();
            chat.CreatorUserName = currentUser.UserName;
            chat.CreationTime = DateTime.Now;
            chat.LastVisitedBy = User.Identity.Name + "=" + new DateTime().ToString() + ","
                + companionUser.UserName + "=" + new DateTime().ToString() + ",";
            chat.Users.Add(currentUser);
            chat.Users.Add(companionUser);
            await chatRepository.AddAsync(chat);
            return RedirectToAction("ChatMenu", "Chat");
        }
        [HttpGet]
        public IActionResult ChatMenu()
        {
            return View("ChatMenu");
        }
        [HttpPost]
        public async Task<IActionResult> GetChats(string currentchatId = null)
        {
            var currentUser = await userRepository.GetByUniqueNameAsync(User.Identity.Name);

            List<Chat> currentUserChats = new List<Chat>();
            currentUserChats = currentUser.Chats;
            List<ChatViewModel> chatViewModelList = new List<ChatViewModel>();
            if (currentUserChats.Count != 0)
            {
                foreach (Chat chat in currentUserChats)
                {
                    if (chat.CreatorUserName == User.Identity.Name || chat.Messages.Count != 0)
                    {
                        ChatViewModel chatViewModel = new ChatViewModel();
                        chatViewModel.ChatId = chat.Id.ToString();

                        if (chat.Messages.Count != 0)
                        {
                            chatViewModel.TimeForCompare = chat.Messages[chat.Messages.Count - 1].Time;
                            chatViewModel.LastMessageTime = chat.Messages[chat.Messages.Count - 1].Time;
                            string LastMessageText = chat.Messages[chat.Messages.Count - 1].Text;
                            if (LastMessageText.Length <= 80)
                            {
                                chatViewModel.LastMessageText = LastMessageText;
                            }
                            else
                            {
                                chatViewModel.LastMessageText = LastMessageText.Substring(0, 77) + "...";
                            }
                            if (chat.LastVisitedBy != null)
                            {
                                DateTime timeToCompare = new DateTime();
                                char[] separators = new char[] { ',', '=' };
                                string[] subs = chat.LastVisitedBy.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < subs.Length; i += 2)
                                {
                                    if (subs[i] == User.Identity.Name)
                                    {
                                        timeToCompare = DateTime.Parse(subs[i + 1]);
                                    }
                                }
                                if (currentchatId == chatViewModel.ChatId)
                                {
                                    chatViewModel.UnreadMessagesCount = 0.ToString();
                                }
                                else
                                {
                                    int mesCount = chat.Messages
                                        .Where(m => m.Time > timeToCompare)
                                        .ToList().Count;
                                    if (mesCount < 100)
                                    {
                                        chatViewModel.UnreadMessagesCount = mesCount.ToString();
                                    }
                                    else
                                    {
                                        chatViewModel.UnreadMessagesCount = "99+";
                                    }
                                }
                            }
                            else
                            {
                                chatViewModel.UnreadMessagesCount = 0.ToString();
                            }
                        }
                        else
                        {
                            chatViewModel.TimeForCompare = chat.CreationTime;
                            chatViewModel.LastMessageTime = new DateTime();
                            chatViewModel.LastMessageText = " ";
                            chatViewModel.UnreadMessagesCount = 0.ToString();
                        }

                        chatViewModel.CompanionUserName = chat.Users.Find(u => u.Id != currentUser.Id).FullName;
                        chatViewModel.CompanionUserProfilePicture = chat.Users.Find(u => u.Id != currentUser.Id).ProfilePicture;
                        chatViewModelList.Add(chatViewModel);
                    }
                }
                chatViewModelList.Sort((This, Next) => This.TimeForCompare < Next.TimeForCompare ? 1 : -1);
            }
            return PartialView("ChatListPartial", chatViewModelList);
        }
        [HttpPost]
        public async Task<IActionResult> WriteLastVisitedTimeForChat(string chatId)
        {
            Chat chat = await chatRepository.GetByIdAsync(chatId);
            if (chat.LastVisitedBy != null)
            {
                char[] separators = new char[] { ',', '=' };
                string[] subs = chat.LastVisitedBy.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                bool isTheSame = false;
                for (int i = 0; i < subs.Length; i += 2)
                {
                    if (subs[i] == User.Identity.Name)
                    {
                        subs[i + 1] = DateTime.Now.ToString();
                        isTheSame = true;
                    }
                }
                for (int i = 0; i < subs.Length; i++)
                {
                    if (i == 0 || i % 2 == 0)
                    {
                        subs[i] += "=";
                    }
                    else
                    {
                        subs[i] += ",";
                    }
                }
                if (isTheSame)
                {
                    await chatRepository.UpDateLastVisitedAsync(chat.Id, String.Concat(subs),true);
                    return new EmptyResult();
                }
            }
            string changeOfLastVisitedBy = User.Identity.Name + "=" + DateTime.Now.ToString() + ",";
            await chatRepository.UpDateLastVisitedAsync(chat.Id, changeOfLastVisitedBy, false);
            return new EmptyResult();
        }
        [HttpPost]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            Chat chat = await chatRepository.GetByIdAsync(chatId);
           
            User currentUser = chat.Users.Find(u => u.UserName == User.Identity.Name);
            User companionUser = chat.Users.Find(u => u.UserName != User.Identity.Name);

            MessagesViewModel mvm = new MessagesViewModel();
            mvm.CompanionUserIdentityName = companionUser.UserName;
            mvm.CompanionUserName = companionUser.FullName;
            mvm.CompanionUserProfilePicture = companionUser.ProfilePicture;
            mvm.CompanionUserStatus = companionUser.Status;

            if (currentUser.BlackList.Exists(u => u.UserName == companionUser.UserName)) 
                mvm.DidCurrentUserAddedCompanionUserToBlackList = true;
            if (companionUser.BlackList.Exists(u => u.UserName == currentUser.UserName))
                mvm.DidCompanionUserAddedCurrentUserToBlackList = true;

            if (chat.Messages.Count != 0)
            {
                foreach (Message mes in chat.Messages)
                {
                    mvm.Messages.Add(mes);
                }
                mvm.Messages.Reverse();
            }
            return PartialView("MessagesViewPartial", mvm);
        }
    }
}
