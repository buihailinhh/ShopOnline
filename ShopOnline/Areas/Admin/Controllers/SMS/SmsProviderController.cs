using Microsoft.Extensions.Logging;
using NLog;
using ShopOnline.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ShopOnline.Utils;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Data.Entity.Validation;

namespace ShopOnline.Areas.Admin.Controllers.SMS
{
    [RoutePrefix("api/sms")]
    public class SmsProviderController : ApiController
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        ShopOnlineEntities db = new ShopOnlineEntities();
       

        [Route("receive")]
        [HttpGet]
        public HttpResponseMessage Receive(string Command_Code, string User_ID, string Service_ID, string Request_ID, string Message)
        {
            string MT = saicuphap;
            int status = 0;
            logger.Info(string.Format("[MO] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, Message));

            try
            {

                //format sdt ve dau 0
                User_ID = Utils.Control.formatUserId(User_ID, 2);
                string[] words = Message.ToUpper().Trim().Split(' ');

                // Kiểm tra cú pháp Command_Code
                if (words[0].Equals("BB1"))
                {
                    if (words.Length == 2)
                    {
                        if (words[1] == "BH")
                        {
                            var customer = db.Customers.SingleOrDefault(a => a.phone == User_ID);
                            if (customer == null)
                            {
                                MT = bh_notvalid;
                            }
                            else
                            {
                                // lưu thông tin bảo hành
                                CreateWarranti(customer);
                                MT = bh_ok;

                            }

                        }
                        else
                        {

                        }


                    }

                    // Logic xử lý tin nhắn ở đây
                    // ...


                }

                else//khong dung cu phap
                {
                    MT = saicuphap;
                }
            }
            catch (Exception ex)
            {
                status = 0;
                MT = saicuphap;
                logger.Error(ex.Message);
            }

            var result = new Result()
            {
                status = "0",
                message = MT,
                phone = User_ID
            };
            var log_mo = new LOG_MO()
            {

                
                User_Id = User_ID,
                Service_Id = Service_ID,
                Request_Id = Request_ID,
                Message = Message,
                Createdate = DateTime.Now,
                Status = status,
                Response = MT
            };
            db.LOG_MO.Add(log_mo);
            db.SaveChanges();
            //var response = new HttpResponseMessage();
            //response.Content = new StringContent("0|" + MT);
            //response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            logger.Info(string.Format("[MT] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, MT));
            var response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
            return response;



        }


        void CreateWarranti(Customer customer)
        {
            var warranti = new Warranti()
            {
                Status = 0,
                Createdate = DateTime.Now,
                Chanel = "SMS",
                Phone = customer.phone,
                Address = customer.address,
                Ward = customer.Ward,
                District = customer.District,
                Province = customer.Province

            };
            db.Warrantis.Add(warranti);
            db.SaveChanges();
            //tao ma phieu bao hanh 
            warranti.Code = Utils.Control.CreateCode(warranti.Id);
              db.Entry(warranti).State = System.Data.Entity.EntityState.Modified;
            
            db.SaveChanges();
            //send zns bảo hành
            //string[] paraTemp = new string[] { customer.Name, warranti.Code, warranti.ProductName, warranti.Model };
            //SendZNS apiZns = new SendZNS();
            //apiZns.CallApiZNS(customer.Phone, paraTemp, 287839, warranti.Code);
        }



        class Result
        {
            public string message { get; set; }
            public string status { get; set; }

            public string phone { get; set; }
        }

        string saicuphap = "Cu phap khong dung, vui long soan ELM MaCao gui 8077. Moi thong tin lien he 1900636925. Tran trong cam on.";
        string seri_notvalid = "Ma cao san pham cua ban khong ton tai, vui long kiem tra lai. Moi thong tin lien he 1900636925 hoac https://baohanh.elmich.vn. Tran trong cam on.";
        string seri_outdate = "San pham {0} da het han bao hanh vao {1}. Moi thong tin lien he 1900636925 hoac https://baohanh.elmich.vn . Tran trong cam on.";
        string seri_actived = "San pham {0} da duoc kich hoat bao hanh thanh cong tu ngay {1} den ngay {2}. Moi thong tin lien he 1900636925. Tran trong cam on.";
        string bh_ok = "Yeu cau bao hanh cua ban da duoc ghi nhan, chung toi se lien he lai trong vong 24h. Lien he 1900636925 hoac https://baohanh.elmich.vn. Tran trong cam on.";
        string bh_notvalid = "So dien thoai Yeu cau bao hanh cua ban khong ton tai. Moi thong tin lien he 1900636925 hoac https://baohanh.elmich.vn. Tran trong cam on.";
        string kh_actived = "San pham {0} da duoc kich hoat tu ngay {1} den ngay {2}. Moi thong tin lien he 1900636925. Tran trong cam on.";
        string agent_actived = "San pham {0} kich hoat thanh cong tai {1} vao ngay {2}, ngay het han {3}. Moi thong tin lien he 1900636925. Tran trong cam on.";
        string agent_notvalid = "TU CHOI do ma dai ly khong ton tai trong he thong. Moi thong tin lien he 1900636925. Tran trong cam on.";
        string agent_phone_notvalid = "SDTKH khong dung (Gom 10 ki tu la SO), vui long kiem tra lai. Moi thong tin lien he 1900636925.Tran trong cam on.";
    }
}
