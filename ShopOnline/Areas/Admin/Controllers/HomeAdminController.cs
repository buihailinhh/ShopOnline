using Microsoft.AspNet.Identity;
using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ShopOnline.Areas.Admin.Controllers
{
    public class HomeAdminController : Controller
    {
        private ShopOnlineEntities db = new ShopOnlineEntities();
        // GET: Admin/HomeAdmin


        public class IncomeStatistic
{
    public int Day { get; set; }
    public decimal TotalAmount { get; set; }
}


        [Authorize(Roles = "Admin")]
        public ActionResult HomeAdmin(int? selectedMonth, int? selectedYear)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.GetUserName();
                ViewBag.UserName = userName;
            }
            else
            {
                ViewBag.UserName = "Guest";
            }

            var categiesCount = db.Categories.Count();
            ViewBag.CategoriesCount = categiesCount;

            var productCount = db.Products.Count();
            ViewBag.ProductsCount = productCount;

            try
            {
                var customerCount = db.Customers.Count();
                ViewBag.CustomersCount = customerCount;
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException?.Message ?? ex.Message;
                // Ghi log hoặc hiển thị lỗi
                Console.WriteLine(detailedError);
            }

            // Xác định tháng và năm hiện tại nếu không được chọn
            var month = selectedMonth ?? DateTime.Now.Month;
            var year = selectedYear ?? DateTime.Now.Year;

            // Lọc doanh thu theo tháng và năm
            var salesData = db.SaleOrders
                .Where(order => order.Create_date.HasValue
                    && order.Create_date.Value.Month == month
                    && order.Create_date.Value.Year == year)
                .GroupBy(order => order.Create_date.Value.Day)
                .Select(group => new
                {
                    Day = group.Key,
                    Total = group.Sum(order => order.total)
                })
                .ToList();

            // Tạo danh sách các ngày trong tháng
            var daysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(day => new
                {
                    Day = day,
                    Total = salesData.FirstOrDefault(s => s.Day == day)?.Total ?? 0
                })
                .ToList();

            ViewBag.SalesData = daysInMonth;

            // Gửi thông tin tháng và năm đến view để hiển thị
            ViewBag.Month = month;
            ViewBag.Year = year;


            return View();
        }






    }
}
