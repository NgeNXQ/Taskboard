using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Infrastructure.Persistence;

internal sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnOrder(0)
            .HasValueGenerator<SequentialGuidValueGenerator>();

        builder.Property(entry => entry.CardId)
            .HasColumnOrder(1);

        builder.Property(entry => entry.Kind)
            .HasColumnOrder(2)
            .IsRequired();

        builder.Property(entry => entry.Current)
            .HasColumnOrder(3);

        builder.Property(entry => entry.Previous)
            .HasColumnOrder(4);

        builder.Property(entry => entry.CreatedAt)
            .HasColumnOrder(5)
            .IsRequired();

        builder.HasIndex(entry => entry.CardId);

        builder.HasIndex(entry => entry.CreatedAt);

        builder.HasOne<Card>().WithMany()
            .HasForeignKey(entry => entry.CardId)
            .OnDelete(DeleteBehavior.ClientNoAction);
    }
}
