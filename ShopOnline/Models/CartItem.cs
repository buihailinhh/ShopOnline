using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopOnline.Models
{
    public class CartItem
{
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public PhuKien PhuKien { get; set; } 
        public  int AccessoryId { get; set; } 
    public int Quantity { get; set; }

        public decimal TotalPrice
        {
            get
            {
                // Sử dụng giá sản phẩm nếu Product không phải null
                if (Product != null)
                {
                    decimal salePrice = Product.Sale_price ?? 0;
                    return salePrice * Quantity;
                }
                // Sử dụng giá phụ kiện nếu PhuKien không phải null
                if (PhuKien != null)
                {
                    decimal accessoryPrice = PhuKien.Price;
                    return accessoryPrice * Quantity;
                }
                // Nếu cả Product và PhuKien đều null, trả về 0
                return 0;
            }
        }
    }


    
}