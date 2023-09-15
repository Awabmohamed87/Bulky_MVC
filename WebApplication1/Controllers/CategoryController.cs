using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        public CategoryController(ApplicationDBContext dbContext)
        {

            _dbContext = dbContext;

        }
        public IActionResult Index()
        {
            List<Category> categoriesList = _dbContext.Categories.ToList();
            return View(categoriesList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category item)
        {
            if(ModelState.IsValid)
            {
                _dbContext.Categories.Add(item);
                _dbContext.SaveChanges();
                TempData["success"] = "Category successfully added";
                return RedirectToAction("Index","Category");
            }   
            else 
                return View();
        }
        public IActionResult Edit(int? id)
        {
            if(id == 0|| id == null)
                return NotFound();

            Category? retrievedCategory = _dbContext.Categories.Find(id);
            //Category? retrievedCategory = _dbContext.Categories.FirstOrDefault(category=>category.CategoryId == id);
            //Category? retrievedCategory = _dbContext.Categories.Where(category=>category.CategoryId ==  id).FirstOrDefault();
            if(retrievedCategory == null)
                return NotFound();
            return View(retrievedCategory);
        }
        [HttpPost]
        public IActionResult Edit(Category item)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Categories.Update(item);
                _dbContext.SaveChanges();
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

            Category? retrievedCategory = _dbContext.Categories.Find(id);
            //Category? retrievedCategory = _dbContext.Categories.FirstOrDefault(category=>category.CategoryId == id);
            //Category? retrievedCategory = _dbContext.Categories.Where(category=>category.CategoryId ==  id).FirstOrDefault();
            if (retrievedCategory == null)
                return NotFound();
            return View(retrievedCategory);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if(id == 0|| id == null) 
                return NotFound();
            Category? retrievedCategory = _dbContext.Categories.Find(id);
            if (retrievedCategory == null)
                return NotFound();

            
                _dbContext.Categories.Remove(retrievedCategory);
                _dbContext.SaveChanges();
                TempData["success"] = "Category deleted successfully";

            return RedirectToAction("Index");
        }


    }
}
