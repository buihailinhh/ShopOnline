using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ShopOnline.Areas.Admin.Data;
using ShopOnline.Models;

[RoutePrefix("api/categories")]
public class CategoryAPIController : ApiController
{
    private readonly ShopOnlineEntities db = new ShopOnlineEntities();

    //[HttpGet]
    //[Route("allcategory")]
    //public IHttpActionResult GetCategory()
    //{
    //    var query = db.Categories.ToList();
    //    return Ok(query);
    //}
    // // GET: api/categories/pagination
    //[HttpGet]
    //[Route("pagination")]
    //public IHttpActionResult GetPagedCategories(int pageNumber = 1, int pageSize = 6)
    //{
    //    // Kiểm tra tổng số lượng danh mục
    //    var totalCategories = db.Categories.Count();

    //    // Phân trang
    //    var pagedCategories = db.Categories
    //        .OrderBy(c => c.Name) // Sắp xếp theo tên hoặc trường khác
    //        .Skip((pageNumber - 1) * pageSize) // Bỏ qua các danh mục trước đó
    //        .Take(pageSize) // Lấy số lượng danh mục theo kích thước trang
    //        .Select(c => new
    //        {
    //            Id = c.Id,
    //            Name = c.Name,
    //            Description = c.Description,
    //            CreateDate = c.Create_date
    //        })
    //        .ToList();

    //    // Nếu không có danh mục nào (trường hợp số trang không hợp lệ)
    //    if (!pagedCategories.Any())
    //    {
    //        return NotFound();
    //    }

    //    // Trả về kết quả với thông tin phân trang
    //    var result = new
    //    {
    //        TotalItems = totalCategories, // Tổng số danh mục
    //        PageSize = pageSize,          // Số lượng danh mục trên mỗi trang
    //        PageNumber = pageNumber,      // Trang hiện tại
    //        Categories = pagedCategories  // Danh sách danh mục trên trang hiện tại
    //    };

    //    return Ok(result);
    //}


    [HttpGet]
[Route("allcategory")]
public IHttpActionResult GetCategory(int pageNumber = 1, int pageSize = 6)
{
    // Tính tổng số lượng danh mục
    var totalItems = db.Categories.Count();

    // Phân trang danh mục
    var categories = db.Categories
                       .OrderBy(c => c.Id) // Sắp xếp theo Id hoặc trường khác mà bạn muốn
                       .Skip((pageNumber - 1) * pageSize) // Bỏ qua các danh mục trước đó
                       .Take(pageSize) // Lấy số lượng danh mục theo kích thước trang
                       .Select(c => new
                       {
                           Id = c.Id,
                           Name = c.Name,
                           Description = c.Description,
                           CreateDate = c.Create_date
                       })
                       .ToList();

    // Kiểm tra nếu không có danh mục nào
    if (!categories.Any())
    {
        return NotFound(); // Trả về 404 nếu không tìm thấy danh mục nào
    }

    // Trả về kết quả với thông tin phân trang
    var result = new
    {
        TotalItems = totalItems,  // Tổng số danh mục
        PageSize = pageSize,      // Số lượng danh mục trên mỗi trang
        PageNumber = pageNumber,  // Trang hiện tại
        Categories = categories   // Danh sách danh mục trên trang hiện tại
    };

    return Ok(result);
}



    //[HttpGet]
    //[Route("{id:int}")]
    //public IHttpActionResult GetCategory(int id)
    //{
    //    var category = db.Categories.Find(id);
    //    if (category == null)
    //    {
    //        return NotFound();
    //    }
    //    return Ok(category);
    //}


    [HttpGet]
    [Route("{id:int}")]
    public IHttpActionResult GetCategory(int id)
    {
        Category category = null;
        CategoryDTO categoryDto = null;

        using (var context = new ShopOnlineEntities())
        {
            category = context.Categories
                              .Where(c => c.Id == id)


                              .FirstOrDefault();
        }

        if (category != null)
        {
            categoryDto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                
            };
        }

        if (categoryDto == null)
        {
            return NotFound(); // Trả về mã trạng thái 404 nếu không tìm thấy danh mục
        }

        return Ok(categoryDto); // Trả về kết quả dưới dạng JSON với mã trạng thái 200
    }




    [HttpPost]
[Route("addcategory")]
public IHttpActionResult PostCategory(Category category)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

        category.Create_date = DateTime.Now;  // Hoặc lấy từ request nếu cần
        db.Categories.Add(category);
    db.SaveChanges();

    // Thay thế CreatedAtRoute bằng Created (nếu không có route DefaultApi)
    return Created(new Uri(Request.RequestUri + "/" + category.Id), category);
}

    [HttpPut]  
    [Route("{id:int}")]
    public IHttpActionResult PutCategory(int id, Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != category.Id)
        {
            return BadRequest();
        }

        db.Entry(category).State = System.Data.Entity.EntityState.Modified;
        db.SaveChanges();

        return StatusCode(HttpStatusCode.NoContent);
    }

    // DELETE api/categories/{id}
    [HttpDelete]
    [Route("{id:int}")]
    public HttpResponseMessage DeleteCategory(int id)
    {
        try
        {
            // Tìm kiếm category với ID được cung cấp
            var category = db.Categories.FirstOrDefault(c => c.Id == id);

            // Nếu không tìm thấy category
            if (category == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Không tìm thấy category");
            }
                      db.Categories.Remove(category);
            
            db.SaveChanges();

                       return Request.CreateResponse(HttpStatusCode.OK, "Xóa thành công Category");
        }
        catch (Exception ex)
        {
            // Trả về lỗi nếu có ngoại lệ
            return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        }
    }


    // GET: api/categories/search
    [HttpGet]
    [Route("search")]
    public IHttpActionResult SearchCategories(string searchTerm = "")
    {
        // Kiểm tra từ khóa tìm kiếm
        var categories = db.Categories.Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm));

        // Nếu không có kết quả
        if (!categories.Any())
        {
            return NotFound();  
        }

        // Trả về danh sách danh mục tìm được
        var categoryList = categories.Select(c => new
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreateDate = c.Create_date
        }).ToList();

        return Ok(categoryList);  
    }

   








}