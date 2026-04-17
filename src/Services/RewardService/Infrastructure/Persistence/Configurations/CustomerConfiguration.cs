using LoyaltySphere.RewardService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltySphere.RewardService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Customer entity.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("TenantId")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .HasColumnName("CustomerId")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("PhoneNumber")
            .HasMaxLength(20);

        // Value Objects - Points
        builder.OwnsOne(c => c.PointsBalance, points =>
        {
            points.Property(p => p.Value)
                .HasColumnName("PointsBalance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.OwnsOne(c => c.LifetimePoints, points =>
        {
            points.Property(p => p.Value)
                .HasColumnName("LifetimePoints")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.Property(c => c.EnrolledAt)
            .HasColumnName("EnrolledAt")
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(c => c.Tier)
            .HasColumnName("Tier")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Indexes
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_customers_tenant_id");

        builder.HasIndex(c => new { c.TenantId, c.CustomerId })
            .IsUnique()
            .HasDatabaseName("IX_customers_tenant_customer_unique");

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("IX_customers_email");

        builder.HasIndex(c => c.Tier)
            .HasDatabaseName("IX_customers_tier");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_customers_is_active");

        // Relationships
        builder.HasMany(c => c.Rewards)
            .WithOne(r => r.Customer)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(c => c.DomainEvents);
    }
}
