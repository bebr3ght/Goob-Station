using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Mind;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IsOwnerComponent : Component
{
    [DataField]
    public string StatusIcon = "OwnerFaction";
}
