using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Mind;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
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
    [DataField, AutoNetworkedField]
    public List<NetEntity> OwnersMob = new();

    [DataField]
    public string StatusIcon = "FamiliarIcon";
}
