using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.ServiceInterfaces;
using NatterLite_OA.Core.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Drawing.Imaging;
using System.Drawing;
using NatterLite_OA.WebUI.ViewModels;

namespace NatterLite_OA.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly IPicturesProvider picturesProvider;
        private readonly ICountryList countriesProvider;
        private readonly IImageValidator imageValidator;
        private readonly IUserRepository userRepository;
        private readonly IWebHostEnvironment env;

        public AccountController(
            ILogger<AccountController> _logger,
            IConfiguration _configuration,
            IPicturesProvider _picturesProvider,
            ICountryList _countriesProvider,
            IImageValidator _imageValidator,
            IUserRepository _userRepository,
            IWebHostEnvironment _env)
        {
            logger = _logger;
            configuration = _configuration;
            picturesProvider = _picturesProvider;
            countriesProvider = _countriesProvider;
            imageValidator = _imageValidator;
            userRepository = _userRepository;
            env = _env;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("ChatMenu", "Chat");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel lvm)
        {
            if (ModelState.IsValid)
            {
                User user = await userRepository.GetByEmailAsync(lvm.Email);
                if (user != null)
                {
                    if (user.IsBanned)
                    {
                        ModelState.AddModelError(string.Empty, "You were banned by admin");
                        return View();
                    }
                    var result = await userRepository.SignInAsync(user, lvm.Password);
                    if (result)
                    {
                        try
                        {
                            //string userPicturePath = @$"C:\MyApps\NatterLite_OA\NatterLite_OA.WebUI\wwwroot\SignedUsersPics\{user.UserName}.jpg";
                            string userPicturePath = @$"{env.WebRootPath}\SignedUsersPics\{user.UserName}.jpg";
                            using (Image image = Image.FromStream(new MemoryStream(user.ProfilePicture)))
                            {
                                image.Save(userPicturePath, ImageFormat.Jpeg);
                            }
                            HttpContext.Response.Cookies.Append("userPicturePath", $"{user.UserName}.jpg");
                            HttpContext.Response.Cookies.Append("userName", $"{user.FullName}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Can't create an user's profile pictire at SignedUsersPics");
                            return RedirectToAction("Error", "Home");
                        }

                        userRepository.CacheUser(user);

                        return RedirectToAction("ChatMenu", "Chat");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Password or Login");
                    }

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Password or Login");
                }
            }
            return View(lvm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ChatMenu", "Chat");
            }
            ViewBag.Countries = countriesProvider.CountryList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel rvm)
        {
            ViewBag.Countries = countriesProvider.CountryList();
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    Email = rvm.Email,
                    FirstName = rvm.FirstName,
                    LastName = rvm.LastName,
                    FullName = rvm.FirstName + " " + rvm.LastName,
                    UserName = rvm.UniqueName,
                    DateOfBirth = rvm.DateOfBirth,
                    Country = rvm.Country,
                };
                if (rvm.ProfilePicture != null)
                {
                    if (imageValidator.IsImageValid(rvm.ProfilePicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(rvm.ProfilePicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)rvm.ProfilePicture.Length);
                        }
                        user.ProfilePicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("ProfilePicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(rvm);
                    }
                }
                else
                {
                    try
                    {
                        user.ProfilePicture = picturesProvider.GetDefaultPicture(configuration["PicturesPaths:DefaultProfilePicturePath"]);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Can't read default profile picture");
                        return Content($"{ex.Message}");
                    }

                }
                if (rvm.BackgroundPicture != null)
                {
                    if (imageValidator.IsImageValid(rvm.BackgroundPicture))
                    {
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(rvm.BackgroundPicture.OpenReadStream()))
                        {
                            imageData = binaryReader.ReadBytes((int)rvm.BackgroundPicture.Length);
                        }
                        user.BackgroundPicture = imageData;
                    }
                    else
                    {
                        ModelState.AddModelError("BackgroundPicture", "Picture size bigger than 2Mb or has invalid extension");
                        return View(rvm);
                    }
                }
                else
                {
                    try
                    {
                        user.BackgroundPicture = picturesProvider.GetDefaultPicture(configuration["PicturesPaths:DefaultBackgroundPicturePath"]);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Can't read default background picture");
                        return Content($"{ex.Message}");
                    }

                }

                var result = await userRepository.CreateAsync(user, rvm.Password);
                if (result)
                {
                    await userRepository.AddToRoleAsync(user, "user");
                    try
                    {
                        //string userPicturePath = @$"C:\MyApps\NatterLite_OA\NatterLite_OA.WebUI\wwwroot\SignedUsersPics\{user.UserName}.jpg";
                        string userPicturePath = @$"{env.WebRootPath}\SignedUsersPics\{user.UserName}.jpg";
                        using (Image image = Image.FromStream(new MemoryStream(user.ProfilePicture)))
                        {
                            image.Save(userPicturePath, ImageFormat.Jpeg);
                        }
                        HttpContext.Response.Cookies.Append("userPicturePath", $"{user.UserName}.jpg");
                        HttpContext.Response.Cookies.Append("userName", $"{user.FullName}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Can't create an user's profile pictire at SignedUsersPics");
                        return RedirectToAction("Error", "Home");
                    }

                    await userRepository.SignInAsync(user, rvm.Password);

                    userRepository.CacheUser(user);

                    return RedirectToAction("ChatMenu", "Chat");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong while registering");
                }
            }
            return View(rvm);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated) 
                return RedirectToAction("Login", "Account");

            await userRepository.SignOutAsync();

            userRepository.RemoveCacheUser(User.Identity.Name);

            if (System.IO.File.Exists(@$"{env.WebRootPath}\SignedUsersPics\{User.Identity.Name}.jpg"))
            {
                System.IO.File.Delete(@$"{env.WebRootPath}\SignedUsersPics\{User.Identity.Name}.jpg");
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
