using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.ServiceInterfaces;
using NatterLite_OA.Core.Models;
using NatterLite_OA.WebUI.ViewModels;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using NatterLite_OA.WebUI.Filters;

namespace NatterLite_OA.WebUI.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(IsBannedFilter))]
    public class UserController : Controller
    {
        private readonly ICountryList countriesProvider;
        private readonly IImageValidator imageValidator;
        private readonly IUserRepository userRepository;
        public UserController(
            IUserRepository _userRepository,
            IImageValidator _imageValidator,
            ICountryList _countriesProvider)
        {
            userRepository = _userRepository;
            countriesProvider = _countriesProvider;
            imageValidator = _imageValidator;
        }

        [HttpGet]
        public async Task<IActionResult> SeeProfile(string UserUniqueName)
        {
            User user;
            bool IsCurrentUser = false;
            if (UserUniqueName != null)
            {
                user = await userRepository.GetByUniqueNameAsync(UserUniqueName);
            }
            else
            {
                user = await userRepository.GetByUniqueNameAsync(User.Identity.Name);
                IsCurrentUser = true;
            }

            UserProfileViewModel upvm = new UserProfileViewModel();
            upvm.UserFullName = user.FullName;
            upvm.UserUniqueName = user.UserName;
            upvm.UserStatus = user.Status;
            upvm.UserDateOfBirth = user.DateOfBirth.ToString("dd.MM.yyyy");
            upvm.UserCountry = user.Country;
            upvm.UserProfilePicture = user.ProfilePicture;
            upvm.UserBackgroundPicture = user.BackgroundPicture;
            upvm.IsThisCurrentUser = IsCurrentUser;

            return View(upvm);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            ViewBag.Countries = countriesProvider.CountryList();
            User user=userRepository.GetUserFromCache(User.Identity.Name);

            if (user==null)
                user = await userRepository.GetByUniqueNameAsync(User.Identity.Name);
           
            EditViewModel evm = new EditViewModel();
            evm.FirstName = user.FirstName;
            evm.LastName = user.LastName;
            evm.UniqueName = user.UserName;
            evm.Email = user.Email;
            evm.Country = user.Country;
            evm.DateOfBirth = user.DateOfBirth;

            return View(evm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditViewModel evm)
        {
            ViewBag.Countries = countriesProvider.CountryList();
            User user = await userRepository.GetByUniqueNameAsync(User.Identity.Name);

            bool IsUserUniqueNameCanged = user.UserName != evm.UniqueName;

            if (ModelState.IsValid)
            {
                user.FirstName = evm.FirstName;
                user.LastName = evm.LastName;
                user.FullName = evm.FirstName + " " + evm.LastName;
                user.UserName = evm.UniqueName;
                user.Email = evm.Email;
                user.Country = evm.Country;
                user.DateOfBirth = evm.DateOfBirth;
                if (evm.OldPassword != null && evm.NewPassword != null)
                {
                    var result = await userRepository.ChangePasswordAsync(user, evm.OldPassword, evm.NewPassword);
                    if (!result)
                    {
                        ModelState.AddModelError("OldPassword", "Old password is incorrect!");
                        return View(evm);
                    }
                }
                if (evm.ProfilePicture != null)
                {
                    if (imageValidator.IsImageValid(evm.ProfilePicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(evm.ProfilePicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)evm.ProfilePicture.Length);
                        }
                        user.ProfilePicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("ProfilePicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(evm);
                    }
                }
                if (evm.BackgroundPicture != null)
                {
                    if (imageValidator.IsImageValid(evm.BackgroundPicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(evm.BackgroundPicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)evm.BackgroundPicture.Length);
                        }
                        user.BackgroundPicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("BackgroundPicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(evm);
                    }
                }
                try
                {
                    string userPicturePath = Directory.GetCurrentDirectory() + @$"\wwwroot\SignedUsersPics\{user.UserName}.jpg";
                    using (Image image = Image.FromStream(new MemoryStream(user.ProfilePicture)))
                    {
                        image.Save(userPicturePath, ImageFormat.Jpeg);
                    }
                    HttpContext.Response.Cookies.Append("userPicturePath", $"{user.UserName}.jpg");
                    HttpContext.Response.Cookies.Append("userName", $"{user.FullName}");
                }
                catch
                {
                    return RedirectToAction("Error", "Home");
                }

                if (userRepository.GetUserFromCache(User.Identity.Name) != null)
                {
                    userRepository.RemoveCacheUser(User.Identity.Name);
                    userRepository.CacheUser(user);
                }
                
                await userRepository.UpDateAsync(user);

                if (IsUserUniqueNameCanged) 
                    return RedirectToAction("Logout", "Account");

            }

            return View(evm);
        }

        [HttpPut]
        public async Task<IActionResult> AddToBlackList(string CompanionUniqueName)
        {
            bool result =await userRepository.AddToBlackListAsync(User.Identity.Name,CompanionUniqueName);

            if (result==false)
                return new StatusCodeResult(204);
            else
                return new StatusCodeResult(200);
        }

        [HttpGet]
        public async Task<IActionResult> SeeBlackList()
        {
            var bl = await userRepository.GetBlackListAsync(User.Identity.Name);
            List<User> userBL =bl.ToList();

            List<UserBlackListViewModel> ublvmList = new List<UserBlackListViewModel>();
            if (userBL.Count != 0)
            {
                foreach (User user in userBL)
                {
                    UserBlackListViewModel ublvm = new UserBlackListViewModel();
                    ublvm.UserName = user.FullName;
                    ublvm.UserUniqueName = user.UserName;
                    ublvm.UserProfilePicture = user.ProfilePicture;
                    ublvmList.Add(ublvm);
                }
            }
            return View(ublvmList);

        }

        [HttpPut]
        public async Task<IActionResult> RemoveFromBlackList(string userName)
        {
            User currentUser = await userRepository.GetByUniqueNameAsync(User.Identity.Name);

            await userRepository.RemoveFromBlackListAsync(User.Identity.Name,userName);

            List<UserBlackListViewModel> ublvmList = new List<UserBlackListViewModel>();
            if (currentUser.BlackList.Count != 0)
            {
                foreach (User user in currentUser.BlackList)
                {
                    UserBlackListViewModel ublvm = new UserBlackListViewModel();
                    ublvm.UserName = user.FullName;
                    ublvm.UserUniqueName = user.UserName;
                    ublvm.UserProfilePicture = user.ProfilePicture;
                    ublvmList.Add(ublvm);
                }
            }
            return PartialView("SeeBlackList", ublvmList);
        }

        [HttpPut]
        public async Task<IActionResult> SetUserStatus(string status)
        {
            User currentUser = await userRepository.GetByUniqueNameAsync(User.Identity.Name);
            if (status == "online")
            {
                currentUser.Status = status;
                await userRepository.UpDateAsync(currentUser);
                return new StatusCodeResult(200);
            }
            else
            {
                currentUser.Status = "last seen " + DateTime.Now.ToString("dd.MM.yy,HH:mm");
                await userRepository.UpDateAsync(currentUser);
                return new StatusCodeResult(200);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompanionUserStatus(string userName)
        {
            User companionUser = await userRepository.GetByUniqueNameAsync(userName);
            return Content(companionUser.Status);
        }
    }
}
