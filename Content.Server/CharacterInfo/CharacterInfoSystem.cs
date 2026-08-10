// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 chavonadelal <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Shared.CharacterInfo;
using Content.Shared.Objectives;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;

// Goobstation
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Server.CharacterInfo;

public sealed class CharacterInfoSystem : EntitySystem
{
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    // Goobstation
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestCharacterInfoEvent>(OnRequestCharacterInfoEvent);
    }

    // Goobstation: Briefing Improve - edited, added supervisors
    private void OnRequestCharacterInfoEvent(RequestCharacterInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue
            || args.SenderSession.AttachedEntity != GetEntity(msg.NetEntity))
            return;

        var entity = args.SenderSession.AttachedEntity.Value;

        if (!_minds.TryGetMind(entity, out var mindId, out var mind))
            return;

        Log.Debug($"Generating Character Briefing for {args.SenderSession} in CharacterInfoSystem");
        var jobTitle = _jobs.MindTryGetJobName(mindId, out var jobName)
            ? jobName
            : string.Empty;
        var supervisors = _jobs.MindTryGetJobSupervisors(mindId, out var supervisorsString)
            ? supervisorsString
            : string.Empty;
        var antagRoles = CollectAntagRoles(mindId, mind);

        RaiseNetworkEvent(new CharacterInfoEvent(GetNetEntity(entity), jobTitle, supervisors, antagRoles),
            args.SenderSession);
    }

    // Goobstation: Briefing Improve
    private List<AntagRoleInfo> CollectAntagRoles(EntityUid mindId, MindComponent mind)
    {
        var antagRoles = new List<AntagRoleInfo>();
        var processedObjectives = new HashSet<EntityUid>();

        foreach (var mindRole in mind.MindRoles)
        {
            if (HasComp<JobRoleComponent>(mindRole) ||
                !TryComp<MindRoleComponent>(mindRole, out var mindRoleComp) ||
                !_proto.TryIndex(mindRoleComp.RoleType, out var roleType) ||
                !_proto.TryIndex(mindRoleComp.AntagPrototype, out var antagProto))
            {
                Log.Error($"Failed to collect antagonist {mindRole} role info!");
                continue;
            }

            var roleObjectives = CollectRoleObjectives(
                mind.Objectives,
                mindRole,
                antagProto,
                mindId,
                mind,
                processedObjectives
            );

            var localisedList = new List<string>();
            foreach (var issuer in antagProto.Issuers)
                localisedList.Add(Loc.GetString(issuer));

            var issuerDisplay = string.Join(", ", localisedList);

            var briefingEvent = new GetBriefingEvent { Mind = (mindId, mind) };
            RaiseLocalEvent(mindRole, ref briefingEvent);

            var antagRoleInfo = new AntagRoleInfo(
                RoleTitle: Loc.GetString(antagProto.Name),
                RoleTitleColor: antagProto.NameColor,
                RoleType: Loc.GetString(roleType.Name),
                Issuer: issuerDisplay,
                Briefing: briefingEvent.Briefing,
                Objectives: roleObjectives,
                Color: roleType.Color,
                BriefingColor: briefingEvent.BriefingColor ?? roleType.Color,
                Bold: briefingEvent.Bold
            );

            antagRoles.Add(antagRoleInfo);
        }

        return antagRoles;
    }


    // Goobstation: Briefing Improve
    private List<ObjectiveInfo> CollectRoleObjectives(
        List<EntityUid> allObjectives,
        EntityUid mindRole,
        AntagPrototype antagProto,
        EntityUid mindId,
        MindComponent mind,
        HashSet<EntityUid> processedObjectives)
    {
        var roleObjectives = new List<ObjectiveInfo>();

        foreach (var objectiveUid in allObjectives)
        {
            if (processedObjectives.Contains(objectiveUid) ||
                !TryComp<ObjectiveComponent>(objectiveUid, out var objectiveComp) ||
                !IsObjectiveForRole(objectiveUid, mindRole, antagProto, objectiveComp))
            {
                Log.Error($"Failed to collect antagonist {mindRole} role objectives!");
                continue;
            }

            var objectiveInfo = _objectives.GetInfo(objectiveUid, mindId, mind);
            if (objectiveInfo == null)
            {
                Log.Error($"Failed to collect antagonist {mindRole} role objectives! ObjectiveInfo does not exist!");
                continue;
            }

            roleObjectives.Add(objectiveInfo.Value);
            processedObjectives.Add(objectiveUid);
        }

        return roleObjectives;
    }

    // Goobstation: Briefing Improve
    private bool IsObjectiveForRole(
        EntityUid objectiveUid,
        EntityUid mindRole,
        AntagPrototype antagProto,
        ObjectiveComponent objectiveComp)
    {
        if (TryComp<RoleRequirementComponent>(objectiveUid, out var roleReq))
        {
            foreach (var requiredRoleName in roleReq.Roles)
            {
                var roleType = _factory.GetRegistration(requiredRoleName).Type;
                if (HasComp(mindRole, roleType))
                    return true;
            }
            return false;
        }

        return !string.IsNullOrWhiteSpace(objectiveComp.Issuer) && antagProto.Issuers.Contains(objectiveComp.Issuer);
    }
}
