using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShopOnline.Areas.Admin.Controllers.API_Controller
{


    public class CustomerController : ApiController
    {

        public IHttpActionResult GetAllCustomer()
        {
            var customer = new List<object>();
            using (var x = new ShopOnlineEntities())
            {
                customer = x.Customers
                            .Select(c => new
                            {
                                CustomerId = c.customer_id,
                                LastName = c.last_name,
                                Phone = c.phone,
                                Note = c.note,
                                CreateDate = c.create_date,
                            }).ToList<object>();
            }
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        public IHttpActionResult GetCustomerById(int id)
        {
            using (var x = new ShopOnlineEntities())
            {
                // Tìm kiếm khách hàng dựa trên customer_id
                var customer = x.Customers
                                .Where(c => c.customer_id == id)
                                .Select(c => new
                                {
                                    CustomerId = c.customer_id,
                                    LastName = c.last_name,
                                    Phone = c.phone,
                                    Note = c.note,
                                    CreateDate = c.create_date
                                })
                                .FirstOrDefault(); // Lấy khách hàng đầu tiên phù hợp với ID

                // Kiểm tra xem khách hàng có tồn tại không
                if (customer == null)
                {
                    return NotFound(); // Trả về mã lỗi 404 nếu không tìm thấy
                }

                return Ok(customer); // Trả về thông tin khách hàng nếu tìm thấy
            }
        }


        //post
        public IHttpActionResult PostNewCustomer(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            using (var x = new ShopOnlineEntities())
            {
                x.Customers.Add(new Customer()
                {
                    customer_id = customer.customer_id,
                    last_name = customer.last_name,
                    phone = customer.phone,
                    note = customer.note,
                    create_date = DateTime.Now
                });
                x.SaveChanges();

            }
            return Ok();


        }


        public IHttpActionResult PutCustomer(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);


            }
            using (var x = new ShopOnlineEntities())
            {
                var checkCustomer = x.Customers.Where(c => c.customer_id == customer.customer_id).FirstOrDefault();

                if (checkCustomer != null)
                {
                    checkCustomer.last_name = customer.last_name;
                    checkCustomer.phone = customer.phone;
                    checkCustomer.note = customer.note;
                    checkCustomer.create_date = DateTime.Now;
                    x.SaveChanges();

                }
                else
                {
                    return NotFound();
                }



            }
            return Ok();
        }

        public IHttpActionResult DeleteCustomer(int id)
        {
            if (id <= 0)
                return BadRequest("Please enter valid customer Id ");

            using (var x = new ShopOnlineEntities())
            {
                var customer = x.Customers.Where(c => c.customer_id == id).FirstOrDefault(); 
                x.Entry(customer).State = System.Data.Entity.EntityState .Deleted;
                x.SaveChanges(); 
            }
            return Ok();
    }

    }
}
