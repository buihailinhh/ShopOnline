using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopOnline.Areas.Admin.Data
{
    public class ProductCategory:Product
    {
        public string CategoryName { get;set; }
        public string CategoryDescription { get;set; }
       



    }
}