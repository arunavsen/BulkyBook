using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModel;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderDetailsVM OrderDetailsVm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderDetailsVm=new OrderDetailsVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id==id,IncludeProperties:"ApplicationUser"),
                OrderDetailses = _unitOfWork.OrderDetails.GetAll(o=>o.OrderId==id,IncludeProperties:"Product")
            };
            return View(OrderDetailsVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader =
                _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsVm.OrderHeader.Id,
                    IncludeProperties: "ApplicationUser");
            if (stripeToken!=null)
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.BalanceTransactionId == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.BalanceTransactionId;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }
                _unitOfWork.Save();
            }
            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsVm.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderDetailsVm.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailsVm.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate=DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }


        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PaymentStatus==SD.StatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal*100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };
                var service = new RefundService();
                //Refund refund = service.Create(options);

                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        #region API Calls

        [HttpGet]
        public IActionResult GetAllOrder(string status)
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeadersList;
            if (User.IsInRole(SD.Role_Admin)||User.IsInRole(SD.Role_Employee))
            {
                orderHeadersList = _unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            }
            else
            {
                orderHeadersList = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId==claim.Value, IncludeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeadersList = orderHeadersList.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeadersList = orderHeadersList.Where(o => o.OrderStatus == SD.StatusApproved ||
                                                                   o.OrderStatus == SD.StatusInProcess ||
                                                                   o.OrderStatus == SD.StatusPending);
                    break;
                case "completed":
                    orderHeadersList = orderHeadersList.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                case "rejected":
                    orderHeadersList = orderHeadersList.Where(o => o.OrderStatus == SD.StatusCancelled ||
                                                                   o.OrderStatus == SD.StatusRefunded ||
                                                                   o.OrderStatus == SD.PaymentStatusRejected);
                    break;
                default:
                    break;

            }
            return Json(new{Data=orderHeadersList});
        }

        #endregion
    }
}