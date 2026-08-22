using Microsoft.EntityFrameworkCore;
using BudgetApp.Models;

namespace BudgetApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FinancialEvent> FinancialEvents => Set<FinancialEvent>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<Category> Categories => Set<Category>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---- User (1) -> Accounts (many) ----
        // Accounts is a private collection on User, so we configure the relationship
        // from the Account side (which has a public User navigation) and tell EF
        // the inverse collection's name as a string, since there's no public lambda for it.
        modelBuilder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany("Accounts")
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<User>()
            .Navigation("Accounts")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---- User (1) -> Budget (1) ----
        modelBuilder.Entity<Budget>()
            .HasOne(b => b.User)
            .WithOne(u => u.Budget)
            .HasForeignKey<Budget>(b => b.UserId);

        // ---- Account (1) -> FinancialEvents (many) ----
        modelBuilder.Entity<FinancialEvent>()
            .HasOne(fe => fe.Account)
            .WithMany("FinancialEvents")
            .HasForeignKey(fe => fe.AccountId);

        modelBuilder.Entity<Account>()
            .Navigation("FinancialEvents")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---- FinancialEvent -> Category (optional - null for transfers) ----
        modelBuilder.Entity<FinancialEvent>()
            .HasOne(fe => fe.Category)
            .WithMany()
            .HasForeignKey(fe => fe.CategoryId)
            .IsRequired(false);

        // ---- Budget (1) -> BudgetItems (many) ----
        // Items is a private collection on Budget, same pattern as Accounts above.
        modelBuilder.Entity<BudgetItem>()
            .HasOne(bi => bi.Budget)
            .WithMany("Items")
            .HasForeignKey(bi => bi.BudgetId);

        modelBuilder.Entity<Budget>()
            .Navigation("Items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---- BudgetItem -> Category (required) ----
        modelBuilder.Entity<BudgetItem>()
            .HasOne(bi => bi.Category)
            .WithMany()
            .HasForeignKey(bi => bi.CategoryId)
            .IsRequired();
    }
}