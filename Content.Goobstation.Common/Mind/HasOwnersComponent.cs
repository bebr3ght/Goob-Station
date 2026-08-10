using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Mind;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasOwnersComponent : Component
{
    /// <summary>
    /// The item this familiar was summoned from.
    /// </summary>
    [DataField]
    public EntityUid? OwnerItem;

    /// <summary>
    /// The mob this familiar attached to.
    /// </summary>
    [DataField]
    public List<EntityUid> OwnersMob = new();

    /// <summary>
    /// If true, familiar will delete when SpawnedFromItem disappear; SpawnedFromItem item will be able to spawn familiar again.
    /// </summary>
    [DataField]
    public bool HandleOwnerItem;

    [DataField]
    public bool RandomizeOwner;

    [DataField]
    public List<EntityUid> PossibleOwners = new();
}
