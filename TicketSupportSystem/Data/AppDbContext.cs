using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Data;

public class AppDbContext : IdentityDbContext<Staff, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Staff>().ToTable("Staff");
            modelBuilder.Entity<Message>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Message_ExactlyOneAuthor",
                    "(\"ResponseByUserID\" IS NULL) <> (\"ResponseByStaffID\" IS NULL)"));
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<PendingRegistration>().HasIndex(p => p.Email).IsUnique();
            modelBuilder.Entity<EmailSend>().HasIndex(e => new { e.Email, e.Purpose }).IsUnique();
            modelBuilder.Entity<PendingLogin>().HasIndex(e => e.UserID).IsUnique();
            modelBuilder.Entity<PendingReset>().HasIndex(p => p.TokenHash).IsUnique();
            modelBuilder.Entity<PendingReset>().HasIndex(p => p.UserID).IsUnique();
            modelBuilder.Entity<Category>().HasData(
                    new Category { CategoryID = 1, Name = "General Question", HandledByLevel = Level.Level1,
                                DefaultPriority = TicketPriority.Standard, IsEnabled = true, SortOrder = 1 },
                    new Category { CategoryID = 2, Name = "Billing", HandledByLevel = Level.Level1,
                                DefaultPriority = TicketPriority.Standard, IsEnabled = true, SortOrder = 2 },
                    new Category { CategoryID = 3, Name = "Account", HandledByLevel = Level.Level2,
                                DefaultPriority = TicketPriority.Standard, IsEnabled = true, SortOrder = 3 },
                    new Category { CategoryID = 4, Name = "Product", HandledByLevel = Level.Level2,
                                DefaultPriority = TicketPriority.Standard, IsEnabled = true, SortOrder = 4 },
                    new Category { CategoryID = 5, Name = "Feedback / Complaint", HandledByLevel = Level.Level1,
                                DefaultPriority = TicketPriority.Low, IsEnabled = true, SortOrder = 6 },
                    new Category { CategoryID = 6, Name = "Sales", HandledByLevel = Level.Level1,
                                DefaultPriority = TicketPriority.Standard, IsEnabled = true, SortOrder = 5 });
            modelBuilder.Entity<Ticket>().HasOne(t => t.Category).WithMany().HasForeignKey(t => t.CategoryID).OnDelete(DeleteBehavior.Restrict);
;
    }
    public DbSet<User> Users {get; set;}
    public DbSet<Ticket> Tickets {get; set;}
    public DbSet<Staff> Staff {get; set;}
    public DbSet<Message> Messages {get; set;}
    public DbSet<PendingRegistration> PendingRegistrations {get; set;}
    public DbSet<EmailSend> EmailSends {get; set;}
    public DbSet<PendingLogin> PendingLogins {get; set;}
    public DbSet<PendingReset> PendingResets {get; set;}
    public DbSet<Category> Categories {get; set;}
}
