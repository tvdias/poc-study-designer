using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionAnswer> QuestionAnswers => Set<QuestionAnswer>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<ModuleQuestion> ModuleQuestions => Set<ModuleQuestion>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<CommissioningMarket> CommissioningMarkets => Set<CommissioningMarket>();
    public DbSet<FieldworkMarket> FieldworkMarkets => Set<FieldworkMarket>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTemplate> ProductTemplates => Set<ProductTemplate>();
    public DbSet<ConfigurationQuestion> ConfigurationQuestions => Set<ConfigurationQuestion>();
    public DbSet<ConfigurationQuestionAnswer> ConfigurationQuestionAnswers => Set<ConfigurationQuestionAnswer>();
    public DbSet<ProductConfigurationQuestion> ProductConfigurationQuestions => Set<ProductConfigurationQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Question
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            
            entity.HasOne(e => e.ParentQuestion)
                .WithMany(e => e.ChildQuestions)
                .HasForeignKey(e => e.ParentQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure QuestionAnswer
        modelBuilder.Entity<QuestionAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50);
            
            entity.HasOne(e => e.Question)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure QuestionTag (many-to-many)
        modelBuilder.Entity<QuestionTag>(entity =>
        {
            entity.HasKey(e => new { e.QuestionId, e.TagId });
            
            entity.HasOne(e => e.Question)
                .WithMany(e => e.QuestionTags)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Tag)
                .WithMany(e => e.QuestionTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Module
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            
            entity.HasOne(e => e.ParentModule)
                .WithMany(e => e.ChildModules)
                .HasForeignKey(e => e.ParentModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ModuleQuestion (many-to-many)
        modelBuilder.Entity<ModuleQuestion>(entity =>
        {
            entity.HasKey(e => new { e.ModuleId, e.QuestionId });
            
            entity.HasOne(e => e.Module)
                .WithMany(e => e.ModuleQuestions)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Question)
                .WithMany(e => e.ModuleQuestions)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Configure Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        // Configure CommissioningMarket
        modelBuilder.Entity<CommissioningMarket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsoCode).IsRequired().HasMaxLength(3);
            entity.HasIndex(e => e.IsoCode).IsUnique();
        });

        // Configure FieldworkMarket
        modelBuilder.Entity<FieldworkMarket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsoCode).IsRequired().HasMaxLength(3);
            entity.HasIndex(e => e.IsoCode).IsUnique();
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // Configure ProductTemplate
        modelBuilder.Entity<ProductTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            
            entity.HasOne(e => e.Product)
                .WithMany(e => e.ProductTemplates)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ConfigurationQuestion
        modelBuilder.Entity<ConfigurationQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Rule).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // Configure ConfigurationQuestionAnswer
        modelBuilder.Entity<ConfigurationQuestionAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.ConfigurationQuestion)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.ConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ProductConfigurationQuestion (many-to-many)
        modelBuilder.Entity<ProductConfigurationQuestion>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.ConfigurationQuestionId });
            
            entity.HasOne(e => e.Product)
                .WithMany(e => e.ProductConfigurationQuestions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ConfigurationQuestion)
                .WithMany(e => e.ProductConfigurationQuestions)
                .HasForeignKey(e => e.ConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                // NOTE: Authentication context integration pending. Currently defaults to "system".
                // Future: Retrieve from IHttpContextAccessor or claims principal
                entity.CreatedBy = "system";
            }
            
            if (entry.State == EntityState.Modified)
            {
                entity.ModifiedAt = DateTime.UtcNow;
                // NOTE: Authentication context integration pending. Currently defaults to "system".
                // Future: Retrieve from IHttpContextAccessor or claims principal
                entity.ModifiedBy = "system";
            }
        }
    }
}
