using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class admin      
    {
        [Key]
        public int id { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
    }
}