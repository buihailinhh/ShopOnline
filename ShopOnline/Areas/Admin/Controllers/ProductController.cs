using ShopOnline.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ShopOnline.Areas.Admin.Data;
using PagedList;
using X.PagedList;
using System.Web.Razor.Tokenizer.Symbols;


namespace ShopOnline.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private ShopOnlineEntities db = new ShopOnlineEntities();


      public ActionResult ListProduct(string keyword, int? categoryId, int? page)
{
            // Truy vấn dữ liệu từ database
            var query = from a in db.Products
                        join b in db.Categories on a.Category_id equals b.Id
                        join sold in db.SaleOrderProducts on a.Id equals sold.Product_Id into soldGroup
                        select new ProductCategory
                        {
                            Id = a.Id,
                            Name = a.Name,
                            CategoryName = b.Name,
                            CategoryDescription = b.Description,
                            Price = a.Price,
                            Sale_price = a.Sale_price,
                            Avatar = a.Avatar,
                            Create_date = a.Create_date,
                            Update_date = a.Update_date,
                            Short_description = a.Short_description,
                            TotalQuantity = a.TotalQuantity,
                            SoldQuantity = soldGroup.Sum(s => (int?)s.Quatity) ?? 0, // Tổng số lượng đã bán
                          
                        };

          



            // Áp dụng tìm kiếm theo từ khóa nếu có
            if (!string.IsNullOrEmpty(keyword))
    {
        query = query.Where(p => p.Name.Contains(keyword));
    }

    // Lọc theo loại hàng nếu có
    if (categoryId.HasValue && categoryId.Value > 0)
    {
        query = query.Where(p => p.CategoryName == db.Categories.FirstOrDefault(c => c.Id == categoryId.Value).Name);
    }

    // Lấy danh sách các Category để hiển thị trong dropdown list
    var categories = db.Categories.Select(c => new SelectListItem
    {
        Value = c.Id.ToString(),
        Text = c.Name
    }).ToList();

    // Thêm lựa chọn mặc định
    categories.Insert(0, new SelectListItem { Value = "0", Text = "Lựa chọn theo loại hàng" });

    // Đưa danh sách này vào ViewBag
    ViewBag.Categories = categories;

    // Sắp xếp dữ liệu trước khi phân trang
    var sortedQuery = query.OrderBy(p => p.Id); // Hoặc OrderByDescending nếu bạn muốn sắp xếp theo thứ tự giảm dần

    // Lấy số lượng tổng loại hàng sau khi đã áp dụng tìm kiếm
    var productCount = sortedQuery.Count();
    ViewBag.ProductCount = productCount;

    // Thiết lập kích thước trang (pageSize)
    int pageSize = 5;

    // Thiết lập số trang hiện tại
    int pageNumber = (page ?? 1);

    // Trả về view với dữ liệu phân trang
    return View(sortedQuery.ToPagedList(pageNumber, pageSize));
}




        public ActionResult AddProduct()
        {
            var categories = db.Categories.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Name
            }).ToList();
            ViewBag.Categories = categories;

            var users = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList();
            ViewBag.AspNetUsers = users;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(Product product, HttpPostedFileBase productAvatar, HttpPostedFileBase[] productImages)
        {
            if (ModelState.IsValid)
            {
                // Xử lý tệp Avatar
                if (productAvatar != null && productAvatar.ContentLength > 0)
                {
                    try
                    {
                        var avatarFileName = Path.GetFileName(productAvatar.FileName);
                        var uploadDir = Server.MapPath("~/UploadFiles/");
                        var avatarPath = Path.Combine(uploadDir, avatarFileName);

                        // Tạo thư mục nếu chưa tồn tại
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        // Ghi tệp vào thư mục
                        productAvatar.SaveAs(avatarPath);

                        // Cập nhật đường dẫn vào cơ sở dữ liệu
                        product.Avatar = "/UploadFiles/" + avatarFileName;
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi
                        System.Diagnostics.Debug.WriteLine("Lỗi khi lưu tệp: " + ex.Message);
                    }
                }

                // Lưu thông tin sản phẩm vào cơ sở dữ liệu
                db.Products.Add(product);
                db.SaveChanges();

                // Xử lý tệp Images sau khi sản phẩm đã được lưu và Id được gán
                if (productImages != null && productImages.Length > 0)
                {
                    foreach (var image in productImages)
                    {
                        if (image != null && image.ContentLength > 0)
                        {
                            var imageFileName = Path.GetFileName(image.FileName);
                            var imagePath = Path.Combine(Server.MapPath("~/UploadFiles/"), imageFileName);

                            // Tạo thư mục nếu chưa tồn tại
                            if (!Directory.Exists(Server.MapPath("~/UploadFiles/")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/UploadFiles/"));
                            }

                            image.SaveAs(imagePath);

                            // Lưu đường dẫn tệp vào bảng ProductImage
                            var productImage = new ProductImage
                            {
                                Path = "/UploadFiles/" + imageFileName,
                                Product_id = product.Id // Sử dụng Id của sản phẩm vừa lưu
                            };

                            db.ProductImages.Add(productImage);
                        }
                    }

                    db.SaveChanges(); // Lưu các đối tượng ProductImage vào cơ sở dữ liệu
                }

                return RedirectToAction("listProduct");
            }

            // Nếu ModelState không hợp lệ, tải lại các danh sách Category và User
            var categories = db.Categories.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Name
            }).ToList();
            ViewBag.Categories = categories;

            var users = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.UserName
            }).ToList();
            ViewBag.AspNetUsers = users;

            return View(product);
        }

        public ActionResult EditProduct(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách tất cả các category
            var categories = db.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            // Thiết lập ViewBag cho categories
            ViewBag.Categories = new SelectList(categories, "Value", "Text", product.Category_id);

            // Lấy danh sách users
            var users = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            }).ToList();

            ViewBag.AspNetUsers = users;

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                // Tìm bản ghi hiện tại từ cơ sở dữ liệu
                var existingProduct = db.Products.Find(product.Id);
                if (existingProduct != null)
                {
                    // Cập nhật các thuộc tính
                    existingProduct.Name = product.Name;
                    existingProduct.Create_by = product.Create_by;
                    existingProduct.Update_by = product.Update_by;
                    existingProduct.Update_date = DateTime.Now; // Cập nhật Update_date
                    existingProduct.Short_description = product.Short_description;
                    existingProduct.Price = product.Price;
                    existingProduct.Sale_price = product.Sale_price;

                    // Đảm bảo Create_date không bị thay đổi
                    db.Entry(existingProduct).Property(p => p.Create_date).IsModified = false;

                    db.SaveChanges();
                    return RedirectToAction("listProduct");
                }
            }

            // Lấy danh sách người dùng cho ViewBag nếu ModelState không hợp lệ
            ViewBag.AspNetUsers = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            }).ToList();

            return View(product);
        }


        public ActionResult DeleteProduct(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            }
            Product products = db.Products.Find(id);
            if( products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductConfirmed(int id)
        {

            Product products = db.Products.Find(id);
            if(products == null)
            {
                return HttpNotFound();
            }
            db.Products.Remove(products);
            db.SaveChanges();
            return RedirectToAction("listProduct", new { area = "Admin" });

        }
    }
}
