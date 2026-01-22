namespace Content.Shared.Bible.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HasOwnerComponent : Component
{
    /// <summary>
    /// The item this familiar was summoned from.
    /// </summary>
    [DataField]
    public EntityUid? OwnerItem = null;

    /// <summary>
    /// The mob this familiar attached to.
    /// </summary>
    [DataField]
    public EntityUid? OwnerMob = null;

    /// <summary>
    /// If true, familiar will delete when SpawnedFromItem disappear; SpawnedFromItem item will be able to spawn familiar again.
    /// </summary>
    [DataField]
    public bool HandleOwnerItem;

    [DataField]
    public bool RandomizeOwner;

    [DataField]
    public List<EntityUid> PossibleOwners;
}
