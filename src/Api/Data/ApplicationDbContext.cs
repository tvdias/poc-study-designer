using Api.Features.Tags;
using Api.Features.CommissioningMarkets;
using Api.Features.FieldworkMarkets;
using Api.Features.Modules;
using Api.Features.Clients;
using Api.Features.ConfigurationQuestions;
using Api.Features.Products;
using Api.Features.QuestionBank;
using Api.Features.MetricGroups;
using Api.Features.Projects;
using Api.Features.QuestionnaireLines;
using Api.Features.ManagedLists;
using Api.Features.Studies;
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
    public DbSet<ModuleQuestion> ModuleQuestions => Set<ModuleQuestion>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ConfigurationQuestion> ConfigurationQuestions => Set<ConfigurationQuestion>();
    public DbSet<ConfigurationAnswer> ConfigurationAnswers => Set<ConfigurationAnswer>();
    public DbSet<DependencyRule> DependencyRules => Set<DependencyRule>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTemplate> ProductTemplates => Set<ProductTemplate>();
    public DbSet<ProductTemplateLine> ProductTemplateLines => Set<ProductTemplateLine>();
    public DbSet<ProductConfigQuestion> ProductConfigQuestions => Set<ProductConfigQuestion>();
    public DbSet<ProductConfigQuestionDisplayRule> ProductConfigQuestionDisplayRules => Set<ProductConfigQuestionDisplayRule>();
    public DbSet<QuestionBankItem> QuestionBankItems => Set<QuestionBankItem>();
    public DbSet<QuestionAnswer> QuestionAnswers => Set<QuestionAnswer>();
    public DbSet<MetricGroup> MetricGroups => Set<MetricGroup>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<QuestionnaireLine> QuestionnaireLines => Set<QuestionnaireLine>();
    public DbSet<ManagedList> ManagedLists => Set<ManagedList>();
    public DbSet<ManagedListItem> ManagedListItems => Set<ManagedListItem>();
    public DbSet<QuestionManagedList> QuestionManagedLists => Set<QuestionManagedList>();
    public DbSet<SubsetDefinition> SubsetDefinitions => Set<SubsetDefinition>();
    public DbSet<SubsetMembership> SubsetMemberships => Set<SubsetMembership>();
    public DbSet<QuestionSubsetLink> QuestionSubsetLinks => Set<QuestionSubsetLink>();
    public DbSet<Study> Studies => Set<Study>();
    public DbSet<StudyQuestionnaireLine> StudyQuestionnaireLines => Set<StudyQuestionnaireLine>();
    public DbSet<StudyManagedListAssignment> StudyManagedListAssignments => Set<StudyManagedListAssignment>();
    public DbSet<StudyQuestionSubsetLink> StudyQuestionSubsetLinks => Set<StudyQuestionSubsetLink>();

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
            
            entity.HasMany(e => e.ModuleQuestions)
                .WithOne(mq => mq.Module)
                .HasForeignKey(mq => mq.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ModuleQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ModuleId, e.QuestionBankItemId }).IsUnique();
            
            entity.HasOne(e => e.QuestionBankItem)
                .WithMany(e => e.ModuleQuestions)
                .HasForeignKey(e => e.QuestionBankItemId)
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
            
            entity.HasMany(e => e.ProductTemplateLines)
                .WithOne(ptl => ptl.ProductTemplate)
                .HasForeignKey(ptl => ptl.ProductTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductTemplateLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.Module)
                .WithMany()
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.QuestionBankItem)
                .WithMany()
                .HasForeignKey(e => e.QuestionBankItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductConfigQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.ProductId, e.ConfigurationQuestionId }).IsUnique();
            
            entity.HasOne(e => e.ConfigurationQuestion)
                .WithMany()
                .HasForeignKey(e => e.ConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.DisplayRules)
                .WithOne(dr => dr.ProductConfigQuestion)
                .HasForeignKey(dr => dr.ProductConfigQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductConfigQuestionDisplayRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayCondition).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.TriggeringConfigurationQuestion)
                .WithMany()
                .HasForeignKey(e => e.TriggeringConfigurationQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.TriggeringAnswer)
                .WithMany()
                .HasForeignKey(e => e.TriggeringAnswerId)
                .OnDelete(DeleteBehavior.SetNull);
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

            entity.HasOne(e => e.MetricGroup)
                .WithMany()
                .HasForeignKey(e => e.MetricGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.QuestionText);
            entity.HasIndex(e => e.QuestionTitle);
        });

        modelBuilder.Entity<QuestionAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AnswerText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AnswerCode).HasMaxLength(50);
            entity.Property(e => e.AnswerLocation).HasMaxLength(100);
        });

        modelBuilder.Entity<MetricGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique(); // Ensure metric group names are unique
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Owner).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.Methodology)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique(); // Ensure project names are unique
            
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.CommissioningMarket)
                .WithMany()
                .HasForeignKey(e => e.CommissioningMarketId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuestionnaireLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.ToTable("QuestionnaireLines");
            
            // Unique index only when QuestionBankItemId is not null (prevents duplicate imports from question bank)
            entity.HasIndex(e => new { e.ProjectId, e.QuestionBankItemId })
                .IsUnique()
                .HasFilter("\"QuestionBankItemId\" IS NOT NULL");
            
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.QuestionBankItem)
                .WithMany()
                .HasForeignKey(e => e.QuestionBankItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Optional relationship
        });

        modelBuilder.Entity<ManagedList>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            // Unique name within a project
            entity.HasIndex(e => new { e.ProjectId, e.Name }).IsUnique();
            
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Items)
                .WithOne(i => i.ManagedList)
                .HasForeignKey(i => i.ManagedListId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.QuestionAssignments)
                .WithOne(qa => qa.ManagedList)
                .HasForeignKey(qa => qa.ManagedListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ManagedListItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            // Unique constraint: Value (Code) must be unique within the same ManagedList (case-insensitive)
            entity.HasIndex(e => new { e.ManagedListId, e.Value }).IsUnique();
        });

        modelBuilder.Entity<QuestionManagedList>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one managed list per question
            entity.HasIndex(e => new { e.QuestionnaireLineId, e.ManagedListId }).IsUnique();
            
            entity.HasOne(e => e.QuestionnaireLine)
                .WithMany()
                .HasForeignKey(e => e.QuestionnaireLineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubsetDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.SignatureHash).IsRequired().HasMaxLength(64);
            
            // Unique index on (ProjectId, ManagedListId, SignatureHash) for reuse detection
            entity.HasIndex(e => new { e.ProjectId, e.ManagedListId, e.SignatureHash }).IsUnique();
            
            // Index for efficient lookup
            entity.HasIndex(e => new { e.ProjectId, e.ManagedListId });
            
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ManagedList)
                .WithMany()
                .HasForeignKey(e => e.ManagedListId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Memberships)
                .WithOne(m => m.SubsetDefinition)
                .HasForeignKey(m => m.SubsetDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.QuestionLinks)
                .WithOne(ql => ql.SubsetDefinition)
                .HasForeignKey(ql => ql.SubsetDefinitionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SubsetMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one item can only be in a subset once
            entity.HasIndex(e => new { e.SubsetDefinitionId, e.ManagedListItemId }).IsUnique();
            
            entity.HasOne(e => e.ManagedListItem)
                .WithMany()
                .HasForeignKey(e => e.ManagedListItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionSubsetLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one subset link per question per managed list
            entity.HasIndex(e => new { e.QuestionnaireLineId, e.ManagedListId }).IsUnique();
            
            entity.HasOne(e => e.QuestionnaireLine)
                .WithMany()
                .HasForeignKey(e => e.QuestionnaireLineId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ManagedList)
                .WithMany()
                .HasForeignKey(e => e.ManagedListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Study>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.StatusReason).HasMaxLength(500);
            entity.Property(e => e.VersionComment).HasMaxLength(1000);
            entity.Property(e => e.VersionReason).HasMaxLength(1000);
            
            // Unique constraint: one version number per project study lineage
            entity.HasIndex(e => new { e.ProjectId, e.MasterStudyId, e.VersionNumber }).IsUnique();
            
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.MasterStudy)
                .WithMany()
                .HasForeignKey(e => e.MasterStudyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ParentStudy)
                .WithMany(e => e.ChildVersions)
                .HasForeignKey(e => e.ParentStudyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudyQuestionnaireLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariableName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.QuestionText).HasMaxLength(4000);
            entity.Property(e => e.QuestionTitle).HasMaxLength(500);
            entity.Property(e => e.QuestionType).HasMaxLength(100);
            entity.Property(e => e.Classification).HasMaxLength(100);
            entity.Property(e => e.QuestionRationale).HasMaxLength(2000);
            entity.Property(e => e.ScraperNotes).HasMaxLength(2000);
            entity.Property(e => e.CustomNotes).HasMaxLength(2000);
            entity.Property(e => e.QuestionFormatDetails).HasMaxLength(1000);
            
            // Unique constraint: one question per study per sort order
            entity.HasIndex(e => new { e.StudyId, e.SortOrder }).IsUnique();
            
            entity.HasOne(e => e.Study)
                .WithMany(s => s.QuestionnaireLines)
                .HasForeignKey(e => e.StudyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.QuestionBankItem)
                .WithMany()
                .HasForeignKey(e => e.QuestionBankItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudyManagedListAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one managed list per question per study
            entity.HasIndex(e => new { e.StudyQuestionnaireLineId, e.ManagedListId }).IsUnique();
            
            entity.HasOne(e => e.Study)
                .WithMany()
                .HasForeignKey(e => e.StudyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.StudyQuestionnaireLine)
                .WithMany(q => q.ManagedListAssignments)
                .HasForeignKey(e => e.StudyQuestionnaireLineId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ManagedList)
                .WithMany()
                .HasForeignKey(e => e.ManagedListId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudyQuestionSubsetLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one subset link per question per managed list in study
            entity.HasIndex(e => new { e.StudyQuestionnaireLineId, e.ManagedListId }).IsUnique();
            
            entity.HasOne(e => e.Study)
                .WithMany()
                .HasForeignKey(e => e.StudyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.StudyQuestionnaireLine)
                .WithMany(q => q.SubsetLinks)
                .HasForeignKey(e => e.StudyQuestionnaireLineId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ManagedList)
                .WithMany()
                .HasForeignKey(e => e.ManagedListId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.SubsetDefinition)
                .WithMany()
                .HasForeignKey(e => e.SubsetDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
