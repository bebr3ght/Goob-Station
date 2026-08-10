// SPDX-FileCopyrightText: 2023 Colin-Tel <113523727+Colin-Tel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfall <rainfey0+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Rainfey <11758391+Rainfey@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Shared.Humanoid;
// Goobstation
using Content.Shared.Roles;
using Content.Server.Mind;

namespace Content.Server.GameTicking.Rules;

public sealed class ThiefRuleSystem : GameRuleSystem<ThiefRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    // Goobstation
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThiefRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        // Goobstation: Briefing Improve - Comment out unnecessary subs
        // SubscribeLocalEvent<ThiefRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    // Greeting upon thief activation
    // Goobstation: Briefing Improve - Added subColor, bold & character briefing through RoleBriefingComponent
    private void AfterAntagSelected(Entity<ThiefRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var entUid = args.EntityUid;

        var subColor = args.Def.Briefing?.SubColor ?? args.Def.Briefing?.Color ?? Color.Orange;
        var bold = args.Def.Briefing?.CharacterBriefingBold ?? false;

        if (!_mindSystem.TryGetMind(entUid, out var mindId, out _) ||
            !_roleSystem.MindHasRole<ThiefRoleComponent>(mindId, out var thiefRole))
            return;
        _antag.SendBriefing(entUid, MakeBriefing(entUid, args.Def.Briefing?.SubColor), args.Def.Briefing?.Color, null);
        _antag.AddCharacterBriefing(thiefRole.Value.Owner, MakeBriefing(entUid, subColor), args.Def.Briefing?.Color, bold, subColor);
    }

    // Character screen briefing
    private void OnGetBriefing(Entity<ThiefRoleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is null)
            return;
        args.Append(MakeBriefing(ent.Value));
    }

    // Goobstation: Briefing Improve - Added subColor
    private string MakeBriefing(EntityUid ent, Color? subColor = null)
    {
        var isHuman = HasComp<HumanoidAppearanceComponent>(ent);
        subColor ??= Color.Orange;
        var briefing = isHuman
            ? Loc.GetString("thief-role-greeting-human", ("subColor", subColor))
            : Loc.GetString("thief-role-greeting-animal", ("subColor", subColor));

        if (isHuman)
            briefing += "\n \n" + Loc.GetString("thief-role-greeting-equipment", ("subColor", subColor));

        return briefing;
    }
}
