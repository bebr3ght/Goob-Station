using Content.Goobstation.Common.Mind;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Relations;

public sealed class RelationStatusIconSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IsOwnerComponent, GetStatusIconsEvent>(OnGetOwnerIcon);
        SubscribeLocalEvent<HasOwnersComponent, GetStatusIconsEvent>(OnGetFamiliarIcon);
    }

    private void OnGetFamiliarIcon(Entity<HasOwnersComponent> ent, ref GetStatusIconsEvent args)
    {
        var player = _player.LocalEntity;

        if (!HasComp<IsOwnerComponent>(player) || !ent.Comp.OwnersMob.Contains(GetNetEntity(player.Value)))
            return;
        if (_prototype.TryIndex<FactionIconPrototype>(ent.Comp.StatusIcon, out var icon))
            args.StatusIcons.Add(icon);
    }

    private void OnGetOwnerIcon(Entity<IsOwnerComponent> ent, ref GetStatusIconsEvent args)
    {
        var player = _player.LocalEntity;

        if (!TryComp<HasOwnersComponent>(player, out var playerOwners) || !playerOwners.OwnersMob.Contains(GetNetEntity(ent.Owner)))
            return;
        if (_prototype.TryIndex<FactionIconPrototype>(ent.Comp.StatusIcon, out var icon))
            args.StatusIcons.Add(icon);
    }

}
