using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualBasic;
using StudentManagement.Models;
using StudentManagement.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentManagement.Controllers
{
    //to prevent back press when user logged out
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AdminController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        { 
            base.OnActionExecuting(context);
            if (HttpContext.Session.GetString("admin") == null)
            {
                context.Result = RedirectToAction("Index", "Home");
            }
        }

        DatabaseConnectionEF connectionEF;
        EmailSender emailSender;
        SMSSender smssender;
        PaymentLink payment;
        public AdminController(DatabaseConnectionEF connectionEF, EmailSender emailSender, SMSSender smssender, PaymentLink payment)
        {
            this.connectionEF = connectionEF;
            this.emailSender = emailSender;
            this.smssender = smssender;
            this.payment = payment;
        }

        public IActionResult Index()
        {
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            var a = connectionEF.admission.Where(x => x.datetime == date && x.paymentstatus == "Success").ToList();
            ViewBag.todayadmission = a.Count;
            var b = connectionEF.admission.Where(x => x.paymentstatus == "Success").ToList();
            ViewBag.totaladmission = b.Count;

            //count today fee
            var c = connectionEF.admission.Where(x => x.datetime == date && x.paymentstatus == "Success").ToList();
            double todayfee = 0;
            foreach(var x in c)
            {
                todayfee += Convert.ToDouble(x.registrationfee);
            }
            var d = connectionEF.fee.Where(x => x.datetime == date && x.paymentstatus == "Success").ToList();
            foreach(var x in d)
            {
                todayfee += Convert.ToDouble(x.amount);
            }
            ViewBag.todayfee = todayfee;

            //total fee collection calculation
            var e = connectionEF.admission.Where(x => x.paymentstatus == "Success").ToList();
            double totalfee = 0;
            foreach (var x in e)
            {
                totalfee += Convert.ToDouble(x.registrationfee);
            }
            var f = connectionEF.fee.Where(x => x.paymentstatus == "Success").ToList();
            foreach (var x in f)
            {
                totalfee += Convert.ToDouble(x.amount);
            }
            ViewBag.totalfee = totalfee;

            //count due fee
            double totaltrainingfee = 0;
            foreach(var x in e)
            {
                totaltrainingfee += Convert.ToDouble(x.totalfee); 
            }
            double duefee = totaltrainingfee - totalfee;
            ViewBag.duefee = duefee;

            //Admin Dashboard
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult NewAdmission()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewAdmission(admission data)
        {
            data.datetime = DateTime.Now.ToString("dd/MM/yyy");
            if(data.paymentmode == "Cash")
            {
                data.paymentstatus = "Success";
                data.paymentid = "Cash";
            }
            else
            {
                data.paymentstatus = "Failed";
            }
             
            connectionEF.admission.Add(data);
            connectionEF.SaveChanges();

            HttpContext.Session.SetString("admission", data.id.ToString());

            TempData["name"] = data.name;
            TempData["email"]= data.email;
            TempData["mobile"] = data.mobile;
            TempData["amount"] = data.registrationfee;

            if (data.paymentmode == "Online")
            {
                return RedirectToAction("PayRegistrationFee");
            }
            else
            {
                return RedirectToAction("AllStudents");
            }
        }

        public IActionResult PayRegistrationFee()
        {
            return View();
        }

        public IActionResult RetryRegistrationFee(int id)
        {
            var data = connectionEF.admission.Find(id);

            HttpContext.Session.SetString("admission", data.id.ToString());

            TempData["name"] = data.name;
            TempData["email"] = data.email;
            TempData["mobile"] = data.mobile;
            TempData["amount"] = data.registrationfee;

            return RedirectToAction("PayRegistrationFee");
        }

        public IActionResult RegistrationPaymentSuccess()
        {
            string admissionid = HttpContext.Session.GetString("admission");
            string paymentid = HttpContext.Request.Query["paymentid"];

            var data = connectionEF.admission.Find(int.Parse(admissionid));
            data.paymentstatus = "Success";
            data.paymentid = paymentid;
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            connectionEF.admission.Update(data);
            connectionEF.SaveChanges();

            return View();
        }

        public IActionResult RegistrationPaymentFailed()
        {
            string admissionid = HttpContext.Session.GetString("admission");

            var data = connectionEF.admission.Find(int.Parse(admissionid));
            data.paymentstatus = "Failed";
            data.paymentid = "0";
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            connectionEF.admission.Update(data);
            connectionEF.SaveChanges();

            ViewBag.id = admissionid;
            return View();
        }

        public IActionResult AllStudents()
        {
            var data = connectionEF.admission.ToList();
            return View(data);
        }

        public IActionResult PrintRegistrationRecipt(int id)
        {
            var data = connectionEF.admission.Find(id);
            return View(data);
        }

        public IActionResult EditStudent(int id)
        {
            var data = connectionEF.admission.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult UpdateStudent(int id, admission newdata)
        {
            var olddata = connectionEF.admission.Find(id);
            olddata.name = newdata.name;
            olddata.mobile = newdata.mobile;
            olddata.email = newdata.email;
            olddata.duration = newdata.duration;
            olddata.technology = newdata.technology;
            olddata.registrationfee = newdata.registrationfee;
            olddata.totalfee = newdata.totalfee;
            olddata.qualification = newdata.qualification;
            olddata.year = newdata.year;
            olddata.college = newdata.college;

            connectionEF.admission.Update(olddata);
            connectionEF.SaveChanges();

            TempData["msg"] = "Data Updated Successfully";
            return RedirectToAction("AllStudents");
        }

        public IActionResult DeleteStudent(int id)
        {
            var data = connectionEF.admission.Find(id);
            connectionEF.admission.Remove(data);
            connectionEF.SaveChanges();
            return RedirectToAction("AllStudents");
        }

        public IActionResult NewFeePayment()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewFeePayment(fee data)
        {
            data.paymentstatus = "Failed";
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            if(data.paymentmode == "Cash")
            {
                data.paymentstatus = "Success";
                data.paymentid = "Cash";
            }

            connectionEF.fee.Add(data);
            connectionEF.SaveChanges();

            if(data.paymentmode == "Cash")
            {
                return RedirectToAction("AllFee");
            }
            else
            {
                HttpContext.Session.SetString("feeid", data.id.ToString());
                TempData["name"] = data.name;
                TempData["mobile"] = data.mobile;
                TempData["email"] = data.email;
                TempData["amount"] = data.amount;

                return RedirectToAction("PayFee");
            }
        }

        public IActionResult AllFee()
        {
            var data = connectionEF.fee.ToList();
            return View(data);
        }

        public IActionResult PayFee()
        {
            return View();
        }

        public IActionResult FeePaySuccess()
        {
            string feeid = HttpContext.Session.GetString("feeid");
            string paymentid = HttpContext.Request.Query["paymentid"];

            var data = connectionEF.fee.Find(int.Parse(feeid));
            data.paymentstatus = "Success";
            data.paymentid = paymentid;
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            connectionEF.fee.Update(data);
            connectionEF.SaveChanges();

            return View();
        }

        public IActionResult FeePayFaield()
        {
            string feeid = HttpContext.Session.GetString("feeid");

            var data = connectionEF.fee.Find(int.Parse(feeid));

            data.paymentstatus = "Failed";
            data.paymentid = "0";
            data.datetime = DateTime.Now.ToString("dd/MM/yyyy");
            connectionEF.fee.Update(data);
            connectionEF.SaveChanges();

            ViewBag.id = feeid;
            return View();
        }

        public IActionResult RetryFeePay(int id)
        {
            var data = connectionEF.fee.Find(id);

            HttpContext.Session.SetString("feeid", data.id.ToString());

            TempData["name"] = data.name;
            TempData["email"] = data.email;
            TempData["mobile"] = data.mobile;
            TempData["amount"] = data.amount;

            return RedirectToAction("PayFee");
        }

        public IActionResult PrintRecipt(int id)
        {
            var data = connectionEF.fee.Find(id);
            return View(data);
        }

        public IActionResult ShowAllEnquiry()
        {
            var enquirydata = connectionEF.enquiry.ToList();
            return View(enquirydata);
        }

        public IActionResult DeleteEnquiry(int id)
        {
            var enq = connectionEF.enquiry.Find(id);
            connectionEF.enquiry.Remove(enq);
            connectionEF.SaveChanges();
            return RedirectToAction("ShowAllEnquiry");
        }

        public IActionResult AddBatch()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddBatch(batch data)
        {
            connectionEF.batch.Add(data);
            connectionEF.SaveChanges();
            TempData["Success"] = "Batch Added Successfuly";
            return RedirectToAction("BatchList");
        }

        public IActionResult BatchList()
        {
            var data = connectionEF.batch.ToList();
            return View(data);
        }

        public IActionResult DeleteBatch(int id)
        {
            var data = connectionEF.batch.Find(id);
            connectionEF.batch.Remove(data);
            connectionEF.SaveChanges();
            TempData["Success"] = "Batch Deleted Successfuly";
            return RedirectToAction("BatchList");
        }

        public IActionResult EditBatch(int id)
        {
            var data = connectionEF.batch.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult UpdateBatch(int id, batch newdata)
        {
            var olddata = connectionEF.batch.Find(id);
            olddata.name = newdata.name;
            olddata.teacher = newdata.teacher;
            olddata.details = newdata.details;
            connectionEF.batch.Update(olddata);
            connectionEF.SaveChanges();
            return RedirectToAction("BatchList");
        }

        public IActionResult ManageStudent(int id)
        {
            var data = connectionEF.batch.Find(id);

            var data1 = connectionEF.admission.Where(x => x.paymentstatus=="Success").ToList();

            var data2 = connectionEF.batchallot.Where(x => x.batchid == id).ToList();

            List<batchallot> data3 = new List<batchallot>();
            foreach(var row in data2)
            {
                row.batch = connectionEF.batch.Find(row.batchid);
                row.admission = connectionEF.admission.Find(row.studentid);
                data3.Add(row);
            }

            managestudentpage x = new managestudentpage
            {
                batch = data,
                admission = data1,
                batchallot = data3
            };
             
            return View(x);
        }

        [HttpPost]
        public IActionResult AddStudentsToBatch(IFormCollection form)
        {
            string a = form["batchid"];
            var b = form["selectedstudent"];
            List<string> students = b.ToList();

            foreach(string sid in students)
            {
                batchallot data = new batchallot
                {
                    batchid = int.Parse(a),
                    studentid = int.Parse(sid)
                };

                connectionEF.batchallot.Add(data);
                connectionEF.SaveChanges();
            }

            return RedirectToAction("BatchList");
        }

        public IActionResult RemoveStudent(int id)
        {
            var data = connectionEF.batchallot.Find(id);
            //int batchid = data.batchid;
            connectionEF.batchallot.Remove(data);
            connectionEF.SaveChanges();
            //return RedirectToAction("ManageStudent", new { id = batchid });
            return RedirectToAction("BatchList");
        }

        public IActionResult MarkAttedance()
        {
            var data = connectionEF.batch.ToList();
            return View(data);
        }

        [HttpPost]

        public IActionResult SaveAttendance(IFormCollection form)
        {
            string a = form["batchid"];
            string b = form["selectedtstudent"];
            List<string> studentid = b.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            string today = form["date"];

            var data = connectionEF.batchallot.Where(x => x.batchid == int.Parse(a)).ToList();
            foreach (var row in data)
            {
                string remark = "Absent";
                if (studentid.Contains(row.studentid.ToString()))
                {
                    remark = "Present";
                }
                attendance x = new attendance
                {
                    batchid = int.Parse(a),
                    studentid = row.studentid,
                    remark = remark,
                    date = today
                };
                connectionEF.attendance.Add(x);
                connectionEF.SaveChanges();
            }
            return RedirectToAction("MarkAttedance");
        }

        public IActionResult ViewAttedance()
        {
            var data = connectionEF.batch.ToList();
            return View(data);
        }

        public IActionResult EmailVerificationOTP(int id)
        {
            var data = connectionEF.admission.Find(id);
            HttpContext.Session.SetString("studentid", data.id.ToString());
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailOTP(admission newdata)
        {
            int id = int.Parse(HttpContext.Session.GetString("studentid"));
            var data = connectionEF.admission.Find(id);
            string email = newdata.email;

            Random random = new Random();
            int otp = random.Next(1000, 9999);

            data.EmailOTP = otp;
            data.EmailOTPStatus = false;
            connectionEF.admission.Update(data);
            connectionEF.SaveChanges();

            string to = email;
            string subject = "OTP for Email Verification from Genius Coaching Center";
            string mail = "Dear, Your OTP for Email verification is " + otp;

            await emailSender.SendEmail(to, subject, mail);

            HttpContext.Session.SetString("emailotp", otp.ToString());
            HttpContext.Session.SetString("email", email);

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
            var data = connectionEF.admission.Find(id);

            if (a == b)
            {
                data.EmailOTPStatus = true;
                connectionEF.admission.Update(data);
                connectionEF.SaveChanges();
                return RedirectToAction("AllStudents");
            }
            else
            {
                TempData["emailotp"] = "Incorrect OTP";
                return RedirectToAction("ConfirmEmailOTP");
            }
        }

        public IActionResult FeeCollecionReport()
        {
            var data1 = connectionEF.admission.Where(x => x.paymentstatus == "Success").ToList();
            var data2 = connectionEF.fee.Where(x => x.paymentstatus == "Success").ToList();

            List<FeeCollection> feeCollection = new List<FeeCollection>();

            foreach(var row in data1)
            {
                FeeCollection fee = new FeeCollection();
                fee.name = row.name;
                fee.amount = row.registrationfee;
                fee.paymentmode = row.paymentid;
                fee.status = row.paymentstatus;
                fee.date = row.datetime;
                fee.source = "Registration Fee";

                feeCollection.Add(fee);
            }

            foreach (var row in data2)
            {
                FeeCollection fee = new FeeCollection();
                fee.name = row.name;
                fee.amount = row.amount;
                fee.paymentmode = row.paymentid;
                fee.status = row.paymentstatus;
                fee.date = row.datetime;
                fee.source = "Training Fee";

                feeCollection.Add(fee);
            }
            return View(feeCollection);
        }

        public async Task<IActionResult> MobileOTP(int id)
        {
            var mobiledata = connectionEF.admission.Find(id);
            var mobile = mobiledata.mobile;
            Random random = new Random();
            int otp = random.Next(1000, 9999);
            mobiledata.MobileOTP = otp;
            mobiledata.MobileOTPStatus = false;
            connectionEF.admission.Update(mobiledata);
            connectionEF.SaveChanges();
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
            var data = connectionEF.admission.Find(id);

            if (a == b)
            {
                data.MobileOTPStatus = true;
                connectionEF.admission.Update(data);
                connectionEF.SaveChanges();
                return RedirectToAction("AllStudents");
            }
            else
            {
                TempData["mobileotp"] = "Incorrect OTP";
                return RedirectToAction("VerifyMobileOTP");
            }
        }

        public async Task<IActionResult> SendLink(int id)
        {
            var data = connectionEF.admission.Find(id);

            string name = data.name;
            string mobile = data.mobile;
            string email = data.email;
            double fee = double.Parse(data.registrationfee);

            string callback = "http://localhost:5008/Admin/PaymentLinkStatus/"+id+"/";

            string link = await payment.CreatePaymentLink(fee, name, email, mobile, callback);

            string subject = "Payment Link for Registration Fee from Genius Coaching Center";
            string message = "Dear " + name + ", Please pay your registration fee using the following link: " + link;

            await emailSender.SendEmail(email, subject, message);

            return RedirectToAction("AllStudents");
        }

        public IActionResult PaymentLinkStatus(int id)
        {
            return Content("PaymentLink Data");
        }
    }
}
