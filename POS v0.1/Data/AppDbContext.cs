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
        public DbSet<TopupCompany> TopupCompanies { get; set; }
        public DbSet<TopupPreset> TopupPresets { get; set; }
        public DbSet<RepairOrder> Repairs { get; set; }
        public DbSet<RepairStatusHistory> RepairStatusHistories { get; set; }
        public DbSet<RepairCategory> RepairCategories { get; set; }
        public DbSet<RepairType> RepairTypes { get; set; }
        public DbSet<FinancialMovement> FinancialMovements { get; set; }
        public DbSet<FinancialSource> FinancialSources { get; set; }
        public DbSet<CashCount> CashCounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<CashSession> CashSessions { get; set; }
        public DbSet<BusinessConfiguration> BusinessConfigs { get; set; }
        public DbSet<CreditNote> CreditNotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=pos_system.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar Llaves Primarias para entidades anidadas
            modelBuilder.Entity<SaleItem>().HasKey(si => si.Id);
            modelBuilder.Entity<PurchaseDetail>().HasKey(pd => pd.Id);
            modelBuilder.Entity<CashDenomination>().HasKey(cd => cd.Id);
            modelBuilder.Entity<CashSession>().HasKey(cs => cs.Id);

            // 4.2 Índices de Base de Datos para Optimización e Integridad
            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Barcode).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Pin).IsUnique();

            // Configurar relaciones y Foreign Keys (4.1)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Provider)
                .WithMany()
                .HasForeignKey(p => p.ProviderId);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Provider)
                .WithMany()
                .HasForeignKey(p => p.ProviderId);

            modelBuilder.Entity<FinancialMovement>()
                .HasOne(m => m.FinancialSource)
                .WithMany(s => s.Movements)
                .HasForeignKey(m => m.SourceId);

            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Items)
                .WithOne()
                .HasForeignKey("SaleId");

            modelBuilder.Entity<Purchase>()
                .HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey("PurchaseId");

            modelBuilder.Entity<CashCount>()
                .HasMany(c => c.Details)
                .WithOne()
                .HasForeignKey("CashCountId");
        }
    }
}
