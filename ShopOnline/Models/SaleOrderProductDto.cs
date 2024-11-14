using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopOnline.Models
{
    public class SaleOrderProductDto: SaleOrder
    {

        
        public List<SaleOrderProduct> saleOrderProducts { get; set; }

    }
}