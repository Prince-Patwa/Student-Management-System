using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        DatabaseConnectionEF connectionef;
        public UserAPIController(DatabaseConnectionEF connectionef)
        {
            this.connectionef = connectionef;
        }

        [HttpGet("batchallot")]
        public IActionResult BatchAllot()
        {
            var result = connectionef.batchallot.ToList();
            List<batchallot> allotedbatch = new List<batchallot>();
            foreach (var row in result)
            {
                row.admission = connectionef.admission.Find(row.studentid);
                row.batch = connectionef.batch.Find(row.batchid);
                allotedbatch.Add(row);
            }
            //return Ok(result);
            return Ok(allotedbatch);
        }
    }
}
