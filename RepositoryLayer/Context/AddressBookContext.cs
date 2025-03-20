using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.Context
{
    public class AddressBookContext : DbContext

    {

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<AddressEntity> AddressBooks { get; set; }
        public AddressBookContext(DbContextOptions<AddressBookContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AddressEntity>()
                .HasOne(a => a.User)
                .WithMany(u => u.AddressBooks)
                .HasForeignKey(a => a.UserEmail)
                .HasPrincipalKey(u => u.Email);
        }
    }
}
