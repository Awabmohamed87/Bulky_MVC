using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    internal class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        ApplicationDBContext _dbContext;
        public ProductImageRepository(ApplicationDBContext db) : base(db)
        {
            _dbContext = db;
        }

        public void Update(ProductImage productImage)
        {
            _dbContext.ProductImage.Update(productImage);
        }
    }
}
