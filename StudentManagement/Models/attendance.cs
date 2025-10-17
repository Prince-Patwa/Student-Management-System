using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Models
{
    public class attendance
    {
        [Key]
        public int id { get; set; }
        public int studentid { get; set; }
        public int batchid { get; set; }
        public string? remark { get; set; }
        public string? date { get; set; }

        [NotMapped]
        public admission admission { get; set; }
        [NotMapped]
        public batch batch { get; set; }
    }
}
