// SPDX-FileCopyrightText: 2024 qwerltaz <69696513+qwerltaz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Station.Systems;
using Content.Shared.Localizations;
using Robust.Server.GameObjects;

namespace Content.Server.GameTicking.Rules;

public sealed class DragonRuleSystem : GameRuleSystem<DragonRuleComponent>
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DragonRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagEntitySelected);
        // Goobstation: Briefing Improve - comment out unnecessary subs
        // SubscribeLocalEvent<DragonRoleComponent, GetBriefingEvent>(UpdateBriefing);
    }

    private void UpdateBriefing(Entity<DragonRoleComponent> entity, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if(ent is null)
            return;

        args.Append(MakeBriefing(ent.Value, Color.White));
    }

    // Goobstation: Briefing Improve - added Character Briefing assign, color, subColor & bold
    private void AfterAntagEntitySelected(Entity<DragonRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var entUid = args.EntityUid;
        var subColor = args.Def.Briefing?.SubColor ?? args.Def.Briefing?.Color ?? Color.Orange;
        var bold = args.Def.Briefing?.CharacterBriefingBold ?? false;
        var briefing = MakeBriefing(args.EntityUid, subColor);

        if (!_mind.TryGetMind(entUid, out var mindId, out _) ||
            !_roleSystem.MindHasRole<DragonRoleComponent>(mindId, out var dragonRole))
            return;
        _antag.AddCharacterBriefing(dragonRole.Value.Owner, briefing, args.Def.Briefing?.Color, bold, subColor);
        _antag.SendBriefing(entUid, briefing, args.Def.Briefing?.Color, null);
    }

    // Goobstation: Briefing Improve - added subColor
    private string MakeBriefing(EntityUid dragon, Color subColor)
    {
        var direction = string.Empty;

        var dragonXform = Transform(dragon);

        EntityUid? stationGrid = null;
        if (_station.GetStationInMap(dragonXform.MapID) is { } station)
            stationGrid = _station.GetLargestGrid(station);

        if (stationGrid is not null)
        {
            var stationPosition = _transform.GetWorldPosition((EntityUid)stationGrid);
            var dragonPosition = _transform.GetWorldPosition(dragon);

            var vectorToStation = stationPosition - dragonPosition;
            direction = ContentLocalizationManager.FormatDirection(vectorToStation.GetDir());
        }

        var briefing = Loc.GetString("dragon-role-briefing", ("direction", direction), ("subColor", subColor));

        return briefing;
    }
}
