using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeRepairHub.Models;
using HomeRepairHub.Data;

namespace HomeRepairHub.Controllers
{
    public class RequestsController : Controller
    {
        private readonly AppDbContext _context;

        public RequestsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("UserId") != null;
        }

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

            var userRole = HttpContext.Session.GetString("UserRole");
            var userIndustry = HttpContext.Session.GetString("UserIndustry");
            var userIdString = HttpContext.Session.GetString("UserId");

            var query = _context.Requests.Include(r => r.User).Include(r => r.Worker).AsQueryable();

            if (userRole == "Worker" && !string.IsNullOrEmpty(userIndustry))
            {
                query = query.Where(r => 
                    (r.Status == "Pending" && r.ProblemType.Trim() == userIndustry.Trim()) || 
                    (r.WorkerId.ToString() == userIdString)
                );
            }

            var requests = query.OrderByDescending(r => r.CreatedAt).ToList();
            return View(requests);
        }

        [HttpPost]
        public IActionResult Accept(Guid id) 
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            
            var userRole = HttpContext.Session.GetString("UserRole");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (userRole != "Worker" || string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index");

            var request = _context.Requests.Find(id);
            if (request != null && request.Status == "Pending")
            {
                request.Status = "Confirmed";
                request.WorkerId = Guid.Parse(userIdString);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CancelRequest(Guid id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);
            var request = _context.Requests.FirstOrDefault(r => r.Id == id && r.UserId == userId);
            
            if (request != null)
            {
                _context.Requests.Remove(request);
                _context.SaveChanges();
            }

            return RedirectToAction("MyRequests");
        }

        [HttpPost]
        public IActionResult CancelAcceptance(Guid id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Worker" || string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index");

            var userId = Guid.Parse(userIdString);
            var request = _context.Requests.FirstOrDefault(r => r.Id == id && r.WorkerId == userId);

            if (request != null)
            {
                request.Status = "Pending";
                request.WorkerId = null;
                _context.SaveChanges();
            }

            return RedirectToAction("MyRequests");
        }

        public IActionResult MyRequests()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

            var userIdString = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            
            var userId = Guid.Parse(userIdString);
            IQueryable<Request> query = _context.Requests.Include(r => r.User).Include(r => r.Worker);

            if (userRole == "Worker")
            {
                query = query.Where(r => r.WorkerId == userId);
            }
            else
            {
                query = query.Where(r => r.UserId == userId);
            }

            var requests = query.OrderByDescending(r => r.CreatedAt).ToList();
            return View(requests);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Request request)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            request.UserId = Guid.Parse(userIdString);
            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";
            
            ModelState.Remove("User"); 
            ModelState.Remove("Status");
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                request.Id = Guid.NewGuid();
                _context.Requests.Add(request);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(request);
        }
    }
}
