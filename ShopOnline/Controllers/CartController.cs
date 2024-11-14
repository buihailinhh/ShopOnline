    using Microsoft.AspNet.Identity;
    using ShopOnline.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Data.Entity;

    namespace ShopOnline.Controllers
    {
        public class CartController : Controller
        {

            private ShopOnlineEntities db = new ShopOnlineEntities();
            // GET: Cart
            [Authorize]
            public ActionResult CartView()
            {
                var cart = (List<CartItem>)Session["Cart"];
                decimal totalCartPrice = 0;

                if (cart != null)
                {
                    totalCartPrice = cart.Sum(item => item.TotalPrice);
                }

                ViewBag.TotalCartPrice = totalCartPrice;

                // Retrieve logged-in user information if available
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var user = db.AspNetUsers.Find(userId);
                        if (user != null)
                        {
                            ViewBag.LoginedUser = user;
                        }
                    }
                }

                return View(cart);
            }



            [HttpPost]
            public JsonResult AddToCart(int? productId, int? accessoryId, int quantity)
            {
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                if (productId.HasValue)
                {
                    var cartItem = cart.FirstOrDefault(c => c.ProductId == productId.Value);
                    if (cartItem != null)
                    {
                        cartItem.Quantity += quantity;
                    }
                    else
                    {
                        var product = db.Products.Find(productId.Value);
                        if (product != null)
                        {
                            cart.Add(new CartItem
                            {
                                ProductId = product.Id,
                                Product = product,
                                Quantity = quantity
                            });
                        }
                        else
                        {
                            return Json(new { error = "Product not found" });
                        }
                    }
                }
                else if (accessoryId.HasValue)
                {
                    var cartItem = cart.FirstOrDefault(c => c.AccessoryId == accessoryId.Value);
                    if (cartItem != null)
                    {
                        cartItem.Quantity += quantity;
                    }
                    else
                    {
                        var accessory = db.PhuKiens.Find(accessoryId.Value);
                        if (accessory != null)
                        {
                            cart.Add(new CartItem
                            {
                                AccessoryId = accessory.Id,
                                PhuKien = accessory,
                                Quantity = quantity
                            });
                        }
                        else
                        {
                            return Json(new { error = "Accessory not found" });
                        }
                    }
                }

                Session["Cart"] = cart;
                return Json(new { totalCartProducts = cart.Sum(c => c.Quantity) });
            }



        
        
            [HttpPost]
            public ActionResult UpdateQuantity(int productId, int change)
            {
                var cart = Session["Cart"] as List<CartItem>;

                if (cart != null)
                {
                    var item = cart.FirstOrDefault(x => x.Product.Id == productId);
                    if (item != null)
                    {
                        item.Quantity += change;

                        // Không cho phép số lượng nhỏ hơn 1
                        if (item.Quantity < 1)
                        {
                            item.Quantity = 1;
                        }

                        // Cập nhật lại giỏ hàng trong session
                        Session["Cart"] = cart;

                        // Trả về JSON với số lượng mới để cập nhật giao diện
                        return Json(new { success = true, newQuantity = item.Quantity });
                    }
                }

                // Trả về JSON khi không tìm thấy sản phẩm trong giỏ hàng hoặc giỏ hàng rỗng
                return Json(new { success = false });
            }



            // update accessory

            [HttpPost]
            public JsonResult UpdateAccessoryQuantity(int accessoryId, int change)
            {
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
                var cartItem = cart.FirstOrDefault(c => c.AccessoryId == accessoryId);

                if (cartItem != null)
                {
                    cartItem.Quantity += change;
                    if (cartItem.Quantity <= 0)
                    {
                        cart.Remove(cartItem);
                    }
                }

                Session["Cart"] = cart;

                // Tính toán lại tổng số lượng và giá
                var totalCartPrice = cart.Sum(c => c.TotalPrice);
                return Json(new
                {
                    success = true,
                    newQuantity = cartItem?.Quantity ?? 0,
                    newTotalPrice = cartItem?.TotalPrice ?? 0,
                    totalCartPrice = totalCartPrice
                });
            }

            // xóa sảm phẩm khoi gio hang
            [HttpPost]
            public JsonResult DeleteCartItem(int id)
            {
                var cart = Session["Cart"] as List<CartItem>;

                if (cart != null)
                {

           var itemToRemove = cart.FirstOrDefault(x =>
         (x.Product != null && x.Product.Id == id) ||
         (x.PhuKien != null && x.PhuKien.Id == id));

                    if (itemToRemove != null)
                    {
                        cart.Remove(itemToRemove);
                        Session["Cart"] = cart;
                        return Json(new { success = true, redirectTo = Url.Action("CartView", "Cart") });
                    }
                }

                return Json(new { success = false });
            }




            public ActionResult PlaceOrder(string CustomerName, string CustomerAddress, string CustomerMobile, string CustomerEmail)
            {
                var cart = (List<CartItem>)Session["Cart"];
                if (cart == null || !cart.Any())
                {
                    return RedirectToAction("CartView");
                }

                // Tạo đơn hàng mới
                var saleOrder = new SaleOrder
                {
                    Code = GenerateOrderCode(),
                    Create_date = DateTime.Now,
                    Update_date = DateTime.Now,
                    Create_by = User.Identity.IsAuthenticated ? User.Identity.GetUserName() : "Guest",
                    Update_by = User.Identity.IsAuthenticated ? User.Identity.GetUserName() : "Guest",
                    status = 1,
                    Customer_name = CustomerName,
                    Customer_address = CustomerAddress,
                    Customer_mobile = CustomerMobile,
                    Customer_email = CustomerEmail
                };

                // Chuyển đổi UserId thành int nếu có
                var userId = User.Identity.GetUserId();
                int userIdInt;
                if (int.TryParse(userId, out userIdInt))
                {
                    saleOrder.User_Id = userIdInt;
                }
                else
                {
                    saleOrder.User_Id = null; // Hoặc xử lý theo cách khác nếu cần
                }

                saleOrder.total = cart.Sum(x => x.TotalPrice);

                using (var context = new ShopOnlineEntities())
                {
                    // Lưu thông tin đơn hàng vào cơ sở dữ liệu
                    context.SaleOrders.Add(saleOrder);
                    context.SaveChanges();

                    int orderId = saleOrder.Id;

                    // Lưu thông tin mặt hàng và phụ kiện vào bảng SaleOrderProduct
                    foreach (var item in cart)
                    {
                        // Nếu là sản phẩm chính
                        if (item.Product != null)
                        {
                            var saleOrderProduct = new SaleOrderProduct
                            {
                                Sale0rder_Id = orderId,
                                Product_Id = item.ProductId,
                                Name = item.Product.Name,
                                Price = item.Product.Sale_price ?? item.Product.Price, // Xử lý giá nếu có sale price
                                Quatity = item.Quantity,
                                Description = item.Product.Short_description,
                                Create_by = User.Identity.IsAuthenticated ? User.Identity.GetUserName() : "Guest",
                                Create_date = DateTime.Now
                            };
                            context.SaleOrderProducts.Add(saleOrderProduct);
                        }

                        // Nếu có phụ kiện đi kèm
                        if (item.PhuKien != null)
                        {
                            var saleOrderAccessory = new SaleOrderProduct
                            {
                                Sale0rder_Id = orderId,
                                Accessory_Id = item.AccessoryId, // Lưu id của phụ kiện
                                Name = item.PhuKien.Name,
                                Price = item.PhuKien.Price, // Giá của phụ kiện
                                Quatity = item.Quantity,
                                Description = item.PhuKien.Description,
                                //Create_by = User.Identity.IsAuthenticated ? User.Identity.GetUserName() : "Guest",
                                Create_date = DateTime.Now
                            };
                            context.SaleOrderProducts.Add(saleOrderAccessory);
                        }
                    }

                    // Lưu các mặt hàng vào cơ sở dữ liệu
                    context.SaveChanges();

                    // Xóa giỏ hàng
                    Session["Cart"] = null;

                    // Chuyển hướng đến trang xác nhận đơn hàng
                    return RedirectToAction("OrderConfirmation", new { orderId });
                }
            }




            public ActionResult OrderConfirmation(int orderId)
            {
                using (var context = new ShopOnlineEntities())
                {
                    // Truy vấn lấy đơn hàng trước
                    var saleOrder = context.SaleOrders
                        .Where(a => a.Id == orderId)
                        .FirstOrDefault();

                    if (saleOrder == null)
                    {
                        return HttpNotFound();
                    }

                    // Truy vấn lấy danh sách sản phẩm liên quan đến đơn hàng
                    var saleOrderProducts = context.SaleOrderProducts
                        .Where(x => x.Sale0rder_Id == saleOrder.Id)
                        .ToList();

                    // Tạo đối tượng DTO để chuyển dữ liệu cho view
                    var saleOrderDto = new SaleOrderProductDto
                    {
                        Id = saleOrder.Id,
                        Code = saleOrder.Code,
                        Customer_name = saleOrder.Customer_name,
                        Customer_address = saleOrder.Customer_address,
                        Customer_mobile = saleOrder.Customer_mobile,
                        Customer_email = saleOrder.Customer_email,
                        total = saleOrder.total,
                        status = saleOrder.status,
                        saleOrderProducts = saleOrderProducts // Gán danh sách sản phẩm vào DTO
                    };

                    // Trả về view với dữ liệu đơn hàng
                    return View(saleOrderDto);
                }
            }




            private string GenerateOrderCode()
            {
                return "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }


        }
    }