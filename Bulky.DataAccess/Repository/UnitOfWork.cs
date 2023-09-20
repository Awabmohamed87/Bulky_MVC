﻿using Bulky.DataAccess.Data;
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
        public UnitOfWork(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
            Category = new CategoryRepository(dBContext);
        }

        public void Save()
        {
            _dBContext.SaveChanges();
        }
    }
}
