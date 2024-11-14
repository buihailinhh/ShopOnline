using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.UI;
using PagedList;
using System.Net;


namespace ShopOnline.Areas.Admin.Controllers
{


    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {

        private ShopOnlineEntities db = new ShopOnlineEntities();
        // GET: Admin/User
        public ActionResult listUser(string keyword, int? page)
        {
            var users = db.AspNetUsers.AsQueryable();

            // Lọc theo từ khóa nếu có
            if (!string.IsNullOrEmpty(keyword))
            {
                users = users.Where(o => o.UserName.Contains(keyword) || o.Email.Contains(keyword));
            }

            var usersCount = users.Count();
            ViewBag.TotalCount = usersCount;

            // Sắp xếp theo Id (hoặc trường khác nếu cần)
            users = users.OrderBy(x => x.Id);

            int pageSize = 5;

            // Thiết lập số trang hiện tại
            int pageNumber = (page ?? 1);

            return View(users.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult DeleteUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser user = db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserConfirmed(string id)
        {
            AspNetUser user = db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            db.AspNetUsers.Remove(user);
            db.SaveChanges();
            return RedirectToAction("listUser", new { area = "Admin" });
        }
    }
}