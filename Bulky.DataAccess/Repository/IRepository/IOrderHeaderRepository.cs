using Bulky.DataAccess.Repository.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, string orederStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id,string sessionId, string paymentIntentId);
    }
}
