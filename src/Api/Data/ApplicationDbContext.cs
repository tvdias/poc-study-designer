using Api.Features.Tags;
using Api.Features.CommissioningMarkets;
using Api.Features.FieldworkMarkets;
using Api.Features.Modules;
using Api.Features.Questions;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<CommissioningMarket> CommissioningMarkets => Set<CommissioningMarket>();
    public DbSet<FieldworkMarket> FieldworkMarkets => Set<FieldworkMarket>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<ModuleVersion> ModuleVersions => Set<ModuleVersion>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ModuleQuestion> ModuleQuestions => Set<ModuleQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique(); // Ensure tag names are unique
        });

        modelBuilder.Entity<CommissioningMarket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsoCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.IsoCode).IsUnique(); // Ensure ISO codes are unique
        });

        modelBuilder.Entity<FieldworkMarket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsoCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.IsoCode).IsUnique(); // Ensure ISO codes are unique
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.QuestionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.QuestionSource).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.VariableName).IsUnique();
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StatusReason).HasMaxLength(200);
            entity.HasIndex(e => e.VariableName).IsUnique();
            
            entity.HasOne(e => e.ParentModule)
                .WithMany(e => e.ChildModules)
                .HasForeignKey(e => e.ParentModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModuleVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeDescription).HasMaxLength(500);
            
            entity.HasOne(e => e.Module)
                .WithMany(e => e.Versions)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ModuleQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Module)
                .WithMany(e => e.ModuleQuestions)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => new { e.ModuleId, e.QuestionId }).IsUnique();
        });
    }
}
