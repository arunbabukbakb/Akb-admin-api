using Models;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Models.Enums;

namespace Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {

        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<Menu>()
                .HasOne(m => m.ParentMenu)
                .WithMany(m => m.ChildMenus)
                .HasForeignKey(m => m.ParentMenuId)
                .OnDelete(DeleteBehavior.Restrict); // Adjust the delete behavior as needed

            modelBuilder.Entity<UserLog>()
                .Property(log => log.Timestamp)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Company>().HasData(
                new Company { Id = 1, Name = "Company1", Code = "COM1", Prefix = "AA", Suffix = "WW" },
                new Company { Id = 2, Name = "Manager", Code = "COM2", Prefix = "BB", Suffix = "LL" }
            );
            // Seed admin user
            modelBuilder.Entity<Users>().HasData(
                new Users
                {
                    Id = 1,
                    UserName = "admin",
                    Password = HashPassword("123"),
                    NickName = "Admin",
                    RoleId = 1,
                    CompanyId = 1,
                    Status = true
                }, new Users
                {
                    Id = 2,
                    UserName = "manager",
                    Password = HashPassword("123"),
                    NickName = "Manager",
                    RoleId = 2,
                    CompanyId = 2,
                    Status = true
                }, new Users
                {
                    Id = 3,
                    UserName = "user",
                    Password = HashPassword("123"),
                    NickName = "User",
                    RoleId = 3,
                    CompanyId = 2,
                    Status = true
                }
            );
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", UserType = UserTypes.admin, CodeName = "admin" },
                new Role { Id = 2, Name = "Manager", UserType = UserTypes.manager, CodeName = "manager" },
                new Role { Id = 3, Name = "User", UserType = UserTypes.user, CodeName = "user" }
            );

            int companyid = 2;
            int masterid = 3;
            int customerid = 4;
            int userid = 5;
            int settingid = 6;

            modelBuilder.Entity<Menu>().HasData(
                //Dashboard
                new Menu { Id = 1, Title = "Home", Path = "/", Icon = "fa fa-home", OrderNumber = 1, IsParent = true, Status = true },
                //Parent menus
                new Menu { Id = companyid, Title = "Company", Path = "", Icon = "fa fa-building", OrderNumber = 2, IsParent = true, Status = true },
                new Menu { Id = masterid, Title = "Masters", Path = "", Icon = "fa fa-bars", OrderNumber = 3, IsParent = true, Status = true },
                new Menu { Id = customerid, Title = "Customer", Path = "", Icon = "fa fa-handshake", OrderNumber = 4, IsParent = true, Status = true },
                new Menu { Id = userid, Title = "Users", Path = "", Icon = "fa fa-users", OrderNumber = 5, IsParent = true, Status = true },
                new Menu { Id = settingid, Title = "Settings", Path = "", Icon = "fa fa-cogs", OrderNumber = 6, IsParent = true, Status = true },
                //company childs
                new Menu { Id = 7, Title = "Company", Path = "company", Icon = "", OrderNumber = 1, IsParent = false, ParentMenuId = companyid, Status = true },
                new Menu { Id = 8, Title = "Branch", Path = "branch", Icon = "", OrderNumber = 2, IsParent = false, ParentMenuId = companyid, Status = true },

                //master childs
                new Menu { Id = 9, Title = "Category", Path = "category", Icon = "", OrderNumber = 4, IsParent = false, ParentMenuId = masterid, Status = true },

                //customer childs
                new Menu { Id = 10, Title = "Customer", Path = "customer", Icon = "", OrderNumber = 1, IsParent = false, ParentMenuId = customerid, Status = true },

                //user childs
                new Menu { Id = 11, Title = "Users", Path = "user", Icon = "", OrderNumber = 1, IsParent = false, ParentMenuId = userid, Status = true },
                new Menu { Id = 12, Title = "Role", Path = "role", Icon = "", OrderNumber = 2, IsParent = false, ParentMenuId = userid, Status = true },
                new Menu { Id = 13, Title = "User Branch", Path = "userbranch", Icon = "", OrderNumber = 3, IsParent = false, ParentMenuId = userid, Status = true },

                //settings childs
                new Menu { Id = 14, Title = "Menu", Path = "menu", OrderNumber = 1, IsParent = false, ParentMenuId = settingid, Status = true },
                new Menu { Id = 15, Title = "Permission", Path = "permission", OrderNumber = 2, IsParent = false, ParentMenuId = settingid, Status = true }

            );

            modelBuilder.Entity<MenuPermission>()
                .HasOne(o => o.Role)
                .WithMany()
                .HasForeignKey(o => o.RoleId);

            modelBuilder.Entity<Users>()
                .HasOne(o => o.Role)
                .WithMany()
                .HasForeignKey(o => o.RoleId);

            modelBuilder.Entity<Users>()
               .HasOne(ub => ub.Company)
               .WithMany()
               .HasForeignKey(ub => ub.CompanyId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserBranch>()
            .HasKey(ub => new { ub.UserId, ub.BranchId });

            modelBuilder.Entity<UserBranch>()
                .HasOne(ub => ub.Branch)
                .WithMany()
                .HasForeignKey(ub => ub.BranchId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserBranch>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBranches)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasIndex(o => o.BranchId);
        }


        //Company Data
        public DbSet<Company> Company { get; set; }
        public DbSet<Branch> Branch { get; set; }

        //Users
        public DbSet<Users> Users { get; set; }
        public DbSet<UserBranch> UserBranches { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuPermission> MenuPermissions { get; set; }
        public DbSet<UserLog> UserLog { get; set; }

        //Customer 
        public DbSet<Customer> Customers { get; set; }

        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt;
            //new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);
            RandomNumberGenerator.Create().GetBytes(salt = new byte[SaltSize]);

            // Create a hash using PBKDF2
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Combine the salt and hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert the byte array to a base64-encoded string
            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }

        //Update modified on default
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
