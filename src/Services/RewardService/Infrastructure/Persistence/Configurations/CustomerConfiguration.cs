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
        builder.ToTable("customers");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        // Value Objects - Points
        builder.OwnsOne(c => c.PointsBalance, points =>
        {
            points.Property(p => p.Value)
                .HasColumnName("points_balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.OwnsOne(c => c.LifetimePoints, points =>
        {
            points.Property(p => p.Value)
                .HasColumnName("lifetime_points")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.Property(c => c.EnrolledAt)
            .HasColumnName("enrolled_at")
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.Tier)
            .HasColumnName("tier")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

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
