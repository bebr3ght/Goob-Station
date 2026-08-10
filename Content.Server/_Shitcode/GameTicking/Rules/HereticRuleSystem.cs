// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.Heretic;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Text;
using Content.Server.Station.Components;
using Content.Server._Goobstation.Objectives.Components;
using Content.Shared.Station.Components;
// Goobstation
using Content.Server._Shitcode.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;

namespace Content.Server._Shitcode.GameTicking.Rules;

public sealed class HereticRuleSystem : GameRuleSystem<HereticRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;

    // Goobstation
    [Dependency] private readonly RoleBriefingSystem _brief = default!;

    public static readonly SoundSpecifier BriefingSoundIntense =
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain_intense.ogg");

    public static readonly ProtoId<NpcFactionPrototype> HereticFactionId = "Heretic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelect);
        SubscribeLocalEvent<HereticRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    // Goobstation: Briefing Improve - added Character Briefing assign, moved TryMakeHeretic logic in this method
    private void OnAntagSelect(Entity<HereticRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var entUid = args.EntityUid;
        var subColor = args.Def.Briefing?.SubColor ?? args.Def.Briefing?.Color ?? Color.Orange;
        var bold = args.Def.Briefing?.CharacterBriefingBold ?? false;
        var briefing = Loc.GetString("heretic-role-greeting-short", ("subColor", subColor));

        if (!_mind.TryGetMind(entUid, out var mindId, out _) ||
            !_role.MindHasRole<HereticRoleComponent>(mindId, out var hereticRole))
            return;
        _antag.AddCharacterBriefing(hereticRole.Value.Owner, briefing, args.Def.Briefing?.Color, bold, subColor);

        // spawn RealityShift
        if (!TryGetRandomStation(out var station))
            return;

        var grid = GetStationMainGrid(Comp<StationDataComponent>(station.Value));

        if (grid == null)
            return;

        for (var i = 0; i < ent.Comp.RealityShiftPerHeretic.Next(_rand); i++)
        {
            if (TryFindTileOnGrid(grid.Value, out _, out var coords))
                Spawn(ent.Comp.RealityShift, coords);
        }
    }

    public void OnTextPrepend(Entity<HereticRuleComponent> ent, ref ObjectivesTextPrependEvent args)
    {
        var sb = new StringBuilder();

        var mostKnowledge = 0f;
        var mostKnowledgeName = string.Empty;

        foreach (var heretic in EntityQuery<HereticComponent>())
        {
            if (!_mind.TryGetMind(heretic.Owner, out var mindId, out var mind))
                continue;

            var name = _objective.GetTitle((mindId, mind), Name(heretic.Owner));
            if (_mind.TryGetObjectiveComp<HereticKnowledgeConditionComponent>(mindId, out var objective, mind))
            {
                if (objective.Researched > mostKnowledge)
                    mostKnowledge = objective.Researched;
                mostKnowledgeName = name;
            }

            var message =
                $"roundend-prepend-heretic-ascension-{(heretic.Ascended ? "success" : heretic.CanAscend ? "fail" : "fail-owls")}";
            var str = Loc.GetString(message, ("name", name));
            sb.AppendLine(str);
        }

        sb.AppendLine("\n" + Loc.GetString("roundend-prepend-heretic-knowledge-named", ("name", mostKnowledgeName), ("number", mostKnowledge)));

        args.Text = sb.ToString();
    }
}
