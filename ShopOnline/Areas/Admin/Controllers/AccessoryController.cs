using PagedList;
using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopOnline.Areas.Admin.Controllers
{
    public class AccessoryController : Controller
    {
        private ShopOnlineEntities db = new ShopOnlineEntities();
        // GET: Admin/Accessory
        public ActionResult listAccessory(string keyword, int? page)
        {

            var accessory = db.PhuKiens.AsQueryable();

            // Lọc theo từ khóa nếu có
            if (!string.IsNullOrEmpty(keyword))
            {
                accessory = accessory.Where(o => o.Name.Contains(keyword));
            }





            var accessoryCount = accessory.Count();
            ViewBag.OrderCount = accessoryCount;



            // Sắp xếp theo Id (hoặc trường khác nếu cần)
            accessory = accessory.OrderBy(x => x.Id);

            int pageSize = 5;

            // Thiết lập số trang hiện tại
            int pageNumber = (page ?? 1);



            return View(accessory.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult addAccessory()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult addAccessory(PhuKien phukien, HttpPostedFileBase productAvatar)
        {
            if (ModelState.IsValid)
            {
                if (productAvatar != null && productAvatar.ContentLength > 0)
                {
                    try
                    {
                        var avatarFileName = Path.GetFileName(productAvatar.FileName);
                        var uploadDir = Server.MapPath("~/UploadFiles/PhuKien/");
                        var avatarPath = Path.Combine(uploadDir, avatarFileName);

                        // tạo thư  mục nếu chưa tồn tại
                        if (!Directory.Exists(uploadDir))
                        {

                            Directory.CreateDirectory(uploadDir);
                        }
                        // ghi tệp vào thư mục
                        productAvatar.SaveAs(avatarPath);

                        // Cập nhật đường dẫn vào CSDL
                        phukien.Avartar = "/UploadFiles/PhuKien/" + avatarFileName;

                    }
                    catch (Exception ex)
                    {

                        System.Diagnostics.Debug.WriteLine("Lỗi khi lưu tệp: " + ex.Message);


                    }

                    // lưu thong tin vao cơ so dư lieu
                    db.PhuKiens.Add(phukien);
                    db.SaveChanges();


                }
                return RedirectToAction("listAccessory");

            }
            return View(phukien);
        }


        public ActionResult editAccessory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            PhuKien phuKien = db.PhuKiens.Find(id);
            if (phuKien == null)
            {
                return HttpNotFound();
            }

            return View(phuKien);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editAccessory(PhuKien phuKien)
        {
            if (ModelState.IsValid)
            {
                // tìm bản ghi hiện tại
                var existingAccessory = db.PhuKiens.Find(phuKien.Id);
                if (existingAccessory != null)
                {
                    //câp nhạt các thuộc tính cho thay đổi

                    existingAccessory.Name = phuKien.Name;
                    existingAccessory.Price = phuKien.Price;
                    existingAccessory.Mission = phuKien.Mission;


                    db.SaveChanges();
                    return RedirectToAction("listAccessory");



                }

            }
            return View(phuKien);
        }

        public ActionResult deleteAccessory(int id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            }
            PhuKien phuKien = db.PhuKiens.Find(id);
            if (phuKien == null)
            {
                return HttpNotFound();
            }
            return View(phuKien);
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult deleteAccessoryConfirmed(int id)
        {

            PhuKien phuKien = db.PhuKiens.Find(id);
            if (phuKien == null)
            {
                return HttpNotFound();
            }
            db.PhuKiens.Remove(phuKien); 
            db.SaveChanges();

            return RedirectToAction("listAccessory", new { area = "Admin" });
        }


    }
}

