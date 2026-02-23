using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using EC.TaskBoard.Web.Shared.Configs;

namespace EC.TaskBoard.Web.Features.Board.Infrastructure.Persistence;

internal sealed class BoardDbContext : DbContext
{
    private readonly CardConfig _cardConfig;
    private readonly ListConfig _listConfig;

    public BoardDbContext(
        IOptions<CardConfig> cardConfig,
        IOptions<ListConfig> listConfig,
        DbContextOptions<BoardDbContext> options
    ) : base(options)
    {
        _cardConfig = cardConfig.Value;
        _listConfig = listConfig.Value;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ActivityLogConfiguration());
        builder.ApplyConfiguration(new CardConfiguration(_cardConfig));
        builder.ApplyConfiguration(new ListConfiguration(_listConfig));
    }
}
