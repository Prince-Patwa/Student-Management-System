using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StudentManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAPIController : ControllerBase
    {
        DatabaseConnectionEF connectionef;
        public AdminAPIController(DatabaseConnectionEF connectionef)
        {
            this.connectionef = connectionef;
        }

        [HttpGet("findstudent/{id}")]
        public IActionResult FindStudentById(int id)
        {
            var data = connectionef.admission.Find(id);

            double regfee = 0;
            if (data.paymentstatus == "Success")
            {
                regfee = Convert.ToDouble(data.registrationfee);
            }

            var a = connectionef.fee.Where(x => x.studentid == id && x.paymentstatus == "Success").ToList();
            double fees = 0;
            if (a != null)
            {
                foreach (var x in a)
                {
                    fees += Convert.ToDouble(x.amount);
                }
            }
            double totalpaidfee = regfee + fees;
            double totalduefee = Convert.ToDouble(data.totalfee) - totalpaidfee;

            data.duefee = totalduefee;
            data.paidfee = totalpaidfee;

            return Ok(data);
        }

        [HttpGet("studentsinbatch/{id}")]
        public IActionResult StudentsInBatch(int id)
        {
            var data = connectionef.batchallot.Where(x => x.batchid == id).ToList();
            List<batchallot> Students = new List<batchallot>();
            foreach (var row in data)
            {
                row.admission = connectionef.admission.Find(row.studentid);
                row.batch = connectionef.batch.Find(row.batchid);
                Students.Add(row);

            }
            return Ok(Students);
        }

        [HttpGet("WeeklyAdmission")]
        public IActionResult WecklyAdmission()
        {
            string day1 = DateTime.Now.ToString("dd/MM/yyyy");
            string day2 = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy");
            string day3 = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy");
            string day4 = DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy");
            string day5 = DateTime.Now.AddDays(-4).ToString("dd/MM/yyyy");
            string day6 = DateTime.Now.AddDays(-5).ToString("dd/MM/yyyy");
            string day7 = DateTime.Now.AddDays(-6).ToString("dd/MM/yyyy");

            int count1 = connectionef.admission.Where(x => x.datetime == day1).Count();
            int count2 = connectionef.admission.Where(x => x.datetime == day2).Count();
            int count3 = connectionef.admission.Where(x => x.datetime == day3).Count();
            int count4 = connectionef.admission.Where(x => x.datetime == day4).Count();
            int count5 = connectionef.admission.Where(x => x.datetime == day5).Count();
            int count6 = connectionef.admission.Where(x => x.datetime == day7).Count();
            int count7 = connectionef.admission.Where(x => x.datetime == day7).Count();

            string[] dates = { day1, day2, day3, day4, day5, day6, day7 };
            int[] counts = { count1, count2, count3, count4, count5, count6, count7 };

            chart1 chart = new chart1
            {
                dates = dates,
                counts = counts
            };
            return Ok(chart);


        }

        [HttpPost("dailyattendance")]
        public IActionResult DailyAttendance([FromForm] DailyAttendanceModel data)
        {
            var x = connectionef.attendance.Where(a => a.batchid == int.Parse(data.batchid) && a.date == data.date).ToList();

            List<attendance> myattendance = new List<attendance>();
            foreach (var row in x)
            {
                row.admission = connectionef.admission.Find(row.studentid);
                row.batch = connectionef.batch.Find(row.batchid);
                myattendance.Add(row);
            }

            return Ok(myattendance);
        }

        [HttpPost("monthlyattendance")]
        public string MonthlyAttendance([FromForm] MonthlyAttendanceModel data)
        {
            string batchid = data.batchid;
            string month = data.month;
            string year = data.year;

            int DaysInMonth = DateTime.DaysInMonth(int.Parse(year), int.Parse(month));

            var data1 = connectionef.batchallot.Where(x => x.batchid == int.Parse(batchid)).ToList();
            List<batchallot> students = new List<batchallot>();
            foreach (var row in data1)
            {
                row.admission = connectionef.admission.Find(row.studentid);
                row.batch = connectionef.batch.Find(row.batchid);
                students.Add(row);
            }

            string table = "<table class='table table-bordered table-hover' >";

            string record = "<tr>";
            for (int i = 0; i <= DaysInMonth; i++)
            {
                if (i == 0)
                {
                    record += "<th>Student Name</th>";
                }
                else
                {
                    record += "<th>" + i + "</th>";
                }
            }
            record += "</tr>";
            table += record;

            foreach (var row in students)
            {
                record = "<tr>";
                record += "<td>" + row.admission.name + "</td>";
                for (int i = 1; i <= DaysInMonth; i++)
                {

                    string date = year + "-" + getTwoDigit(int.Parse(month)) + "-" + getTwoDigit(i);

                    var data2 = connectionef.attendance.FirstOrDefault(x => x.studentid == row.studentid && x.date == date && x.batchid == row.batchid);

                    if (data2 != null)
                    {

                        string remark = data2.remark;
                        if (remark == "Present")
                        {
                            record += "<td class='text-success' >P</td>";
                        }
                        else
                        {
                            record += "<td class='text-danger' >A</td>";
                        }
                    }
                    else
                    {
                        record += "<td>-</td>";
                    }

                }

                record += "</tr>";
                table += record;
            }

            table += "</table>";


            return table;
        }

        public string getTwoDigit(int num)
        {
            if (num < 10)
            {
                return "0" + num;
            }
            else
            {
                return num.ToString();
            }
        }
    }

    //create a new class model "This is second option to create model"
    public class DailyAttendanceModel
    {
        public string batchid { get; set; }
        public string date { get; set; }
    }

    public class MonthlyAttendanceModel 
    {
        public string batchid { get; set; }
        public string month { get; set; }
        public string year { get; set; }
    }

    public class chart1
    {
        public string[] dates { get; set; }
        public int[] counts { get; set; }
    }
}
