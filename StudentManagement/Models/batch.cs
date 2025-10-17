using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class batch
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string teacher { get; set; }
        public string details { get; set; }
    }
}
