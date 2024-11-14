using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Owin;
using ShopOnline.Models;
using System;
using System.Threading.Tasks;

namespace ShopOnline
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesAndUser().Wait();
        }

        public async Task CreateRolesAndUser()
        {
            using (var appDbContext = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(appDbContext));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(appDbContext));

                // Create Admin Role
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    // Create Admin Role
                    var role = new IdentityRole("Admin");
                    await roleManager.CreateAsync(role);

                    // Create Admin User
                    var user = new ApplicationUser()
                    {
                        UserName = "admin@shoponline.com",
                        Email = "admin01@gmail.com.com"
                    };

                    string userPWD = "Admin@123";

                    var chkUser = await userManager.CreateAsync(user, userPWD);

                    // Add User to Role
                    if (chkUser.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user.Id, "Admin");
                    }
                }

                // Create User Role
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    var role = new IdentityRole("User");
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}
