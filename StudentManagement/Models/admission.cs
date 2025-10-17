using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Models
{
    public class admission
    {
        [Key]
        public int id {  get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string fathername { get; set; }
        public string email { get; set; }
        public string duration { get; set; }
        public string technology { get; set; }
        public string registrationfee { get; set; }
        public string totalfee { get; set; }
        public string qualification { get; set; }
        public string year { get; set; }
        public string college {  get; set; }
        public string? paymentstatus { get; set; }  //? tell the feild is may be null
        public string? paymentid { get; set; }
        public string? datetime { get; set; }

        [NotMapped]
        public string? paymentmode { get; set; }

        [NotMapped]
        public double duefee { get; set; }

        [NotMapped]
        public double paidfee { get; set; }

        public int? EmailOTP { get; set; }
        public bool? EmailOTPStatus { get; set; }

        public int? MobileOTP { get; set; }
        public bool? MobileOTPStatus { get; set; }
    }
}
