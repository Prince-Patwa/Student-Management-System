using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagement.Services;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
        EmailSender emailSender;
        DatabaseConnectionEF connectionef;
        public HomeController(EmailSender emailSender, DatabaseConnectionEF connectionef)
        {
            this.emailSender = emailSender;
            this.connectionef = connectionef;
        }
        public IActionResult Index()
        {
            //ViewBag.Title = "Home";
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(admin data)
        {
            var a = connectionef.admin.FirstOrDefault(x => x.email == data.email && x.password == data.password);
            if (a != null)
            {
                //return Content("Login Success");
                HttpContext.Session.SetString("admin", data.email);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                //return Content("Email or Password is incorrect");
                TempData["error"] = "Email Id or Password is incorrect";
                return RedirectToAction("Index");
            }
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(admin data)
        {
            var a = connectionef.admin.FirstOrDefault(x => x.email == data.email);

            if (a != null)
            {
                Random random = new Random();
                int otp = random.Next(1000, 9999);

                string to = data.email;
                string subject = "OTP for Forgot Password from Patwa";
                string mail = "Dear, Your OTP for Password recovery is " + otp;

                await emailSender.SendEmail(to, subject, mail);

                HttpContext.Session.SetString("fotp", otp.ToString());
                HttpContext.Session.SetString("femail", data.email);

                return RedirectToAction("VerifyAccount");
            }
            else
            {
                TempData["msg"] = "This email id in not registered in our Database";
                return RedirectToAction("ForgotPassword");
            }
        }

        public IActionResult VerifyAccount()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyAccount(IFormCollection form)
        {
            string a = form["otpcode"];
            string b = HttpContext.Session.GetString("fotp");

            if (a == b)
            {
                HttpContext.Session.SetString("fotpstatus", "true");
                TempData["msg"] = "OTP Verification successfully.";
                return RedirectToAction("ResetPassword");
            }
            else
            {
                TempData["msg"] = "Incorrect OTP";
                return RedirectToAction("VerifyAccount");
            }
        }

        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("fotpstatus") == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(admin newdata, IFormCollection form)
        {
            if (HttpContext.Session.GetString("fotpstatus") == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            string a = form["newpass"];
            string b = form["confirmpass"];

            if (a == b)
            {
                string email = HttpContext.Session.GetString("femail");

                // Record ko email ke basis pe find karo
                admin olddata = connectionef.admin.FirstOrDefault(x => x.email == email);

                if (olddata != null)
                {
                    olddata.password = a;  // password update
                    connectionef.admin.Update(olddata);
                    connectionef.SaveChanges();

                    TempData["msg"] = "Password updated successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["msg"] = "User not found";
                    return RedirectToAction("ResetPassword");
                }
            }
            else
            {
                TempData["msg"] = "New Password not matched with confirm password";
                return RedirectToAction("ResetPassword");
            }
        }

    }
}
