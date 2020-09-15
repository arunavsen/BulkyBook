using System;
using System.Collections.Generic;
using System.Text;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IShoppingCartRepository ShoppingCart { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IOrderHeaderRepository OrderHeader { get; }
        ICategoryRepository Category { get; }
        ICompanyRepository Company { get; }
        ICoverTypeRepository CoverType { get; }
        IProductRepository Product { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ISP_Call SP_Call { get; }

        void Save();
    }
}
