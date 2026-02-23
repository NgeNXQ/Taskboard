using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Infrastructure.Persistence;

internal sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    private readonly CardConfig _cardConfig;

    internal CardConfiguration(CardConfig cardConfig)
    {
        _cardConfig = cardConfig;
    }

    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnOrder(0)
            .HasValueGenerator<SequentialGuidValueGenerator>();

        builder.Property(entry => entry.ListId)
            .HasColumnOrder(1)
            .IsRequired();

        builder.Property(entry => entry.Name)
            .HasColumnOrder(2)
            .IsRequired()
            .HasMaxLength(_cardConfig.MaxNameLength);

        builder.Property(entry => entry.Priority)
            .HasColumnOrder(3)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasColumnOrder(4)
            .IsRequired()
            .HasMaxLength(_cardConfig.MaxDescriptionLength);

        builder.Property(entry => entry.DueDate)
            .HasColumnOrder(5)
            .IsRequired();

        builder.Property(entry => entry.CreatedAt)
            .HasColumnOrder(6)
            .IsRequired();

        builder.Property(entry => entry.UpdatedAt)
            .HasColumnOrder(7)
            .IsRequired();

        builder.HasIndex(entry => entry.DueDate);

        builder.HasOne<List>().WithMany()
            .HasForeignKey(entry => entry.ListId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
