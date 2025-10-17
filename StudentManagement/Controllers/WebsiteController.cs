using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class WebsiteController : Controller
    {
        DatabaseConnectionEF connectionef;
        public WebsiteController(DatabaseConnectionEF connectionef)
        {
            this.connectionef = connectionef;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Enquiry(enquiry data)
        {
            connectionef.enquiry.Add(data);
            connectionef.SaveChanges();
            TempData["enq"] = "Your enquiry submited successfully. Wait for response.";
            return RedirectToAction("ContactUs");
        }

        public IActionResult Gallery()
        {
            return View();
        }
    }
}
