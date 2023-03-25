using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using NatterLite_OA.Core.ServiceInterfaces;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.Models;

namespace NatterLite_OA.Infrastructure.Services
{
    public class DataInitializer:IDataInitializer
    {
        public async Task InitializeAsync(
            IUserRepository userRepository,
            IPicturesProvider picturesProvider)
        {
            string adminEmail = "admin@gmail.com";
            string password = "admin123456789";
            string uniqueName = "@admin";
            string firstName = "Super";
            string lastName = "Admin";
            string fullName = firstName + " " + lastName;
            string country = "Ukraine";
            
            byte[] profilePicture = picturesProvider.GetDefaultPicture(@$"{Directory.GetCurrentDirectory()}\wwwroot\Images\DefaultUsersPictures\admin.jpg");
            byte[] backgroundPicture = picturesProvider.GetDefaultPicture(@$"{Directory.GetCurrentDirectory()}\wwwroot\Images\DefaultUsersPictures\DefaultBackgroundPicture.jpg");
            DateTime dateOfBirth = new DateTime(1991, 04, 21);

            if (await userRepository.CheckUserRoleAsync("admin") == false)
            {
                await userRepository.CreateUserRoleAsync("admin");
            }
            if (await userRepository.CheckUserRoleAsync("user") == false)
            {
                await userRepository.CreateUserRoleAsync("user");
            }
            if (await userRepository.GetByUniqueNameAsync(uniqueName) == null)
            {
                User admin = new User
                {
                    Email = adminEmail,
                    UserName = uniqueName,
                    FirstName = firstName,
                    LastName = lastName,
                    FullName = fullName,
                    Country = country,
                    DateOfBirth = dateOfBirth,
                    ProfilePicture = profilePicture,
                    BackgroundPicture = backgroundPicture,
                };
                bool result = await userRepository.CreateAsync(admin, password);
                if (result)
                {
                    await userRepository.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
