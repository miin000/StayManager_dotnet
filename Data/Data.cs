using staymanager_pj.Models;
using System.Data.Entity;

namespace staymanager_pj.Data
{
    [DbConfigurationType(typeof(MySqlConfiguration))]
    public class AppDbContext : DbContext
    {
        static AppDbContext()
        {
            Database.SetInitializer(new DbInitializer());
        }

        public AppDbContext() : base("name=StayManagerDbContext")
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryImport> InventoryImports { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers").HasKey(x => x.Id);
            modelBuilder.Entity<Employee>().ToTable("Employees").HasKey(x => x.Id);
            modelBuilder.Entity<Room>().ToTable("Rooms").HasKey(x => x.Id);
            modelBuilder.Entity<Reservation>().ToTable("Reservations").HasKey(x => x.Id);
            modelBuilder.Entity<Invoice>().ToTable("Invoices").HasKey(x => x.Id);
            modelBuilder.Entity<InvoiceItem>().ToTable("InvoiceItems").HasKey(x => x.Id);
            modelBuilder.Entity<InventoryItem>().ToTable("InventoryItems").HasKey(x => x.Id);
            modelBuilder.Entity<InventoryImport>().ToTable("InventoryImports").HasKey(x => x.Id);

            modelBuilder.Entity<Customer>().Property(x => x.FullName).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<Customer>().Property(x => x.PhoneNumber).HasMaxLength(30);
            modelBuilder.Entity<Customer>().Property(x => x.Email).HasMaxLength(150);
            modelBuilder.Entity<Customer>().Property(x => x.IdentityNumber).HasMaxLength(50);
            modelBuilder.Entity<Customer>().Property(x => x.Username).HasMaxLength(80);

            modelBuilder.Entity<Employee>().Property(x => x.FullName).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<Employee>().Property(x => x.Username).HasMaxLength(80);
            modelBuilder.Entity<Employee>().Property(x => x.Position).HasMaxLength(80);

            modelBuilder.Entity<Room>().Property(x => x.RoomNumber).IsRequired().HasMaxLength(30);
            modelBuilder.Entity<Room>().Property(x => x.RoomType).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Room>().Property(x => x.Description).HasMaxLength(500);

            modelBuilder.Entity<Invoice>().Property(x => x.InvoiceCode).IsRequired().HasMaxLength(30);
            modelBuilder.Entity<InvoiceItem>().Property(x => x.ItemName).IsRequired().HasMaxLength(150);

            modelBuilder.Entity<InventoryItem>().Property(x => x.ItemCode).IsRequired().HasMaxLength(30);
            modelBuilder.Entity<InventoryItem>().Property(x => x.ItemName).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<InventoryItem>().Property(x => x.Category).HasMaxLength(80);
            modelBuilder.Entity<InventoryItem>().Property(x => x.Unit).HasMaxLength(30);
            modelBuilder.Entity<InventoryImport>().Property(x => x.ImportCode).IsRequired().HasMaxLength(30);
            modelBuilder.Entity<InventoryImport>().Property(x => x.SupplierName).HasMaxLength(150);

            base.OnModelCreating(modelBuilder);
        }
    }
}

