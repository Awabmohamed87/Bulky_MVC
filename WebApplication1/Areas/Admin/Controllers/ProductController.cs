using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
                    Product = new Product()
                };
            }
            else
            {
                productVM = new()
                {
                    Categories = retrievedCategories,
                    Product = _unitOfWork.Product.GetFirstOrDefault(product => product.Id == id)
                };
                productVM.Product.ProductImages = _unitOfWork.ProductImage.GetAll(image => image.ProductId == id).ToList();
                
            }
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productvm, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (productvm.Product.Id == 0)
                    _unitOfWork.Product.Add(productvm.Product);
                else
                    _unitOfWork.Product.Update(productvm.Product);
                _unitOfWork.Save();

                if(files != null)
                {
                    string wwwrootPath = _webHostEnvironment.WebRootPath;
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\Product-" + productvm.Product.Id;
                        string finalPath = Path.Combine(wwwrootPath, productPath);
                       
                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        
                        if(productvm.Product.ProductImages == null)
                            productvm.Product.ProductImages = new List<ProductImage>();

                        ProductImage productImage = new ProductImage { ImageUrl = @"\" + productPath + @"\" + fileName, ProductId = productvm.Product.Id };
                        productvm.Product.ProductImages.Add(productImage);
                        _unitOfWork.ProductImage.Add(productImage);
                    }
                }
                
                _unitOfWork.Save();
                
                TempData["success"] = "Product successfully added";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productvm = new()
                {
                    Categories = _unitOfWork.Category.GetAll().Select(category => new SelectListItem(category.Name, category.DisplayOrder.ToString())),
                    Product = new Product()
                };
                return View(productvm);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            ProductImage imageToDelete = _unitOfWork.ProductImage.GetFirstOrDefault(image => image.Id == imageId);
            int productId = imageToDelete.ProductId;

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToDelete.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.ProductImage.Remove(imageToDelete);
            _unitOfWork.Save();
            TempData["success"] = "Deleted";
            return RedirectToAction(nameof(Upsert), new {id = productId });
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

            if(productToDelete.ProductImages != null)
            {
                string productPath = @"images\products\Product-" + id;
                string finalPath = Path.Combine(wwwrootPath, productPath);

                if (!Directory.Exists(finalPath))
                {
                    string[] files = Directory.GetFiles(finalPath);
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                    Directory.Delete(finalPath);
                }
            }
            
            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted" });
        }
        #endregion 
    }
}
