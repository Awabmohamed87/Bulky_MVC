using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment; 
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> retrievedProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(retrievedProducts);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM;
            IEnumerable<SelectListItem> retrievedCategories = _unitOfWork.Category.GetAll().Select(category => new SelectListItem(category.Name, category.CategoryId.ToString()));
            if (id == null || id == 0)
            {
                productVM = new()
                {
                    Categories = retrievedCategories,
                    product = new Product()
                };
            }
            else
            {
                productVM = new()
                {
                    Categories = retrievedCategories,
                    product = _unitOfWork.Product.GetFirstOrDefault(product => product.Id == id)
                };
                
            }

            
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productvm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwrootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwrootPath, @"images\product");
                    if (!string.IsNullOrEmpty(productvm.product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwrootPath,productvm.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create)) 
                    {
                        file.CopyTo(fileStream);

                    }
                    productvm.product.ImageUrl = @"\images\product\" + fileName;
                }
                if (productvm.product.Id == 0)
                    _unitOfWork.Product.Add(productvm.product);
                else
                    _unitOfWork.Product.Update(productvm.product);
                _unitOfWork.Save();
                TempData["success"] = "Product successfully added";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productvm = new()
                {
                    Categories = _unitOfWork.Category.GetAll().Select(category => new SelectListItem(category.Name, category.DisplayOrder.ToString())),
                    product = new Product()
                };
                return View(productvm);
            }
        }
       
        
        #region API Calls
        [HttpGet]
        public IActionResult GetAll() 
        {
            List<Product> retrievedProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = retrievedProducts });
        }
        [HttpDelete]
        public IActionResult Delete(int? id) 
        {
            Product productToDelete = _unitOfWork.Product.GetFirstOrDefault(product=>product.Id == id);
            if(productToDelete == null)
                return Json(new {success = false, message = "Error while deleting" });
            string wwwrootPath = _webHostEnvironment.WebRootPath;

            var oldimagePath = Path.Combine(wwwrootPath, productToDelete.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldimagePath))
            {
                System.IO.File.Delete(oldimagePath);
            }
            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted" });
        }
        #endregion 
    }
}
