using Microsoft.EntityFrameworkCore;
using POS_v0_1.Models;

namespace POS_v0_1.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Topup> Topups { get; set; }
        public DbSet<RepairOrder> Repairs { get; set; }
        public DbSet<RepairCategory> RepairCategories { get; set; }
        public DbSet<RepairType> RepairTypes { get; set; }
        public DbSet<FinancialMovement> FinancialMovements { get; set; }
        public DbSet<FinancialSource> FinancialSources { get; set; }
        public DbSet<CashCount> CashCounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<CashSession> CashSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=pos_system.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships or seed data if needed
            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Items)
                .WithOne()
                .HasForeignKey("SaleId");
        }
    }
}
