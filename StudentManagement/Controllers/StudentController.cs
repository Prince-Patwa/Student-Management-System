using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagement.Services;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StudentManagement.Controllers
{
    public class StudentController : Controller
    {
        DatabaseConnectionEF connectionef;
        EmailSender emailSender;
        public StudentController(DatabaseConnectionEF connectionef, EmailSender emailSender)
        {
            this.connectionef = connectionef;
            this.emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StudentLogin(IFormCollection form)
        {
            string a = form["email"];

            var data = connectionef.admission.FirstOrDefault(x => x.email == a);
            if(data != null)
            {
                Random random = new Random();
                int otp = random.Next(100000, 999999);

                string to = a;
                string subject = "OTP for student Login from Patwa";
                string mail = "Dear, Your OTP for student login is " + otp;

                await emailSender.SendEmail(to, subject, mail);

                HttpContext.Session.SetString("otp", otp.ToString());
                HttpContext.Session.SetString("email", a);

                return RedirectToAction("VerifyLoginOTP");
            }
            else
            {
                TempData["studentlogin"] = "Your entered email is not registered";
                return RedirectToAction("Index");
            }
        }

        public IActionResult VerifyLoginOTP()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmLoginOTP(IFormCollection form)
        {
            string a = form["otpcode"];
            string b = HttpContext.Session.GetString("otp");
            string e = HttpContext.Session.GetString("email");

            if(a == b)
            {
                HttpContext.Session.SetString("studentemail", e);
                return RedirectToAction("Index", "User");
            }
            else
            {
                TempData["studentloginotp"] = "Incorrect OTP";
                return RedirectToAction("VerifyLoginOTP");
            }
        }
    }
}
