using Microsoft.EntityFrameworkCore;
using MTS_API.Models;

namespace MTS_API.Data;

public class MtsDbContext : DbContext
{
    public MtsDbContext(
        DbContextOptions<MtsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();

    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProjectInquiry> ProjectInquiries =>
        Set<ProjectInquiry>();
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // USER
        // =====================================================

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(x => x.FullName)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(x => x.Role)
            .HasMaxLength(30)
            .IsRequired();

        // =====================================================
        // USER -> CLIENT (1 : 1)
        // =====================================================

        modelBuilder.Entity<Client>()
            .HasOne(x => x.User)
            .WithOne(x => x.Client)
            .HasForeignKey<Client>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        modelBuilder.Entity<Client>()
            .Property(x => x.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        // =====================================================
        // CLIENT -> PROJECTS (1 : MANY)
        // =====================================================

        modelBuilder.Entity<Project>()
            .HasOne(x => x.Client)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Project>()
            .Property(x => x.ProjectName)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Project>()
            .Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<Project>()
            .Property(x => x.ProjectAmount)
            .HasPrecision(18, 2);

        // =====================================================
        // PROJECT -> MILESTONES (1 : MANY)
        // =====================================================

        modelBuilder.Entity<ProjectMilestone>()
            .HasOne(x => x.Project)
            .WithMany(x => x.Milestones)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectMilestone>()
            .Property(x => x.MilestoneName)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<ProjectMilestone>()
            .Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        // =====================================================
        // CLIENT -> QUOTES (1 : MANY)
        // =====================================================

        modelBuilder.Entity<Quote>()
            .HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // PROJECT -> QUOTES (1 : MANY)
        // =====================================================

        modelBuilder.Entity<Quote>()
            .HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // QUOTE
        // =====================================================

        modelBuilder.Entity<Quote>()
            .HasIndex(x => x.QuoteNumber)
            .IsUnique();

        modelBuilder.Entity<Quote>()
            .Property(x => x.QuoteNumber)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<Quote>()
            .Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Quote>()
            .Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        modelBuilder.Entity<Quote>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);
        // =====================================================
        // Message
        // =====================================================
        modelBuilder.Entity<Message>()
        .HasOne(x => x.SenderUser)
        .WithMany()
        .HasForeignKey(x => x.SenderUserId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.ReceiverUser)
            .WithMany()
            .HasForeignKey(x => x.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Message>()
            .Property(x => x.Subject)
            .HasMaxLength(200);

        modelBuilder.Entity<Message>()
            .Property(x => x.MessageText)
            .HasMaxLength(5000)
            .IsRequired();

 
        // Documents
        modelBuilder.Entity<Document>()
            .HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Document>()
            .HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Document>()
            .HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<Document>()
            .Property(x => x.StoredFileName)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<Document>()
            .Property(x => x.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        modelBuilder.Entity<Document>()
            .Property(x => x.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        // =====================================================
        // PROJECT INQUIRY
        // =====================================================

        modelBuilder.Entity<ProjectInquiry>()
            .HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.ContactName)
            .HasMaxLength(150);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.ContactEmail)
            .HasMaxLength(150);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.ContactPhone)
            .HasMaxLength(30);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.CompanyName)
            .HasMaxLength(200);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.ServiceRequired)
            .HasMaxLength(100);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.ProjectName)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.ProjectType)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.Budget)
            .HasMaxLength(100);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.Description)
            .IsRequired();

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.Requirements)
            .IsRequired();

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.Technologies)
            .HasMaxLength(1000);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.AdditionalRequirements)
            .HasMaxLength(3000);

        modelBuilder.Entity<ProjectInquiry>()
            .Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

    }
}