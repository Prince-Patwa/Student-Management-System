using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Models
{
    public class batchallot
    {
        [Key]
        public int id { get; set; } 
        public int batchid { get; set; }
        public int studentid { get; set; }

        [NotMapped]      // This property will not be mapped to a database column for local variable use only not connect to the table
        public batch batch { get; set; }

        [NotMapped]
        public admission admission { get; set; }
    }
}
