using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NatterLite_OA.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace NatterLite_OA.Infrastructure.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly ApplicationContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMemoryCache cache;
        public UserRepository(
             ApplicationContext context,
            UserManager<User> _userManager,
            SignInManager<User> _signInManager,
            RoleManager<IdentityRole> _manager,
            IMemoryCache _memoryCache)
        {
            db = context;
            userManager = _userManager;
            signInManager = _signInManager;
            roleManager = _manager;
            cache = _memoryCache;
        }

        public void CacheUser(User user) 
        {
            cache.Set(user.UserName, user, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
        }

        public User GetUserFromCache(string userIdentityName)
        {
            cache.TryGetValue(userIdentityName, out User user);
            return user;
        }

        public void RemoveCacheUser(string userIdentityName)
        {
            if (cache.TryGetValue(userIdentityName, out User user)) 
                cache.Remove(userIdentityName);
        }

        public async Task<bool> CreateAsync(User user,string password)
        {
            var result = await userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> AddToRoleAsync(User user, string roleName)
        {
            var result =await userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> AddToBlackListAsync(string currentUserUniqueName, string userToAddUniqueName)
        {
            User userToBlacklist =await userManager.FindByNameAsync(userToAddUniqueName);
            User currentUser =await db.Users.Include(u => u.BlackList)
                .FirstOrDefaultAsync(u => u.UserName == currentUserUniqueName);

            if (currentUser.BlackList.Exists(u => u.UserName == userToAddUniqueName))
            {
                return false;
            }
            else
            {
                currentUser.BlackList.Add(userToBlacklist);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> RemoveFromBlackListAsync(string currentUserUniqueName, string userToRemoveUniqueName)
        {
            User currentUser =await db.Users.Include(u => u.BlackList)
                .FirstOrDefaultAsync(u => u.UserName == currentUserUniqueName);
                    
            User userToRemove = currentUser.BlackList.Find(u => u.UserName == userToRemoveUniqueName);

            bool result=currentUser.BlackList.Remove(userToRemove);
            await db.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<User>> GetBlackListAsync(string currentUserUniqueName)
        {
            User user = await db.Users.Include(u => u.BlackList)
                .FirstOrDefaultAsync(u => u.UserName == currentUserUniqueName);
            return user.BlackList;
        }
        
        public async Task<bool> SignInAsync(User user, string password)
        {
            var result =await signInManager.PasswordSignInAsync(user, password,false,false);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            var result =await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }

        public Task SignOutAsync()
        {
            return signInManager.SignOutAsync();
        }

        public async Task<bool> UpDateAsync(User user)
        {
            var result =await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> CheckUserRoleAsync(string roleName)
        {
            if (await roleManager.FindByNameAsync(roleName) == null)
                return false;
            else
                return true;
        }

        public async Task CreateUserRoleAsync(string roleName)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public Task<User> GetByIdAsync(string userId)
        {
            return db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<User> GetByEmailAsync(string email) 
        {
            return db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByUniqueNameAsync(string uniqueName)
        {
            var user = await db.Users
                .Include(u => u.BlackList)
                    .Include("Chats.Users") 
                        .Include("Chats.Messages")
                            .FirstOrDefaultAsync(u => u.UserName == uniqueName);
            return user;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string Name, int AgeFrom, int AgeTo, string Country)
        {
            bool IsNameFailed = false;
            List<User> userList = new List<User>();
            if (Name != null)
            {
                if (Name.StartsWith("@"))
                {
                    var user =await db.Users.FirstOrDefaultAsync(u => u.UserName == Name);
                    if (user != null)
                    {
                        userList.Add(user);
                    }
                    else
                    {
                        IsNameFailed = true;
                    }
                }
                else
                {
                    var users =await userManager.Users.Where(u => u.FullName.Contains(Name)).ToListAsync();
                    if (users.Count != 0)
                    {
                        foreach (var User in users)
                        {
                            userList.Add(User);
                        }
                    }
                    else
                    {
                        IsNameFailed = true;
                    }
                }
            }
            bool IsAgeFromFailed = false;
            bool IsAgeToFailed = false;

            if (AgeFrom > AgeTo && AgeTo != 0) (AgeFrom, AgeTo) = (AgeTo, AgeFrom);
            if (AgeFrom != 0 && !IsNameFailed)
            {
                DateTime now = DateTime.Now;
                DateTime searchedDate = now.AddYears(-AgeFrom);
                if (userList.Count != 0)
                {
                    userList = userList.Where(u => u.DateOfBirth <= searchedDate).ToList();
                }
                else
                {
                    userList =await userManager.Users.Where(u => u.DateOfBirth <= searchedDate).ToListAsync();
                }
                if (userList.Count == 0) IsAgeFromFailed = true;
            }
            if (AgeTo != 0 && !IsNameFailed)
            {
                DateTime now = DateTime.Now;
                DateTime searchedDate = now.AddYears(-AgeTo);
                if (userList.Count != 0)
                {
                    userList = userList.Where(u => u.DateOfBirth >= searchedDate).ToList();
                }
                else
                {
                    userList =await userManager.Users.Where(u => u.DateOfBirth >= searchedDate).ToListAsync();
                }
                if (userList.Count == 0) IsAgeToFailed = true;
            }
            if (Country != "NoCountry" && !IsNameFailed && !IsAgeFromFailed && !IsAgeToFailed)
            {
                if (userList.Count != 0)
                {
                    userList = userList.Where(u => u.Country == Country).ToList();
                }
                else
                {
                    userList =await userManager.Users.Where(u => u.Country == Country).ToListAsync();
                }
            }
            return userList;
        }

        public async Task<IEnumerable<User>> GetBannedUsers(string Name)
        {
            List<User> searchedUsers = new List<User>();

            if (Name != null)
            {
                if (Name.StartsWith("@"))
                {
                    searchedUsers = await db.Users.Where(u => u.UserName == Name && u.IsBanned).ToListAsync();
                }
                else
                {
                    searchedUsers = await db.Users.Where(u => u.FullName.Contains(Name) && u.IsBanned).ToListAsync();
                }
            }
            else
            {
                searchedUsers = await db.Users.Where(u => u.IsBanned).ToListAsync();
            }

            return searchedUsers;
        }
    }
}
