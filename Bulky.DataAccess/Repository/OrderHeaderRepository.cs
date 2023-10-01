using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDBContext _dbContext;
        public OrderHeaderRepository(ApplicationDBContext dBContext):base(dBContext) 
        {
            _dbContext = dBContext;
        }

        public void Update(OrderHeader orderHeader)
        {
            _dbContext.OrderHeaders.Update(orderHeader);
        }

		public void UpdateStatus(int id, string orederStatus, string? paymentStatus = null)
		{
			OrderHeader? orderToUpdate = _dbContext.OrderHeaders.FirstOrDefault(order=>order.id == id);
            if (orderToUpdate != null)
            {
                orderToUpdate.OrderStatus = orederStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                    orderToUpdate.PaymentStatus = paymentStatus;
            }
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			OrderHeader? orderToUpdate = _dbContext.OrderHeaders.FirstOrDefault(order => order.id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderToUpdate.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderToUpdate.PaymentIntentId = paymentIntentId;
                orderToUpdate.PaymentDate = DateTime.Now;
            }
		}
	}
}
