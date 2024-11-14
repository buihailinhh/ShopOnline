using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using PagedList;

namespace ShopOnline.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]
    public class SaleOrderController : Controller
    {

        private ShopOnlineEntities db = new ShopOnlineEntities();
        // GET: Admin/SaleOrder
        public ActionResult ListOrder(string keyword, int? page)
        {
            var saleOrders = db.SaleOrders.AsQueryable();

            // Lọc theo từ khóa nếu có
            if (!string.IsNullOrEmpty(keyword))
            {
                saleOrders = saleOrders.Where(o => o.Code.Contains(keyword) || o.Customer_name.Contains(keyword));
            }


            // Tính tổng giá trị của các đơn hàng
            var totalPrice = saleOrders.Sum(o => o.total);

            // Tính số lượng đơn hàng
            var orderCount = saleOrders.Count();
            ViewBag.OrderCount = orderCount;
            ViewBag.TotalPrice = totalPrice;


            // Sắp xếp theo Id (hoặc trường khác nếu cần)
            saleOrders = saleOrders.OrderBy(x => x.Id);

            int pageSize = 5;

            // Thiết lập số trang hiện tại
            int pageNumber = (page ?? 1);

            return View(saleOrders.ToPagedList(pageNumber, pageSize));
        }

    }
}