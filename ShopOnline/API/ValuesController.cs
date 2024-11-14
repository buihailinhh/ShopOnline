using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShopOnline.API
{

    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        private readonly ShopOnlineEntities db = new ShopOnlineEntities();


        [HttpGet]
       
        public IHttpActionResult GetCategory()
        {
            var query = db.Categories.ToList();
            return Ok(query);
        }

        [HttpGet ]
        public IHttpActionResult GetCategory(int id)
        {
            var query = db.Categories.Where(a=>a.Id == id).ToList();
            return Ok(query);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult PostCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Categories.Add(category);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = category.Id }, category);
        }




    }
}