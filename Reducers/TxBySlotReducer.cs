using Argus.Sync.Reducers;
using Chrysalis.Cbor.Extensions.Cardano.Core;
using Chrysalis.Cbor.Extensions.Cardano.Core.Header;
using Chrysalis.Cbor.Extensions.Cardano.Core.Transaction;
using Chrysalis.Cbor.Serialization;
using Chrysalis.Cbor.Types.Cardano.Core;
using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Wallet.Models.Addresses;
using Microsoft.EntityFrameworkCore;
using swap_reducer.Extensions;
using swap_reducer.Models;

namespace swap_reducer;

public class OrderBySlotReducer(
    IDbContextFactory<SwapDbContext> dbContextFactory,
    IConfiguration configuration
) : IReducer<TxBySlot>
{
    private readonly string _orderBookScriptHash = configuration.GetValue(
        "",
        ""
    );
    public async Task RollBackwardAsync(ulong slot)
    {
        await using SwapDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IQueryable<TxBySlot> toRemove = dbContext.TxsBySlot.Where(o => o.Slot >= slot);
        dbContext.TxsBySlot.RemoveRange(toRemove);

        await dbContext.SaveChangesAsync();
    }

    public async Task RollForwardAsync(Block block)
    {
        await using SwapDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IEnumerable<TransactionBody> transactions = block.TransactionBodies();
        transactions.SelectMany(e => e.Outputs().Select((output, index) => new { Output = output, Index = (ulong)index, TxHash = e.Hash() }))
            .ToList().ForEach(entity =>
            {
                string? outputBech32Addr = null;
                try
                {
                    outputBech32Addr = new Address(entity.Output.Address()).ToBech32();
                }
                catch
                {
                    return;
                }
                if (string.IsNullOrEmpty(outputBech32Addr) || !outputBech32Addr.StartsWith("addr")) return;

                string pkh = Convert.ToHexString(new Address(entity.Output.Address()).GetPaymentKeyHash() ?? []).ToLowerInvariant();
                if (!pkh.Equals(_orderBookScriptHash, StringComparison.InvariantCultureIgnoreCase)) return;

                SwapDatum? swapDatum = null;
                try
                {
                    swapDatum = CborSerializer.Deserialize<SwapDatum>(entity.Output.Datum());
                }
                catch
                {
                    return;
                }
                if (swapDatum is null) return;

                TxBySlot newEntry = new(
                    entity.TxHash,
                    entity.Index,
                    block.Header().HeaderBody().Slot(),
                    outputBech32Addr,
                    Convert.ToHexStringLower(swapDatum.PolicyId),
                    Convert.ToHexStringLower(swapDatum.AssetName),
                    swapDatum.Amount,
                    entity.Output.Raw.HasValue ? entity.Output.Raw.Value.ToArray() : null
                );

                dbContext.TxsBySlot.Add(newEntry);
            });

        await dbContext.SaveChangesAsync();
    }
}