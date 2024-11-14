using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopOnline.Models
{
    public class ProductDetail : Product
    {
        public List<ProductImage> Images { get; set; }

    }
    
}