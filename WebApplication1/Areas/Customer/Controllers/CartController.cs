using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace WebApplication1.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(cart => cart.UserId == userId, includeProperties: "Product"),
                OrderHeader = new()

            };
            shoppingCartVM.OrderHeader. OrderTotal = getTotal(shoppingCartVM);
            return View(shoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(cart => cart.UserId == userId, includeProperties: "Product"),
                OrderHeader = new()

            };
            shoppingCartVM.OrderHeader.OrderTotal = getTotal(shoppingCartVM);
            shoppingCartVM.OrderHeader.UserId = userId;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.User = _unitOfWork.ApplicationUser.GetFirstOrDefault(user=>user.Id == userId);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.User.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.User.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.User.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.User.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.User.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.User.PostalCode; 

            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(cart => cart.UserId == userId, includeProperties: "Product");

			shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

			shoppingCartVM.OrderHeader.OrderTotal = getTotal(shoppingCartVM);

			shoppingCartVM.OrderHeader.UserId = userId;
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(user => user.Id == userId);
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;

			}
            else
            {
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
				shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedApproved;
			}
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach(var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    OrderHeaderId = shoppingCartVM.OrderHeader.id,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Count = cart.count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                //stripe logic
                var domain = "https://localhost:7226/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.id}",
                    CancelUrl = domain + "Customer/cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};
                foreach(var item in shoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title

                            }
                        },
                        Quantity = item.count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
				var service = new SessionService();
				Session sessionResponse = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentID(shoppingCartVM.OrderHeader.id, sessionResponse.Id, sessionResponse.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location",sessionResponse.Url);
                return new StatusCodeResult(303);
			}
			return RedirectToAction(nameof(OrderConfirmation), new { id =  shoppingCartVM.OrderHeader.id});
		}
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader order = _unitOfWork.OrderHeader.GetFirstOrDefault(order => order.id == id);
            if(order.PaymentStatus != SD.PaymentStatusDelayedApproved)
            {
                //customer order
                var service = new SessionService();
                Session sessionResponse = service.Get(order.SessionId);
                if(sessionResponse.PaymentStatus.ToLower() == "paid")
                {
					_unitOfWork.OrderHeader.UpdateStripePaymentID(id, sessionResponse.Id, sessionResponse.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id,SD.StatusApproved,SD.PaymentStatusApproved);
                    _unitOfWork.Save();
				}
            }
            _unitOfWork.ShoppingCart.RemoveRange(_unitOfWork.ShoppingCart.GetAll(cart=>cart.UserId == order.UserId).ToList());
                    _unitOfWork.Save();
            return View(id);
        }
		public IActionResult Plus(int id)
        {
            ShoppingCart cartToUpdate = _unitOfWork.ShoppingCart.GetFirstOrDefault(item => item.Id == id);
            cartToUpdate.count++;
            _unitOfWork.ShoppingCart.update(cartToUpdate);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int id)
        {
            ShoppingCart cartToUpdate = _unitOfWork.ShoppingCart.GetFirstOrDefault(item => item.Id == id);
            cartToUpdate.count--;
            if(cartToUpdate.count == 0)
                _unitOfWork.ShoppingCart.Remove(cartToUpdate);
            else
                _unitOfWork.ShoppingCart.update(cartToUpdate);

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            ShoppingCart cartToUpdate = _unitOfWork.ShoppingCart.GetFirstOrDefault(item => item.Id == id);
            _unitOfWork.ShoppingCart.Remove(cartToUpdate);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        public double getTotal(ShoppingCartVM shoppingCart)
        {
            double total = 0;
            foreach(var item in shoppingCart.ShoppingCartList)
            {
                if ( item.count <= 50)
                {
                    item.Price = item.Product.Price;
                    total += item.count * item.Product.Price;
                }
                else if (item.count > 50 && item.count <= 100)
                {
                    item.Price = item.Product.Price50;
                    total += item.count * item.Product.Price50;
                }

                else
                {
                    item.Price = item.Product.Price100;
                    total += item.count * item.Product.Price100;
                }
            }
            return total;
        }
    }
}
