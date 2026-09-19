using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Data;

public class AppDbContext : DbContext
{
     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Staff>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Staff_LevelOnlyForAdvisors",
                    $"\"Level\" IS NULL OR \"Role\" = {(int)StaffRole.Advisor}"));

            modelBuilder.Entity<Message>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Message_ExactlyOneAuthor",
                    "(\"ResponseByUserID\" IS NULL) <> (\"ResponseByStaffID\" IS NULL)"));
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<PendingRegistration>().HasIndex(p => p.Email).IsUnique();
            modelBuilder.Entity<EmailSend>().HasIndex(e => e.Email).IsUnique();
    }

    public DbSet<User> Users {get; set;}
    public DbSet<Ticket> Tickets {get; set;}
    public DbSet<Staff> Staff {get; set;}
    public DbSet<Message> Messages {get; set;}
    public DbSet<PendingRegistration> PendingRegistrations {get; set;}
    public DbSet<EmailSend> EmailSends {get; set;}
}
