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
    internal class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository 
    {
        ApplicationDBContext _dbContext;
        public ShoppingCartRepository(ApplicationDBContext db) : base(db)
        {
            _dbContext = db;
        }

        public void update(ShoppingCart shoppingCart)
        {
            _dbContext.ShoppingCart.Update(shoppingCart);
        }
    }
}
