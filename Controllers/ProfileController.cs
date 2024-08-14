using System.Linq;
using System.Web.Mvc;
using ChamaleaKhai0027.Context;

namespace ChamaleaKhai0027.Controllers
{
    public class ProfileController : Controller
    {
        private WedsiteEcomEntities1 _context;

        public ProfileController()
        {
            _context = new WedsiteEcomEntities1();
        }

        // GET: Profile/Index
        public ActionResult Index()
        {
            var email = Session["username"] as string;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "User");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View(user);
        }
        [HttpPost]
        public ActionResult Logout()
        {
            // Xóa thông tin người dùng từ session
            HttpContext.Session.Clear();

            // Điều hướng về trang chính
            return RedirectToAction("Index", "Home");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
