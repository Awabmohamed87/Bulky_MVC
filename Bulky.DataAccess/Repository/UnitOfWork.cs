using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        ApplicationDBContext _dBContext;
        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IProductImageRepository ProductImage { get; private set; }

        public UnitOfWork(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
            Category = new CategoryRepository(dBContext);
            Product = new ProductRepository(dBContext);
            Company = new CompanyRepository(dBContext);
            ShoppingCart = new ShoppingCartRepository(dBContext);
            ApplicationUser = new ApplicationUserRepository(dBContext);
            OrderDetail = new OrderDetailRepository(dBContext);
            OrderHeader = new OrderHeaderRepository(dBContext);
            ProductImage = new ProductImageRepository(dBContext);
        }

        public void Save()
        {
            _dBContext.SaveChanges();
        }
    }
}
