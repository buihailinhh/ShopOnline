using ShopOnline.Areas.Admin.Data;
using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using X.PagedList;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Text;
using System.Security.Policy;




namespace ShopOnline.Controllers
{
    public class HomeController : Controller
    {
        private ShopOnlineEntities db = new ShopOnlineEntities();
        public ActionResult Index(string keyword, int? categoryId, string price, int page = 1, int pageSize = 6)
        {
            var products = db.Products.AsQueryable();

            // Lọc theo từ khóa (keyword)
            if (!string.IsNullOrEmpty(keyword))
            {
                products = products.Where(p => p.Name.Contains(keyword));
            }

            // Lọc theo loại hàng (categoryId)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.Category_id == categoryId.Value);
            }

            // Lọc theo mức giá (price)
            if (!string.IsNullOrEmpty(price))
            {
                switch (price)
                {
                    case "0-1":
                        products = products.Where(p => p.Sale_price < 1000000);
                        break;
                    case "1-5":
                        products = products.Where(p => p.Sale_price >= 1000000 && p.Sale_price <= 5000000);
                        break;
                    case "5-10":
                        products = products.Where(p => p.Sale_price >= 5000000 && p.Sale_price <= 10000000);
                        break;
                    case "10-20":
                        products = products.Where(p => p.Sale_price >= 10000000 && p.Sale_price <= 20000000);
                        break;
                    case "20-9999":
                        products = products.Where(p => p.Sale_price > 20000000);
                        break;
                }
            }

            // Lấy danh sách các loại hàng (categories) để hiển thị trong dropdown
            ViewBag.Categories = db.Categories.ToList();

            // Sắp xếp và phân trang
            products = products.OrderBy(p => p.Id);

            return View(products.ToPagedList(page, pageSize));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }




        public ActionResult PhuKien()
        {

            var phuKiens = db.PhuKiens.ToList();

            
            return View(phuKiens);
          
        }


        public ActionResult DichVuBaoHanh()
        {

            return View();
        }



        [HttpPost]
        public ActionResult DichVuBaoHanh(string customerName, string email, string phoneNumber, int? SaleOrderId)
        {
            // Kiểm tra thông tin khách hàng không trống
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phoneNumber))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên khách hàng, email và số điện thoại.");
                return View("DichVuBaoHanh"); // Trả về form ban đầu nếu thông tin không hợp lệ
            }

            // Tìm kiếm đơn hàng dựa trên tên khách hàng, email và số điện thoại
            var orders = db.SaleOrders
      .Where(o => o.Customer_name == customerName && o.Customer_email == email && o.Customer_mobile == phoneNumber)
      .ToList();

            // Kiểm tra orders có phải là null hoặc không có đơn hàng
            if (orders == null || !orders.Any())
            {
                return Json(new { message = "Không tìm thấy đơn hàng với thông tin bạn vừa nhập.", redirect = false });
            }

            // Lấy danh sách sản phẩm từ các đơn hàng, kiểm tra SaleOrderProducts có phải là null
            var orderIds = orders.Select(o => o.Id).ToList(); // Lấy danh sách các Id của các đơn hàng

            var products = db.SaleOrderProducts
                .Where(x => orderIds.Contains(x.Sale0rder_Id.Value)) // Lọc sản phẩm theo danh sách Id của đơn hàng
                .ToList();


            if (products == null || !products.Any())
            {
                TempData["Message"] = "Không tìm thấy sản phẩm nào với thông tin đã nhập.";
                return RedirectToAction("DichVuBaoHanh"); // Trả về form ban đầu nếu không tìm thấy sản phẩm
            }

            // Chuyển danh sách sản phẩm vào ViewBag
            ViewBag.Products = products;
            // Trả về view hiển thị danh sách sản phẩm
            return RedirectToAction("SanPhamDaMua");
        }



        [HttpPost] 
          public ActionResult SanPhamDaMua(string customerName, string email, string phoneNumber)
          {
             
              // Tìm kiếm đơn hàng dựa trên tên khách hàng, email và số điện thoại
              var orders = db.SaleOrders
        .Where(o => o.Customer_name == customerName && o.Customer_email == email && o.Customer_mobile == phoneNumber)
        .ToList();

              // Kiểm tra orders có phải là null hoặc không có đơn hàng
              if (orders == null || !orders.Any())
              {
                  return Json(new { message = "Không tìm thấy đơn hàng với thông tin bạn vừa nhập.", redirect = false });
              }

              // Lấy danh sách sản phẩm từ các đơn hàng, kiểm tra SaleOrderProducts có phải là null
              var orderIds = orders.Select(o => o.Id).ToList(); // Lấy danh sách các Id của các đơn hàng

              var products = db.SaleOrderProducts
                  .Where(x => orderIds.Contains(x.Sale0rder_Id.Value)) // Lọc sản phẩm theo danh sách Id của đơn hàng
                  .ToList();


              if (products == null || !products.Any())
              {
                  TempData["Message"] = "Không tìm thấy sản phẩm nào với thông tin đã nhập.";
                  return RedirectToAction("DichVuBaoHanh"); // Trả về form ban đầu nếu không tìm thấy sản phẩm
              }

              // Chuyển danh sách sản phẩm vào ViewBag
              ViewBag.Products = products;

              return View();
          }





        [HttpPost]
        public JsonResult KiemTraBaoHanh(int id)
        {
            var product = db.SaleOrderProducts.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return Json(new { message = "Không tìm thấy sản phẩm.", redirect = false });
            }

            var warrantyPeriod = new TimeSpan(365, 0, 0, 0); // 1 năm
            var warrantyEndDate = product.Create_date?.Add(warrantyPeriod);

            if (!product.Create_date.HasValue || (warrantyEndDate.HasValue && DateTime.Now > warrantyEndDate.Value))
            {
                return Json(new { message = "Sản phẩm đã hết thời gian bảo hành.", redirect = false });
            }

            // Nếu sản phẩm còn thời gian bảo hành, chuyển hướng đến trang XacNhanBaoHanh
            return Json(new { message = "Sản phẩm còn thời gian bảo hành.", redirect = true, redirectUrl = Url.Action("XacNhanBaoHanh", "Home", new { Product_Id = product.Id }) });
        }


        public ActionResult XacNhanBaoHanh(int Product_Id)
        {
            var product = db.SaleOrderProducts.FirstOrDefault(p => p.Id == Product_Id);

            if (product == null)
            {
                return HttpNotFound();
            }

            var model = new BaoHanh
            {
                Product_Id = product.Id // Chuẩn bị dữ liệu cho form
            };

            return View(); // Trả về view để người dùng điền thông tin
        }


        [HttpPost]

        public ActionResult XacNhanBaoHanh(BaoHanh model)
        {
            try
            {
                // Kiểm tra lại sản phẩm trong cơ sở dữ liệu

                var product = db.SaleOrderProducts.FirstOrDefault(p => p.Id == model.Product_Id);

                if (product == null)
                {
                    return Json(new { message = "Không tìm thấy sản phẩm.", redirect = false });
                }

                // Lưu thông tin bảo hành vào bảng BaoHanh
                var infor = new BaoHanh()
                {
                    Product_Id = product.Id,
                    Product_name = product.Name,
                    Recipient_phoneNumber = model.Recipient_phoneNumber,
                    Recipient_address = model.Recipient_address,
                    Error_Details = model.Error_Details,
                    Warranty_date = DateTime.Now,
                    SaleOrderId = product.Sale0rder_Id,
                };

                db.BaoHanhs.Add(infor);
                db.SaveChanges();
                var order = db.SaleOrders.FirstOrDefault(a => a.Id == model.SaleOrderId);
                TempData["Message"] = "Thông tin bảo hành đã được lưu. Chúng tôi sẽ liên lạc lại cho bạn sớm nhất cho bạn qua Email hoặc Số điện thoại";
                return View("SanPhamDaMua" /*new { SaleOrderId = product.Sale0rder_Id }*/);
                // Chuyển hướng đến trang sản phẩm đã mua
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { message = "Đã xảy ra lỗi khi lưu thông tin bảo hành: " + ex.Message, redirect = false });
            }
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();


        }

        public ActionResult ContactConFirm(string last_name, string phone, string note)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Index", "Customers");
            }
            var existingCustomer = db.Customers.FirstOrDefault(c => c.phone == phone);
            if (existingCustomer != null)
            {
                // Nếu số điện thoại đã tồn tại, bạn có thể thực hiện một hành động phù hợp ở đây,
                // như thông báo lỗi cho người dùng hoặc chuyển hướng họ đến trang khác.
                // Ở đây, tôi giả sử bạn muốn chuyển hướng người dùng đến trang chủ.
                return RedirectToAction("Index", "Home");
            }
            var cus = new Customer()
            {
                last_name = last_name,
                phone = phone,
                note = note,
                create_date = DateTime.Now
            };

            db.Customers.Add(cus);
            db.SaveChanges();
            StringBuilder Body = new StringBuilder();
            Body.Append("<p>Đăng ký ngay</p>");
            Body.Append("<table>");
            Body.Append("<tr><td colspan='2'><h4>Thông tin khách hàng</h4></td></tr>");
            Body.Append("<tr><td>Họ và tên:</td><td>" + last_name + "</td></tr>");
            Body.Append("<tr><td>Điện thoại:</td><td>" + phone + "</td></tr>");
            Body.Append("<tr><td>Ghi chú:</td><td>" + note + "</td></tr>");
            Body.Append("</table>");

            // Replace with your SMTP server details
            string smtpServer = "smtp.gmail.com"; // Example using Gmail
            int port = 587;
            string username = "buihailinh0604@gmail.com";
            string password = "ckfi usvx oebs savx";

            // Create the email message
            MailMessage message = new MailMessage();
            message.From = new MailAddress(username);
            //   message.To.Add("nguyenthemanh201186@gmail.com");
            //  message.To.Add("ngaptk@bluesea.vn");
            // message.To.Add("thaihq@bluesea.vn");
            message.To.Add("paychplayfuns@gmail.com");

            message.Subject = "Phản hồi từ khách hàng - DodadungHL.vn";
            message.Body = Body.ToString();
            message.IsBodyHtml = true;
            // Configure the SMTP client
            SmtpClient client = new SmtpClient(smtpServer, port);
            client.EnableSsl = true; // Enable SSL for secure communication
            client.Credentials = new NetworkCredential(username, password);

            try
            {
                // Send the email
                client.Send(message);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {


                Console.WriteLine("Error sending email: " + ex.Message);
            }

            return RedirectToAction("Index", "Home");
        }










        public ActionResult Detail(int id)
        {
            // Lấy sản phẩm với ID cụ thể
            var product = db.Products
                            .Where(a => a.Id == id)
                            .Select(a => new ProductDetail
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Price = a.Price,
                                Sale_price = a.Sale_price,
                               
                                Images = db.ProductImages.Where(x => x.Product_id == a.Id).ToList()
                            })
                            .FirstOrDefault(); // Lấy sản phẩm đầu tiên (và duy nhất)

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product); // Trả về View với model là sản phẩm
        }

        [HttpPost]
        public JsonResult AddToCart(int productId, int quantity)
        {
            // Giả sử bạn đang sử dụng Session để lưu giỏ hàng
            var cart = Session["Cart"] as List<CartItem>;

            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var cartItem = cart.FirstOrDefault(c => c.Product.Id == productId);
            if (cartItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ hàng thì tăng số lượng
                cartItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có thì thêm sản phẩm mới vào giỏ hàng
                var product = db.Products.Find(productId);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        Product = product,
                        Quantity = quantity
                    });
                }
            }

            // Cập nhật lại giỏ hàng trong session
            Session["Cart"] = cart;

            // Trả về kết quả JSON với số lượng sản phẩm trong giỏ hàng
            return Json(new { totalCartProducts = cart.Sum(c => c.Quantity) });
        }
    }
}