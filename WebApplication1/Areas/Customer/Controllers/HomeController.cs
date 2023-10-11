using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork  = unitOfWork;
        }

        public IActionResult Index()
        {
            
            IEnumerable<Product> productsList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").ToList();
            return View(productsList);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            ShoppingCart shoppingCart = new() { 
            count = 1,
            Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties: "Category,ProductImages"),
            ProductId = id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.UserId = userId;
            shoppingCart.Id = 0;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(cart=>cart.UserId == userId && cart.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.count += shoppingCart.count;
                _unitOfWork.ShoppingCart.update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                int cartSize = _unitOfWork.ShoppingCart.GetAll(cart => cart.UserId == userId).Count();
                HttpContext.Session.SetInt32(SD.SessionCart, cartSize);
            }
            _unitOfWork.Save();
            TempData["success"] = "Added to cart";
            return RedirectToAction("Index","Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
