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
    public class SearchController : Controller
    {
        private readonly ICountryList countriesProvider;
        private readonly IUserRepository userRepository;
        public SearchController(
            ICountryList _countriesProvider,
            IUserRepository _userRepository)
        {
            countriesProvider = _countriesProvider;
            userRepository = _userRepository;
        }

        [HttpGet]
        public IActionResult Search()
        {
            ViewBag.Countries = countriesProvider.CountryList();
            return View();
        }

        [HttpPost]
        public IActionResult SearchResult(string Name, int AgeFrom, int AgeTo, string Country)
        {
            ViewBag.Countries = countriesProvider.CountryList();

            List<User> userList =userRepository.SearchUsersAsync(Name,AgeFrom,AgeTo,Country).Result.ToList();

            List<UserSearchViewModel> usvmList = new List<UserSearchViewModel>();

            if (userList != null)
            {
                foreach (var user in userList)
                {
                    if (user.UserName != User.Identity.Name)
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
            }
            return PartialView("SearchResultPartial", usvmList);
        }
    }
}
