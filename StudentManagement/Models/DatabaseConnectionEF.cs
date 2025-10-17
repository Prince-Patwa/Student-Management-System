using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Models
{
    public class DatabaseConnectionEF : DbContext
    {
        public DatabaseConnectionEF(DbContextOptions options) : base(options)
        {
        }

        public DbSet<admin> admin { get; set; }

        public DbSet<admission> admission { get; set; }

        public DbSet<fee> fee { get; set; }

        public DbSet<enquiry> enquiry { get; set; }

        public DbSet<batch> batch { get; set; }

        public DbSet<batchallot> batchallot { get; set;}

        public DbSet<attendance> attendance { get; set; }

    }
}
