using Api.Features.Tags;
using Api.Features.CommissioningMarkets;
using Api.Features.FieldworkMarkets;
using Api.Features.Clients;
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
    public DbSet<Client> Clients => Set<Client>();

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

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IntegrationMetadata).HasMaxLength(1000);
            entity.Property(e => e.ProductsModules).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique(); // Ensure client names are unique
        });
    }
}
