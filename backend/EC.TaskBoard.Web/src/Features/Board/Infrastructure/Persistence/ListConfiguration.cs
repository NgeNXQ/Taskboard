using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Infrastructure.Persistence;

internal sealed class ListConfiguration : IEntityTypeConfiguration<List>
{
    private readonly ListConfig _listConfig;

    internal ListConfiguration(ListConfig listConfig)
    {
        _listConfig = listConfig;
    }

    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnOrder(0)
            .HasValueGenerator<SequentialGuidValueGenerator>();

        builder.Property(entry => entry.Name)
            .HasColumnOrder(1)
            .IsRequired()
            .HasMaxLength(_listConfig.MaxNameLength);

        builder.Property(entry => entry.CreatedAt)
            .HasColumnOrder(2)
            .IsRequired();

        builder.Property(entry => entry.UpdatedAt)
            .HasColumnOrder(3)
            .IsRequired();

        builder.HasIndex(entry => entry.CreatedAt);
    }
}
