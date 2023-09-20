using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            List<Category> categoriesList = _unitOfWork.Category.GetAll().ToList();
            return View(categoriesList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category item)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(item);
                _unitOfWork.Save();
                TempData["success"] = "Category successfully added";
                return RedirectToAction("Index", "Category");
            }
            else
                return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
                return NotFound();

            //Category? retrievedCategory = _dbContext.Categories.Find(id);
            //Category? retrievedCategory = _dbContext.Categories.FirstOrDefault(category=>category.CategoryId == id);
            Category? retrievedCategory = _unitOfWork.Category.GetFirstOrDefault(category => category.CategoryId == id);
            if (retrievedCategory == null)
                return NotFound();
            return View(retrievedCategory);
        }
        [HttpPost]
        public IActionResult Edit(Category item)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(item);
                _unitOfWork.Save();
                TempData["success"] = "Category edited successfully";
                return RedirectToAction("Index", "Category");
            }
            else
                return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
                return NotFound();

            Category? retrievedCategory = _unitOfWork.Category.GetFirstOrDefault(element => element.CategoryId == id);
            //Category? retrievedCategory = _dbContext.Categories.FirstOrDefault(category=>category.CategoryId == id);
            //Category? retrievedCategory = _dbContext.Categories.Where(category=>category.CategoryId ==  id).FirstOrDefault();
            if (retrievedCategory == null)
                return NotFound();
            return View(retrievedCategory);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == 0 || id == null)
                return NotFound();
            Category? retrievedCategory = _unitOfWork.Category.GetFirstOrDefault(element => element.CategoryId == id);
            if (retrievedCategory == null)
                return NotFound();


            _unitOfWork.Category.Remove(retrievedCategory);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index");
        }


    }
}
