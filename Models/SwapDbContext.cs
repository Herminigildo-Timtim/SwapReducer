using Argus.Sync.Data;
using Microsoft.EntityFrameworkCore;

namespace swap_reducer.Models;

public class SwapDbContext
(
    DbContextOptions<DbContext> options,
    IConfiguration configuration
) : CardanoDbContext(options, configuration)
{
    public DbSet<TxBySlot> TxsBySlot => Set<TxBySlot>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TxBySlot>(entity =>
        {
            entity.HasKey(e => new { e.Slot, e.TxHash, e.TxIndex });
            entity.HasIndex(e => e.Address);
            entity.HasIndex(e => new { e.TxHash, e.TxIndex });
        });
    }
}