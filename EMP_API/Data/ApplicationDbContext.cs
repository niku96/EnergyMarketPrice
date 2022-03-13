using EMP_API.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMP_API.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options)
                : base(options)
        {
        }
        //public DbSet<Faculty> Faculties { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}
