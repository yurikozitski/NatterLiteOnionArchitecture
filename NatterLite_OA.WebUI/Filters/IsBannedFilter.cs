using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.Models;

namespace NatterLite_OA.WebUI.Filters
{
    public class IsBannedFilter : Attribute, IAsyncResourceFilter
    {
        private readonly IUserRepository userRepository;
        public IsBannedFilter(
           IUserRepository _userRepository
            )
        {
            userRepository = _userRepository;
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
                                    ResourceExecutionDelegate next)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated) await next();

            User user=userRepository.GetUserFromCache(context.HttpContext.User.Identity.Name);

            if (user == null)
                user =await userRepository.GetByUniqueNameAsync(context.HttpContext.User.Identity.Name);

            if (user.IsBanned)
                context.Result = new RedirectToActionResult("Logout", "Account", new { });
            else
                await next();
        }
    }
}
