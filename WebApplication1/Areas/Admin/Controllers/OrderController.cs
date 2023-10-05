using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace WebApplication1.Areas.Admin.Controllers
{
	[Area("Admin")]

    [Authorize]
    public class OrderController : Controller
	{
		IUnitOfWork _unitOfWork {  get; set; }
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork UnitOfWork)
        {
            _unitOfWork = UnitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(header => header.id == orderId, includeProperties:"User"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(detail=>detail.OrderHeaderId == orderId, includeProperties:"Product")
            };
            return View(OrderVM);
        }
        [HttpPost] 
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader OrderToUpdate = _unitOfWork.OrderHeader.GetFirstOrDefault(header=>header.id == OrderVM.OrderHeader.id);
            OrderToUpdate.Name = OrderVM.OrderHeader.Name;
            OrderToUpdate.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            OrderToUpdate.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            OrderToUpdate.City = OrderVM.OrderHeader.City;
            OrderToUpdate.State = OrderVM.OrderHeader.State;
            OrderToUpdate.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier)) {
            OrderToUpdate.PostalCode = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber)) {
            OrderToUpdate.PostalCode = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(OrderToUpdate);
            _unitOfWork.Save();

            TempData["success"] = "Updated";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing() 
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "updated";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.id});
        }
        public IActionResult ShipOrder()
        {
            OrderHeader OrderToUpdate = _unitOfWork.OrderHeader.GetFirstOrDefault(header => header.id == OrderVM.OrderHeader.id);
            OrderToUpdate.Carrier = OrderVM.OrderHeader.Carrier;
            OrderToUpdate.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            OrderToUpdate.OrderDate = DateTime.Now;
            OrderToUpdate.OrderStatus = SD.StatusShipped;
            if(OrderToUpdate.PaymentStatus == SD.PaymentStatusDelayedApproved)
            {
                OrderToUpdate.PaymentDueDate = DateOnly.FromDateTime( DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(OrderToUpdate);
            _unitOfWork.Save();
            TempData["success"] = "updated";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.id });
        }
        public IActionResult CancelOrder()
        {
            OrderHeader OrderToCancel = _unitOfWork.OrderHeader.GetFirstOrDefault(header => header.id == OrderVM.OrderHeader.id);
            if(OrderToCancel.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = OrderToCancel.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(OrderToCancel.id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderToCancel.id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Cancelled";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.id });
        }
        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_pay_Now()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(header => header.id == OrderVM.OrderHeader.id, includeProperties:"User");
            OrderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(detail => detail.OrderHeaderId == OrderVM.OrderHeader.id, includeProperties: "Product");
            //stripe logic
            var domain = "https://localhost:7226/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in OrderVM.OrderDetails)
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
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session sessionResponse = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.id, sessionResponse.Id, sessionResponse.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", sessionResponse.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader order = _unitOfWork.OrderHeader.GetFirstOrDefault(order => order.id == orderHeaderId);
            if (order.PaymentStatus == SD.PaymentStatusDelayedApproved)
            {
                //customer order
                var service = new SessionService();
                Session sessionResponse = service.Get(order.SessionId);
                if (sessionResponse.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, sessionResponse.Id, sessionResponse.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, order.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            _unitOfWork.ShoppingCart.RemoveRange(_unitOfWork.ShoppingCart.GetAll(cart => cart.UserId == order.UserId).ToList());
            _unitOfWork.Save();
            return View(orderHeaderId);
        }


        #region API Calls
        [HttpGet]
		public IActionResult GetAll(string status)
		{
			List<OrderHeader> retrievedOrders ;
            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                retrievedOrders = _unitOfWork.OrderHeader.GetAll(includeProperties: "User").ToList();
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var id = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                retrievedOrders = _unitOfWork.OrderHeader.GetAll(header => header.UserId == id, includeProperties: "User").ToList();
            }
            switch (status)
            {
                case "pending":
                    retrievedOrders = retrievedOrders.Where(order => order.PaymentStatus == SD.PaymentStatusDelayedApproved).ToList(); break;
                case "inprocess":
                    retrievedOrders = retrievedOrders.Where(order => order.OrderStatus == SD.StatusInProcess).ToList(); break;
                case "completed":
                    retrievedOrders = retrievedOrders.Where(order => order.OrderStatus == SD.StatusShipped).ToList(); break;
                case "approved":
                    retrievedOrders = retrievedOrders.Where(order => order.OrderStatus == SD.StatusApproved).ToList(); break;
                case "all":
                     break;
            }
            return Json(new { data = retrievedOrders });
		}
			
		#endregion
	}
}
