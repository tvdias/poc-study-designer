using Api.Features.Tags;
using Api.Features.CommissioningMarkets;
using Api.Features.FieldworkMarkets;
using Api.Features.Modules;
using Api.Features.Clients;
using Api.Features.ConfigurationQuestions;
using Api.Features.Products;
using Api.Features.QuestionBank;
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
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ConfigurationQuestion> ConfigurationQuestions => Set<ConfigurationQuestion>();
    public DbSet<ConfigurationAnswer> ConfigurationAnswers => Set<ConfigurationAnswer>();
    public DbSet<DependencyRule> DependencyRules => Set<DependencyRule>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTemplate> ProductTemplates => Set<ProductTemplate>();
    public DbSet<ProductConfigQuestion> ProductConfigQuestions => Set<ProductConfigQuestion>();
    public DbSet<QuestionBankItem> QuestionBankItems => Set<QuestionBankItem>();
    public DbSet<QuestionAnswer> QuestionAnswers => Set<QuestionAnswer>();

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

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.HasIndex(e => e.VariableName).IsUnique();
            
            entity.HasOne(e => e.ParentModule)
                .WithMany(e => e.ChildModules)
                .HasForeignKey(e => e.ParentModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CompanyNumber).HasMaxLength(50);
            entity.Property(e => e.CustomerNumber).HasMaxLength(50);
            entity.Property(e => e.CompanyCode).HasMaxLength(50);
            entity.HasIndex(e => e.AccountName).IsUnique(); // Ensure account names are unique
        });

        modelBuilder.Entity<ConfigurationQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RuleType)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.HasMany(e => e.Answers)
                .WithOne(a => a.ConfigurationQuestion)
                .HasForeignKey(a => a.ConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConfigurationAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<DependencyRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Classification).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.QuestionBank).HasMaxLength(100);
            entity.Property(e => e.Tag).HasMaxLength(100);
            entity.Property(e => e.StatusReason).HasMaxLength(200);
            
            entity.HasOne(e => e.ConfigurationQuestion)
                .WithMany()
                .HasForeignKey(e => e.ConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.TriggeringAnswer)
                .WithMany()
                .HasForeignKey(e => e.TriggeringAnswerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.Name).IsUnique(); // Ensure product names are unique
            
            entity.HasMany(e => e.ProductTemplates)
                .WithOne(pt => pt.Product)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.ProductConfigQuestions)
                .WithOne(pcq => pcq.Product)
                .HasForeignKey(pcq => pcq.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Version).IsRequired();
            
            entity.HasIndex(e => new { e.ProductId, e.Name, e.Version }).IsUnique();
        });

        modelBuilder.Entity<ProductConfigQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StatusReason).HasMaxLength(200);
            
            entity.HasIndex(e => new { e.ProductId, e.ConfigurationQuestionId }).IsUnique();
            
            entity.HasOne(e => e.ConfigurationQuestion)
                .WithMany()
                .HasForeignKey(e => e.ConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuestionBankItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.QuestionType).HasMaxLength(100);
            entity.Property(e => e.Classification).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Methodology).HasMaxLength(100);
            
            entity.HasIndex(e => new { e.VariableName, e.Version }).IsUnique();
            
            entity.HasOne(e => e.ParentQuestion)
                .WithMany()
                .HasForeignKey(e => e.ParentQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.Answers)
                .WithOne(a => a.QuestionBankItem)
                .HasForeignKey(a => a.QuestionBankItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnswerText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AnswerCode).HasMaxLength(50);
            entity.Property(e => e.AnswerLocation).HasMaxLength(100);
        });

    }
}
