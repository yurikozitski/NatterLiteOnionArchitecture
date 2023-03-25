using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite_OA.Core.Models;
using NatterLite_OA.Core.RepositoryInterfaces;

namespace NatterLite_OA.WebUI.Controllers
{
    public class DataCheckController : Controller
    {
        private readonly IUserRepository userRepository;
        public DataCheckController(IUserRepository _userRepository)
        {
            userRepository = _userRepository;
        }
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            User user = await userRepository.GetByEmailAsync(email);
            if (user != null)
                return Json(false);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmail_Edit(string email)
        {
            User currentuser = await userRepository.GetByUniqueNameAsync(User.Identity.Name);
            User user = await userRepository.GetByEmailAsync(email);
            if (user != null && currentuser.Id == user.Id) return Json(true);
            if (user != null)
                return Json(false);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckUniqueName(string uniqueName)
        {
            User user = await userRepository.GetByUniqueNameAsync(uniqueName);
            if (user != null)
                return Json(false);
            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckUniqueName_Edit(string uniqueName)
        {
            User currentuser = await userRepository.GetByUniqueNameAsync(User.Identity.Name);
            User user = await userRepository.GetByUniqueNameAsync(uniqueName);
            if (user != null && currentuser.Id == user.Id) return Json(true);
            if (user != null)
                return Json(false);
            return Json(true);
        }
    }
}
