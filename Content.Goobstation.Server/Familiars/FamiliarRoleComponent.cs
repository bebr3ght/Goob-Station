using Content.Shared.Roles;

namespace Content.Goobstation.Server.Familiars;

/// <summary>
///     Added to mind role entities to tag that they are a familiar.
/// </summary>
[RegisterComponent]
public sealed partial class FamiliarRoleComponent : BaseMindRoleComponent;
