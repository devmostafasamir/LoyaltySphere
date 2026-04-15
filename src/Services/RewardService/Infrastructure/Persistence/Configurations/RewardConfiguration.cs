using LoyaltySphere.RewardService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltySphere.RewardService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Reward entity.
/// Defines table structure, indexes, and relationships.
/// </summary>
public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("rewards");

        // Primary Key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Generated in domain

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(r => r.CustomerExternalId)
            .HasColumnName("customer_external_id")
            .HasMaxLength(100)
            .IsRequired();

        // Value Objects - Points
        builder.OwnsOne(r => r.PointsAwarded, points =>
        {
            points.Property(p => p.Value)
                .HasColumnName("points_awarded")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        // Value Objects - Money
        builder.OwnsOne(r => r.TransactionAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("transaction_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(r => r.RewardType)
            .HasColumnName("reward_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Source)
            .HasColumnName("source")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.TransactionId)
            .HasColumnName("transaction_id")
            .HasMaxLength(100);

        builder.Property(r => r.CampaignId)
            .HasColumnName("campaign_id")
            .HasMaxLength(100);

        builder.Property(r => r.MerchantId)
            .HasColumnName("merchant_id")
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();

        builder.Property(r => r.IsProcessed)
            .HasColumnName("is_processed")
            .IsRequired();

        builder.Property(r => r.ProcessingError)
            .HasColumnName("processing_error")
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes for performance
        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("IX_rewards_tenant_id");

        builder.HasIndex(r => r.CustomerId)
            .HasDatabaseName("IX_rewards_customer_id");

        builder.HasIndex(r => r.CustomerExternalId)
            .HasDatabaseName("IX_rewards_customer_external_id");

        builder.HasIndex(r => new { r.TenantId, r.CustomerId, r.ProcessedAt })
            .HasDatabaseName("IX_rewards_tenant_customer_processed");

        builder.HasIndex(r => r.TransactionId)
            .HasDatabaseName("IX_rewards_transaction_id");

        builder.HasIndex(r => r.CampaignId)
            .HasDatabaseName("IX_rewards_campaign_id");

        builder.HasIndex(r => r.RewardType)
            .HasDatabaseName("IX_rewards_reward_type");

        // Relationships
        builder.HasOne(r => r.Customer)
            .WithMany(c => c.Rewards)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events (not persisted)
        builder.Ignore(r => r.DomainEvents);
    }
}
