
namespace Content.Shared.Ghost.Roles.Components;

/// <summary>
///     Added to entities to tag that they are on cooldown to spawn new creature.
/// </summary>
[RegisterComponent]
public sealed partial class SpawnCooldownComponent : Component
{
    [DataField]
    public float? Cooldown;

    [DataField]
    public float Accumulator;
}
