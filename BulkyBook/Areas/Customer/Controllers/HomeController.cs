using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using BulkyBook.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModel;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> ProductList = _unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim!=null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, Count);

            }
            return View(ProductList);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            //First find the product from the Product database
            var ProductFromDb =
                _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, IncludeProperties: "Category,CoverType");

            ShoppingCart shoppingCart = new ShoppingCart()
            {
                Product = ProductFromDb,
                ProductId = ProductFromDb.Id
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cartObj)
        {
            cartObj.Id = 0;
            if (ModelState.IsValid)
            {
                //will add to cart
                var claimsIdentity = (ClaimsIdentity) User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cartObj.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == cartObj.ApplicationUserId && u.ProductId == cartObj.ProductId,
                    IncludeProperties: "Product");


                if (cartFromDb == null)
                {
                    //no record exist in database for that product for the user

                    _unitOfWork.ShoppingCart.Add(cartObj);
                }
                else
                {
                    cartFromDb.Count += cartObj.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();


                var Count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == cartObj.ApplicationUserId)
                    .ToList().Count();

                //var Count = _db.ShoppingCarts.Where(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).ToList()
                //    .Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart,Count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var ProductFromDb =
                    _unitOfWork.Product.GetFirstOrDefault(u => u.Id == cartObj.ProductId, IncludeProperties: "Category,CoverType");
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    Product = ProductFromDb,
                    ProductId = ProductFromDb.Id
                };

                return View(shoppingCart);
            }
            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
