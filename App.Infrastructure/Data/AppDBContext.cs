using App.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System.Reflection;


namespace App.Infrastructure.Data
{
    public class AppDBContext : IdentityDbContext
    {

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
            base.Database.Migrate();
            this.ChangeTracker.LazyLoadingEnabled = false; 
        }

        public DbSet<AppUser> AppUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }


    }
}
