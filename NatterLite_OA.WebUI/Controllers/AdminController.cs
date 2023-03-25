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

namespace NatterLite_OA.WebUI.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository userRepository;
        public AdminController(
            IUserRepository _userRepository
            )
        {
            userRepository = _userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(string userName)
        {
            User user = await userRepository.GetByUniqueNameAsync(userName);
            if (user == null) 
                return RedirectToAction("Search", "Search");

            user.IsBanned = true;
            await userRepository.UpDateAsync(user);

            userRepository.RemoveCacheUser(userName);
            userRepository.CacheUser(user);

            return new StatusCodeResult(200);
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userName)
        {
            User user = await userRepository.GetByUniqueNameAsync(userName);
            if (user == null) 
                return RedirectToAction("Search", "Search");

            user.IsBanned = false;
            await userRepository.UpDateAsync(user);

            userRepository.RemoveCacheUser(userName);
            userRepository.CacheUser(user);

            return new StatusCodeResult(200);
        }

        [HttpGet]
        public IActionResult BannedUsers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetBannedUsers(string Name)
        {
            IEnumerable<User> bannedUsers = await userRepository.GetBannedUsers(Name);
            List<User> searchedUsers = bannedUsers.ToList();

            List<UserSearchViewModel> usvmList = new List<UserSearchViewModel>();
            
            if (searchedUsers.Count != 0)
            {
                foreach (User user in searchedUsers)
                {
                    UserSearchViewModel usvm = new UserSearchViewModel();
                    usvm.UserId = user.Id;
                    usvm.UserName = user.FullName;
                    usvm.UserUniqueName = user.UserName;
                    usvm.UserProfilePicture = user.ProfilePicture;
                    usvm.IsBanned = user.IsBanned;
                    usvmList.Add(usvm);
                }
            }

            return PartialView("BannedUsersSearchResultPartial", usvmList);
        }
    }
}
