using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitializer
{
    public class Dbinitalizer : IDbInitiaizer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDBContext _db;

        public Dbinitalizer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDBContext db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public void Initialize()
        {
            // 1. Apply not applied migrations
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }

            // 2. Create roles if not created
            if (!_roleManager.RoleExistsAsync(SD.Role_User_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

                // 3. if roles are not created, then we will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "testAdmin@gmail.com",
                    Email = "testAdmin@gmail.com",
                    Name = "Awab",
                    PhoneNumber = "1234567890",
                    StreetAddress = "1234567890",
                    State = "Hadayek",
                    City = "Cairo",
                    PostalCode = "45961"

                }, "Admin123*").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(user => user.Email == "testAdmin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
        }
    }
}
