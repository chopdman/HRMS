using backend.Entities.Common;
using backend.Entities.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Travel> Travels => Set<Travel>();
        public DbSet<TravelAssignment> TravelAssignments => Set<TravelAssignment>();
        public DbSet<TravelDocument> TravelDocuments => Set<TravelDocument>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseProof> ExpenseDocuments => Set<ExpenseProof>();
        public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
        public DbSet<Notification> Notifications => Set<Notification>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Manager)
            .WithMany(u => u.DirectReports)
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Travel>()
            .HasOne(t => t.Creator)
            .WithMany(u => u.TravelsCreated)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<TravelAssignment>()
            .HasOne(a => a.Travel)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TravelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TravelAssignment>()
            .HasOne(a => a.Employee)
            .WithMany(u => u.TravelAssignments)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TravelDocument>()
            .HasOne(d => d.Travel)
            .WithMany(t => t.Documents)
            .HasForeignKey(d => d.TravelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TravelDocument>()
            .HasOne(d => d.Employee)
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TravelDocument>()
            .HasOne(d => d.Uploader)
            .WithMany(u => u.TravelDocumentsUploaded)
            .HasForeignKey(d => d.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TravelDocument>().Property(d => d.OwnerType)
            .HasConversion<string>() 
            .IsRequired();

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Travel)
            .WithMany(a => a.Expenses)
            .HasForeignKey(e => e.TravelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Reviewer)
            .WithMany(u => u.ExpensesReviewed)
            .HasForeignKey(e => e.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExpenseProof>()
            .HasOne(d => d.Expense)
            .WithMany(e => e.ProofDocuments)
            .HasForeignKey(d => d.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<UserRefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRefreshToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();
    }
        

    }
}
