using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin )]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string userId) 
        {
            
            UserVM userVM = new()
            {
                User = _unitOfWork.ApplicationUser.GetFirstOrDefault(user => user.Id == userId, includeProperties:"Company"),
                Roles = _roleManager.Roles.Select(role =>  new SelectListItem(role.Name, role.Name)),
                Companies = _unitOfWork.Company.GetAll().Select(compan => new SelectListItem(compan.Name,compan.Id.ToString())),
            };
            
            userVM.User.Role = _userManager.GetRolesAsync(userVM.User).GetAwaiter().GetResult()[0];
            return View(userVM);
        }
        [HttpPost]
        public IActionResult RoleManagement(UserVM userVM) 
        {
            string oldRole = _userManager.GetRolesAsync(userVM.User).GetAwaiter().GetResult()[0];
            ApplicationUser userToUpdate = _unitOfWork.ApplicationUser.GetFirstOrDefault(user => user.Id == userVM.User.Id, includeProperties:"Company", isTracked: true);
            if (oldRole != userVM.User.Role)
            {
                //Role updated
                if(userVM.User.Role == SD.Role_User_Company)
                {
                    userToUpdate.CompanyId = userVM.User.Company.Id;
                }
                if(oldRole == SD.Role_User_Company)
                {
                    userToUpdate.CompanyId = null;
                }

                _userManager.RemoveFromRoleAsync(userToUpdate, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(userToUpdate, userVM.User.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole == SD.Role_User_Company)
                    userToUpdate.CompanyId = userVM.User.Company.Id;
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        #region API Calls

        [HttpGet]
        public IActionResult GetAll() 
        {

            IEnumerable<ApplicationUser> retrievedUsers = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();
            foreach (ApplicationUser user in retrievedUsers)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult()[0];
                if(user.Company == null)
                {
                    user.Company = new() { Name = "" };
                }
            }

            return Json(new {data = retrievedUsers });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id) 
        {
            var userToDelete = _unitOfWork.ApplicationUser.GetFirstOrDefault(user => user.Id == id);
            if(userToDelete == null)
                return Json(new {success = false, message = "Error locking/unlocking user"});


            string msg; 
            if(userToDelete.LockoutEnd != null && userToDelete.LockoutEnd > DateTime.Now)
            {
                userToDelete.LockoutEnd = DateTime.Now;
                msg = "User unlocked";
            }
            else
            {
                userToDelete.LockoutEnd = DateTime.Now.AddDays(7);
                msg = "User locked";

            }
            _unitOfWork.Save();
            return Json(new { success = true, message = msg });
        }
        #endregion
    }
}
