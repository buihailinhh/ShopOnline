using Newtonsoft.Json;
using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopOnline.Areas.Admin.Controllers.API_Controller
{

  
    public class AllCategoryController : Controller

    {
        private readonly string apiUrl = "https://localhost:44312/api/categories";  
        // GET: Admin/AllCategory
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }



        public async Task<ActionResult> Edit(int id)
        {
            Category category = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);

                // Gọi API để lấy category theo ID
                HttpResponseMessage response = await client.GetAsync(apiUrl + "/" + id);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc dữ liệu JSON trả về từ API
                    var jsonData = await response.Content.ReadAsStringAsync();
                    category = JsonConvert.DeserializeObject<Category>(jsonData);
                }
                else
                {
                    // Xử lý lỗi nếu API trả về không thành công
                    ViewBag.ErrorMessage = "Failed to retrieve data from API.";
                }
            }

            return View(category);  // Trả dữ liệu category cho View
        }

     //   Action chỉnh sửa Category(POST)
        [HttpPost]
        public async Task<ActionResult> Edit(Category category)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);

                // Gửi dữ liệu category đã chỉnh sửa tới API
                var content = new StringContent(JsonConvert.SerializeObject(category), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(apiUrl + "/" + category.Id, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");  // Chuyển hướng về danh sách sau khi sửa thành công
                }
                else
                {
                    ViewBag.ErrorMessage = "Failed to update the category.";
                }
            }

            return View(category);
        }


    }
}