// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Ghost.Roles;
using Content.Shared.Bible.Components;

namespace Content.Server.Roles;

public sealed class RoleBriefingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoleBriefingComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(EntityUid uid, RoleBriefingComponent comp, ref GetBriefingEvent args)
    {
        // Goobstation - briefing for familiars
        var ev = new FamiliarBriefingEvent(args.Mind.Owner);

        if (HasComp<HasOwnerComponent>(args.Mind.Comp.OwnedEntity))
            RaiseLocalEvent(ref ev);

        var briefing = string.IsNullOrEmpty(comp.Briefing) ? "" : Loc.GetString(comp.Briefing);
        if (ev.Briefing != null)
            args.Append(ev.Briefing + "\n" + briefing);
        else
            args.Append(Loc.GetString(comp.Briefing));
    }
}
