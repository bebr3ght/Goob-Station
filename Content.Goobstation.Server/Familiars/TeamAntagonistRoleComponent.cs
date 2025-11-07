using Content.Shared.Roles;

namespace Content.Goobstation.Server.Familiars;

/// <summary>
///     Added to mind role entities to tag that they are a team antagonists.
/// </summary>
[RegisterComponent]
public sealed partial class TeamAntagonistRoleComponent : BaseMindRoleComponent;
