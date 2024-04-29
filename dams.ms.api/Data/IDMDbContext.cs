using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Dams.ms.auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dams.ms.auth.Data
{
    public class IDMDbContext : IdentityDbContext<UserEntity, IdentityRole<int>, int>
    {
        private readonly IConfigurationRoot ObjConfiguration;

        public IDMDbContext(IConfigurationRoot Configuration)
        {
            ObjConfiguration = Configuration;
        }

        public IDMDbContext(DbContextOptions<IDMDbContext> options, IConfigurationRoot Configuration)
            : base(options)
        {
            ObjConfiguration = Configuration;
        }

        public virtual DbSet<UserEntity> Users { get; set; }
        public virtual DbSet<PasswordHistory> PasswordHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseSqlServer(ObjConfiguration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //User Entity//
            var user = builder.Entity<UserEntity>().ToTable("Users");
            user.Property(i => i.Id).HasColumnName("UserId");
            user.HasIndex("NormalizedUserName").HasName("UserNameIndex");
            user.HasIndex("NormalizedEmail").HasName("EmailIndex");

            builder.Ignore<IdentityUserRole<int>>();
            builder.Ignore<IdentityUserLogin<int>>();
            builder.Ignore<IdentityUserClaim<int>>();
            builder.Ignore<IdentityRole<int>>();
            builder.Ignore<IdentityUserToken<int>>();
            builder.Ignore<IdentityRoleClaim<int>>();
        }
    }
}
