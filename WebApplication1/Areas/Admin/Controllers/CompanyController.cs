using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> retrievedComapnies = _unitOfWork.Company.GetAll().ToList();
            return View(retrievedComapnies);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == 0 || id == null)
                return View(new Company());
            else
            {
                Company companyToEdit = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
                return View(companyToEdit);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {

            if (company.Id == 0)
                _unitOfWork.Company.Add(company);
            else
                _unitOfWork.Company.Update(company);

            _unitOfWork.Save();
            TempData["success"] = "Product successfully added";
            return RedirectToAction("Index", "Company");
        }
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> retrievedComapnies = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = retrievedComapnies });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company companyToDelete = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
            if (companyToDelete == null)
                return Json(new { success = false, message = "Error while deleting" });

            _unitOfWork.Company.Remove(companyToDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted" });
        }
        #endregion

    }
}
