using Argus.Sync.Data.Models;

namespace swap_reducer.Models;
public record TxBySlot(
    string TxHash,
    ulong TxIndex,
    ulong Slot,
    string Address,
    string PolicyId,
    string AssetName,
    ulong Amount,
    byte[]? RawData
) : IReducerModel;