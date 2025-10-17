using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentManagement.Controllers
{
    //to prevent back press when user logged out
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UserController : Controller
    {
        DatabaseConnectionEF connectionef;
        EmailSender emailSender;
        SMSSender smssender;
        public UserController(DatabaseConnectionEF connectionef, EmailSender emailSender, SMSSender smssender)
        {
            this.connectionef = connectionef;
            this.emailSender = emailSender;
            this.smssender = smssender;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if(HttpContext.Session.GetString("studentemail") == null)
            {
                context.Result = RedirectToAction("Index", "Student");
            }
        }
        public IActionResult Index()
        {
            string a = HttpContext.Session.GetString("studentemail");
            var data = connectionef.admission.FirstOrDefault(x => x.email == a);
            //Find due fee
            int sid = data.id;
            var feedata = connectionef.fee.Find(sid);


            var data2 = connectionef.fee.Where(x => x.studentid == sid).ToList();
            int regfee = int.Parse(data.registrationfee);
            int duepaidfee = 0;
            foreach(var row in data2)
            {
                duepaidfee += int.Parse(row.amount);
            }
            int totalpaidfee = regfee + duepaidfee;
            ViewBag.totalpaidfee = totalpaidfee;
            return View(data);
        }

        public IActionResult LogoutStudent()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Student");
        }
         
        public async Task<IActionResult> MobileOTP(int id)
        {
            var mobiledata = connectionef.admission.Find(id);
            var mobile = mobiledata.mobile;
            Random random = new Random();
            int otp = random.Next(1000, 9999);
            mobiledata.MobileOTP = otp;
            mobiledata.MobileOTPStatus = false;
            connectionef.admission.Update(mobiledata);
            connectionef.SaveChanges();
            await smssender.SendOTP(otp.ToString(), mobile);

            HttpContext.Session.SetString("mobileotp", otp.ToString());
            ViewBag.id = id;
            HttpContext.Session.SetString("studentid", id.ToString());
            ViewBag.mobile = mobile;
            return RedirectToAction("VerifyMobileOTP");
        }

        public IActionResult VerifyMobileOTP()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyMobileOTP(IFormCollection form)
        {
            string a = form["mobileotp"];
            string b = HttpContext.Session.GetString("mobileotp");

            int id = int.Parse(HttpContext.Session.GetString("studentid"));
            var data = connectionef.admission.Find(id);

            if (a == b)
            {
                data.MobileOTPStatus = true;
                connectionef.admission.Update(data);
                connectionef.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["mobileotp"] = "Incorrect OTP";
                return RedirectToAction("VerifyMobileOTP");
            }
        }

        public IActionResult EmailVerificationOTP(int id)
        {
            var data = connectionef.admission.Find(id);
            HttpContext.Session.SetString("studentid", data.id.ToString());
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailOTP(admission newdata)
        {
            int id = int.Parse(HttpContext.Session.GetString("studentid"));
            var data = connectionef.admission.Find(id);
            string email = newdata.email;

            Random random = new Random();
            int otp = random.Next(1000, 9999);

            data.EmailOTP = otp;
            data.EmailOTPStatus = false;
            connectionef.admission.Update(data);
            connectionef.SaveChanges();

            string to = email;
            string subject = "OTP for Email Verification from Genius Coaching Center";
            string mail = "Dear, Your OTP for Email verification is " + otp;

            await emailSender.SendEmail(to, subject, mail);

            HttpContext.Session.SetString("emailotp", otp.ToString());
            HttpContext.Session.SetString("email", email);
            HttpContext.Session.SetString("id", data.id.ToString());

            return RedirectToAction("ConfirmEmailOTP");
        }

        public IActionResult ConfirmEmailOTP()
        {
            return View();
        }

        public IActionResult VerifyEmail(IFormCollection form, admission newdata)
        {
            string a = form["emailotp"];
            string b = HttpContext.Session.GetString("emailotp");

            int id = int.Parse(HttpContext.Session.GetString("studentid"));
            var data = connectionef.admission.Find(id);

            if (a == b)
            {
                data.EmailOTPStatus = true;
                connectionef.admission.Update(data);
                connectionef.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["emailotp"] = "Incorrect OTP";
                return RedirectToAction("ConfirmEmailOTP");
            }
        }

        public IActionResult ViewAttedance()
        {
            string email = HttpContext.Session.GetString("studentemail");
            var data = connectionef.admission.FirstOrDefault(x => x.email == email);
            var data1 = connectionef.batchallot.Where(x=>x.studentid == data.id).ToList();
            var data2 = data1.Select(x => x.batchid).ToList();
            var data3 = connectionef.batch.Where(a=>data2.Contains(a.id)).ToList();
            ViewBag.StudentID = data.id;
            return View(data3);
        }

        public IActionResult PayDueFee()
        {
            string email = HttpContext.Session.GetString("studentemail");
            var data = connectionef.admission.FirstOrDefault(x => x.email == email);
            ViewBag.sid = data.id;
            return View();
        }

        [HttpPost]
        public IActionResult PayDueFee(fee data)
        {
            data.paymentstatus = "Failed";
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            
            connectionef.fee.Add(data);
            connectionef.SaveChanges();

            HttpContext.Session.SetString("feeid", data.id.ToString());
            TempData["name"] = data.name;
            TempData["mobile"] = data.mobile;
            TempData["email"] = data.email;
            TempData["amount"] = data.amount;

            return RedirectToAction("PayFee");
        }

        public IActionResult PayFee()
        {
            return View();
        }

        public IActionResult FeePaySuccess()
        {
            string feeid = HttpContext.Session.GetString("feeid");
            string paymentid = HttpContext.Request.Query["paymentid"];

            var data = connectionef.fee.Find(int.Parse(feeid));
            data.paymentstatus = "Success";
            data.paymentid = paymentid;
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            connectionef.fee.Update(data);
            connectionef.SaveChanges();

            return View();
        }

        public IActionResult FeePayFaield()
        {
            string feeid = HttpContext.Session.GetString("feeid");

            var data = connectionef.fee.Find(int.Parse(feeid));

            data.paymentstatus = "Failed";
            data.paymentid = "0";
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            connectionef.fee.Update(data);
            connectionef.SaveChanges();

            ViewBag.id = feeid;
            return View();
        }

        public IActionResult RetryFeePay(int id)
        {
            var data = connectionef.fee.Find(id);

            HttpContext.Session.SetString("feeid", data.id.ToString());

            TempData["name"] = data.name;
            TempData["email"] = data.email;
            TempData["mobile"] = data.mobile;
            TempData["amount"] = data.amount;

            return RedirectToAction("PayFee");
        }

        public IActionResult FeeReport()
        {
            var data = connectionef.fee.ToList();
            string email = HttpContext.Session.GetString("studentemail");
            var data1 = connectionef.admission.FirstOrDefault(x => x.email == email);
            ViewBag.sid = data1.id;
            return View(data);
        }

        public IActionResult PrintRecipt(int id)
        {
            var data = connectionef.fee.Find(id);
            return View(data);
        }
    }
}
