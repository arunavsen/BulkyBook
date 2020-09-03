using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(s => s.Id == product.Id);
            if (objFromDb != null)
            {
                if (objFromDb.ImageURL != null)
                {
                    objFromDb.ImageURL = product.ImageURL;
                }

                objFromDb.ISBN = product.ISBN;
                objFromDb.Price = product.Price;
                objFromDb.Price50 = product.Price50;
                objFromDb.Price100 = product.Price100;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Title = product.Title;
                objFromDb.Description = product.Description;
                objFromDb.CategoryId = product.CategoryId;
                objFromDb.Author = product.Author;
                objFromDb.CoverTypeId = product.CoverTypeId;

                _db.SaveChanges();
            }
        }
    }
}
