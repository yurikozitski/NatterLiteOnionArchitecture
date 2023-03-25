using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NatterLite_OA.Core.Models;

namespace NatterLite_OA.Core.RepositoryInterfaces
{
    public interface IUserRepository
    {
        void CacheUser(User user);
        User GetUserFromCache(string userIdentityName);
        void RemoveCacheUser(string userIdentityName);
        Task<bool> CreateAsync(User user, string password);
        Task<bool> AddToRoleAsync(User user, string roleName);
        Task<bool> AddToBlackListAsync(string currentUserUniqueName, string userToAddUniqueName);
        Task<bool> RemoveFromBlackListAsync(string currentUserUniqueName, string userToRemoveUniqueName);
        Task<IEnumerable<User>> GetBlackListAsync(string currentUserUniqueName);
        Task<bool> SignInAsync(User user,string password);
        Task<bool> ChangePasswordAsync(User user,string oldPassword, string newPassword);
        Task SignOutAsync();
        Task<bool> UpDateAsync(User user);
        Task<bool> CheckUserRoleAsync(string roleName);
        public Task CreateUserRoleAsync(string roleName);
        Task<User> GetByIdAsync(string userId);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUniqueNameAsync(string uniqueName);
        Task<IEnumerable<User>> SearchUsersAsync(string Name, int AgeFrom, int AgeTo, string Country);
        Task<IEnumerable<User>> GetBannedUsers(string Name);
    }
}
