using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;

namespace swap_reducer.Models;

[CborSerializable]
[CborConstr(0)]
public partial record SwapDatum(
    [CborOrder(0)]
    byte[] Owner,

    [CborOrder(1)]
    byte[] PolicyId,

    [CborOrder(2)]
    byte[] AssetName,

    [CborOrder(3)]
    ulong Amount
) : CborBase;