using Microsoft.AspNetCore.Mvc;
using HomeRepairHub.Models;
using HomeRepairHub.Data;

namespace HomeRepairHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Requests");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                if (!string.IsNullOrEmpty(user.Industry)) 
                    HttpContext.Session.SetString("UserIndustry", user.Industry);

                return RedirectToAction("Index", "Requests");
            }
            ViewBag.Error = "البريد الإلكتروني أو كلمة المرور غير صحيحة.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Requests");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ViewBag.Error = "يوجد مستخدم مسجل بهذا البريد الإلكتروني بالفعل.";
                    return View(user);
                }

                user.Id = Guid.NewGuid();
                _context.Users.Add(user);
                _context.SaveChanges();

                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                if (!string.IsNullOrEmpty(user.Industry)) 
                    HttpContext.Session.SetString("UserIndustry", user.Industry);

                return RedirectToAction("Index", "Requests");
            }
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
