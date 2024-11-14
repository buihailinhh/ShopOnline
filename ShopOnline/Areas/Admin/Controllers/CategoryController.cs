using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using PagedList;
using PagedList.Mvc;




namespace ShopOnline.Areas.Admin.Controllers
{


    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        // GET: Admin/Category

        private ShopOnlineEntities db = new ShopOnlineEntities();


          
        public ActionResult ListCategory(string keyword, int? categoryId, int? page)
        {
            // Lấy tất cả danh mục và sắp xếp theo tên
            var categories = db.Categories.AsQueryable();

            // Áp dụng tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                categories = categories.Where(p => p.Name.Contains(keyword));
            }

            // Lọc theo category nếu có
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                categories = categories.Where(p => p.Id == categoryId.Value);
            }

            // Lấy số lượng tổng loại hàng sau khi đã áp dụng tìm kiếm
            var categoryCount = categories.Count();
            ViewBag.CategoryCount = categoryCount;

            // Sắp xếp theo Id (hoặc trường khác nếu cần)
            categories = categories.OrderBy(x => x.Id);

            // Thiết lập kích thước trang (pageSize)
            int pageSize = 5;

            // Thiết lập số trang hiện tại
            int pageNumber = (page ?? 1);

            // Trả về các danh mục được phân trang
            return View(categories.ToPagedList(pageNumber, pageSize));
        }



        public ActionResult AddCategory()
        {
            var users = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            })
                .ToList();

            ViewBag.AspNetUsers = users;

            return View();
        }

        [HttpPost]
        public ActionResult AddCategory([Bind(Include = "Name, Create_by, Update_by, Create_date, Update_date, Description, Status")] Category categories)
        {
            // Xử lý dữ liệu từ form
            if (ModelState.IsValid)
            {

                db.Categories.Add(categories);
                db.SaveChanges();
                return RedirectToAction("listCategory", new {are="Admin"});
                            }

            ViewBag.AspNetUsers = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            }).ToList();

            return View(categories);
        }

        public ActionResult EditCategory(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if(category == null)
            {
                return HttpNotFound();

            }
            var users = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            })
                .ToList();

            ViewBag.AspNetUsers = users;



            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Tìm bản ghi hiện tại từ cơ sở dữ liệu
                var existingCategory = db.Categories.Find(category.Id);
                if (existingCategory != null)
                {
                   
                    existingCategory.Name = category.Name;
                    existingCategory.Create_by = category.Create_by; 
                    existingCategory.Update_by = category.Update_by;
                    existingCategory.Update_date = DateTime.Now; 
                    existingCategory.Description = category.Description;

                    // Không thay đổi Create_date
                    // existingCategory.Create_date = existingCategory.Create_date;

                    db.Entry(existingCategory).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("listCategory");
                }
            }

            ViewBag.AspNetUsers = db.AspNetUsers.Select(u => new SelectListItem
            {
                Value = u.Email,
                Text = u.Email
            }).ToList();
            return View(category);
        }

        public ActionResult DeleteCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategoryConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("listCategory", new { area = "Admin" });
        }






    }
}