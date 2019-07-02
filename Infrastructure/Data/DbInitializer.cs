using ApplicationCore.Interfaces;
using ApplicationCore.Users.Domain;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data
{
    public class DbInitializer
    {
        public static void Initialize(UserManager<ApplicationUser> userManager)
        {
            //create bot user
            var user = new ApplicationUser
            {
                UserName = "bot@chat.com",
                Email = "bot@chat.com",
            };
            userManager.CreateAsync(user, "BotPassword123!").Wait();
            var code = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
            userManager.ConfirmEmailAsync(user, code).Wait();
        }
    }
}