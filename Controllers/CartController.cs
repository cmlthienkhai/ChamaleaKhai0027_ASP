using ChamaleaKhai0027.Context;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChamaleaKhai0027.Controllers
{
    public class CartController : Controller
    {
        private WedsiteEcomEntities1 _context = new WedsiteEcomEntities1();

        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["Cart"] as List<Cart> ?? new List<Cart>();
            var total = cart.Sum(c => c.TotalPrice);

            // Kiểm tra giá trị của session
            if (cart != null)
            {
                ViewBag.Total = total.ToString("C", new CultureInfo("vi-VN"));
                ViewBag.CartItemCount = cart.Count;
            }
            else
            {
                ViewBag.Total = "0";
                ViewBag.CartItemCount = 0;
            }

            return View(cart);
        }


        // GET: ProductList
        public ActionResult ProductList()
        {
            var products = _context.Products.ToList(); // Lấy danh sách sản phẩm từ cơ sở dữ liệu
            return View(products); // Trả về view với danh sách sản phẩm
        }

        [HttpPost]
        public JsonResult AddToCart(int productId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return Json(new { Message = "Số lượng sản phẩm không hợp lệ!" });
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { Message = "Sản phẩm không tồn tại!" });
            }

            var cart = Session["Cart"] as List<Cart> ?? new List<Cart>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.Price;
            }
            else
            {
                var newItem = new Cart
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl, // Đây là nơi bạn gán ảnh
                    Quantity = quantity,
                    Price = product.Price,
                    TotalPrice = product.Price * quantity
                };
                cart.Add(newItem);
                // Kiểm tra giá trị ProductImage
                Console.WriteLine($"ProductImage: {newItem.ProductImage}");
            }

            Session["Cart"] = cart;

            var total = cart.Sum(c => c.TotalPrice);

            return Json(new
            {
                Message = "Sản phẩm đã được thêm vào giỏ hàng!",
                Count = cart.Sum(c => c.Quantity), // Số lượng sản phẩm
                Total = total.ToString("C", new CultureInfo("vi-VN"))
            });
        }
        [HttpPost]
        public JsonResult Update(int productId, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }

            var cart = Session["Cart"] as List<Cart> ?? new List<Cart>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                existingItem.Price = product.Price; // Cập nhật giá từ cơ sở dữ liệu nếu cần
                existingItem.TotalPrice = existingItem.Price * quantity;
                Session["Cart"] = cart;

                var newTotalPrice = cart.Sum(c => c.TotalPrice);

                return Json(new
                {
                    success = true,
                    newPrice = existingItem.TotalPrice.ToString("C", new CultureInfo("vi-VN")),
                    newTotalPrice = newTotalPrice.ToString("C", new CultureInfo("vi-VN"))
                });
            }

            return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng!" });
        }


        [HttpPost]
        public JsonResult Remove(int id)
        {
            var cart = Session["Cart"] as List<Cart> ?? new List<Cart>();

            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove); // Xóa sản phẩm khỏi giỏ hàng
                Session["Cart"] = cart;
                return Json(new { Message = "Sản phẩm đã được xóa khỏi giỏ hàng", Count = cart.Count });
            }

            return Json(new { Message = "Sản phẩm không tồn tại trong giỏ hàng" });
        }

        // Thay đổi thông tin này với thông tin của bạn
        private const string VNPAY_URL = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private const string VNPAY_TMN_CODE = "YOUR_TMN_CODE";
        private const string VNPAY_SECRET_KEY = "YOUR_SECRET_KEY";
        public ActionResult Pay()
        {
            // Logic xử lý thanh toán với thẻ tín dụng
            return View();
        }
        [HttpPost]
        public ActionResult PayWithVNPay()
        {
            // Tạo dữ liệu thanh toán
            var paymentData = CreateVNPayData();
            // Chuyển hướng đến VNPay để thanh toán
            return Redirect(paymentData);

        }

        private string CreateVNPayData()
        {
            var amount = GetCartTotal("VND"); // Tổng tiền từ giỏ hàng
            var transactionId = Guid.NewGuid().ToString("N"); // Mã giao dịch duy nhất
            var returnUrl = Url.Action("PaymentResult", "Cart", null, Request.Url.Scheme); // Sử dụng Request.Url.Scheme
            var orderId = DateTime.Now.Ticks.ToString();

            var data = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_TmnCode", VNPAY_TMN_CODE },
                { "vnp_Amount", (amount * 100).ToString() }, // VNPay yêu cầu số tiền tính bằng đồng
                { "vnp_Command", "pay" },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_Currency", "VND" },
                { "vnp_IpAddr", Request.UserHostAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", "Thanh toán đơn hàng #" + orderId },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", transactionId }
            };

            var hashData = string.Join("&", data.OrderBy(x => x.Key).Select(x => x.Key + "=" + x.Value));
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(VNPAY_SECRET_KEY));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData));
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var url = VNPAY_URL + "?" + hashData + "&vnp_SecureHash=" + hashString;
            return url;
        }

        [HttpGet]
        public ActionResult PaymentReturn()
        {
            var vnp_ResponseCode = Request.QueryString["vnp_ResponseCode"];
            var vnp_TxnRef = Request.QueryString["vnp_TxnRef"];
            var vnp_Amount = Request.QueryString["vnp_Amount"];

            if (vnp_ResponseCode == "00")
            {
                ViewBag.Message = "Thanh toán thành công!";
            }
            else
            {
                ViewBag.Message = "Thanh toán thất bại!";
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PayWithCreditCard(string token)
        {
            try
            {
                StripeConfiguration.ApiKey = STRIPE_SECRET_KEY; // Thay thế bằng khóa bí mật của bạn

                var options = new ChargeCreateOptions
                {
                    Amount = (long)(GetCartTotal("USD") * 100), // Stripe yêu cầu số tiền tính bằng xu
                    Currency = "usd",
                    Description = "Thanh toán đơn hàng",
                    Source = token, // Token từ client-side
                };

                var service = new ChargeService();
                Charge charge = await service.CreateAsync(options);

                if (charge.Status == "succeeded")
                {
                    ViewBag.Message = "Thanh toán thành công!";
                }
                else
                {
                    ViewBag.Message = "Thanh toán thất bại!";
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và ghi log nếu cần
                ViewBag.Message = "Đã xảy ra lỗi khi thanh toán: " + ex.Message;
            }

            return View("PaymentReturn");
        }


        private decimal GetCartTotal(string currency)
        {
            var cart = Session["Cart"] as List<Cart> ?? new List<Cart>();
            if (currency == "USD")
            {
                // Giả sử giá trị USD
                return cart.Sum(c => c.TotalPrice / 23000); // Chuyển đổi VND sang USD với tỷ giá giả sử là 1 USD = 23000 VND
            }
            else if (currency == "VND")
            {
                // Giá trị VND
                return cart.Sum(c => c.TotalPrice);
            }
            return 0;
        }

        private const string STRIPE_SECRET_KEY = "sk_test_4eC39HqLyjWDarjtT1zdp7dc"; // Thay thế bằng khóa bí mật của bạn

    }

}