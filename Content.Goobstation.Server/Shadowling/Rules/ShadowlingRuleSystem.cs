// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;

namespace Content.Goobstation.Server.Shadowling.Rules;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly NpcFactionSystem _npc = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);

        SubscribeLocalEvent<ShadowlingAscendEvent>(OnAscend);
        SubscribeLocalEvent<ShadowlingDeathEvent>(OnDeath);
    }

    private void OnDeath(ShadowlingDeathEvent args)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var shadowling, out _))
        {
            var shadowlingCount = 0;
            var shadowlingDead = 0;
            var query = EntityQueryEnumerator<ShadowlingComponent>();

            while (query.MoveNext(out var uid, out _))
            {
                shadowlingCount++;
                if (_mob.IsDead(uid) || _mob.IsInvalidState(uid))
                    shadowlingDead++;
            }

            if (shadowlingCount == shadowlingDead)
                shadowling.WinCondition = ShadowlingWinCondition.Failure;
        }
    }

    private void OnAscend(ShadowlingAscendEvent args)
    {
        var rulesQuery = QueryActiveRules();
        while (rulesQuery.MoveNext(out _, out var shadowling, out _))
        {
            shadowling.WinCondition = ShadowlingWinCondition.Win;
            return;
        }
    }

    private void OnSelectAntag(EntityUid uid, ShadowlingRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        var entUid = args.EntityUid;
        var subColor = args.Def.Briefing?.SubColor ?? args.Def.Briefing?.Color ?? Color.Orange;
        var briefing = Loc.GetString("shadowling-briefing", ("subColor", subColor));
        var bold = args.Def.Briefing?.CharacterBriefingBold ?? false;

        if (!_mind.TryGetMind(entUid, out var mindId, out _) ||
            !_role.MindHasRole<ShadowlingRoleComponent>(mindId, out var shadowlingRole))
            return;
        _antag.AddCharacterBriefing(shadowlingRole.Value.Owner, briefing, args.Def.Briefing?.Color, bold, subColor);
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        ShadowlingRuleComponent component,
        GameRuleComponent gamerule,
        ref RoundEndTextAppendEvent args
    )
    {
        base.AppendRoundEndText(uid, component, gamerule, ref args);
        var winText = Loc.GetString($"shadowling-condition-{component.WinCondition.ToString().ToLower()}");
        args.AddLine(winText);

        args.AddLine(Loc.GetString("shadowling-list-start"));

        var sessionData = _antag.GetAntagIdentifiers(uid);
        foreach (var (_, data, name) in sessionData)
        {
            var listing = Loc.GetString("shadowling-list-name", ("name", name), ("user", data.UserName));
            args.AddLine(listing);
        }
    }
}
