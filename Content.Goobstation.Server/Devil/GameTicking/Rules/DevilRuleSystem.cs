// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Goobstation.Server.Devil.Roles;
using Content.Goobstation.Shared.Devil;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.NPC.Systems;

namespace Content.Goobstation.Server.Devil.GameTicking.Rules;

public sealed class DevilRuleSystem : GameRuleSystem<DevilRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;
    [Dependency] private readonly RoleSystem _role = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<DevilRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnSelectAntag(EntityUid uid, DevilRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        var entUid = args.EntityUid;
        var subColor = args.Def.Briefing?.SubColor ?? args.Def.Briefing?.Color ?? Color.Orange;
        var bold = args.Def.Briefing?.CharacterBriefingBold ?? false;
        var devilComp = EnsureComp<DevilComponent>(entUid);
        var greeting = args.Def.Briefing?.Text ?? Loc.GetString("devil-role-greeting", ("trueName", devilComp.TrueName), ("playerName", Name(entUid)), ("subColor", subColor));
        var briefing = Loc.GetString("devil-role-briefing", ("trueName", devilComp.TrueName), ("playerName", Name(entUid)), ("subColor", subColor));

        if (!_mind.TryGetMind(entUid, out var mindId, out _) ||
            !_role.MindHasRole<DevilRoleComponent>(mindId, out var deviLRole))
            return;
        _antag.AddCharacterBriefing(deviLRole.Value.Owner, briefing, args.Def.Briefing?.Color, bold, subColor);
        _antag.SendBriefing(entUid, greeting, args.Def.Briefing?.Color, args.Def.Briefing?.Sound);

        _npcFaction.RemoveFaction(entUid, comp.NanotrasenFaction);
        _npcFaction.AddFaction(entUid, comp.DevilFaction);
    }

    private void OnTextPrepend(EntityUid uid, DevilRuleComponent comp, ref ObjectivesTextPrependEvent args)

    {
        var mostContractsName = string.Empty;
        var mostContracts = 0f;

        var query = EntityQueryEnumerator<DevilComponent>();
        while (query.MoveNext(out var devil, out var devilComp))
        {
            if (!_mind.TryGetMind(devil, out var mindId, out var mind))
                continue;

            var metaData = MetaData(devil);
            if (devilComp.Souls < mostContracts)
                continue;

            mostContracts = devilComp.Souls;
            mostContractsName = _objective.GetTitle((mindId, mind), metaData.EntityName);
        }
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString($"roundend-prepend-devil-contracts{(!string.IsNullOrWhiteSpace(mostContractsName) ? "-named" : "")}", ("name", mostContractsName), ("number", mostContracts)));
        args.Text = sb.ToString();
    }
}
