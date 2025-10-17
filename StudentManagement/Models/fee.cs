using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Models
{
    public class fee
    {
        [Key]
        public int id { get; set; }
        public int studentid { get; set; }
        public string name { get; set; }    
        public string email { get; set; }
        public string mobile { get; set; }
        public string college { get; set; }
        public string course { get; set; }
        public string year { get; set; }
        public string training { get; set; }
        public string regfee { get; set; }
        public string totalfee { get; set; }
        public string? duefee { get; set; }  
        public string amount { get; set; }
        public string remark { get; set; }
        public string datetime { get; set; }
        public string paymentstatus { get; set; }
        public string? paymentid { get; set; }
        [NotMapped]
        public string paymentmode { get; set; }
    }
}
