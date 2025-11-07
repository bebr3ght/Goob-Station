using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Ghost.Roles.Components;

/// <summary>
///     Added to mind role entities to tag that they are a familiar.
/// </summary>
[RegisterComponent]
public sealed partial class RepeatSpawnCooldownComponent : Component
{
    /// <summary>
    ///     Cooldown time before the ghost role can be spawned again.
    ///     Required when Repeatable is true (DeleteOnSpawn is false).
    /// </summary>
    [DataField]
    public float? RepeatCooldown;

    [DataField]
    public float RepeatAccumulator;
}
