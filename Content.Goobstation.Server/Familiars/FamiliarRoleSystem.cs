using Content.Server.Antag;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Roles;
using Content.Server.Speech.Components;
using Content.Shared.Bible.Components;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Familiars;

public sealed class FamiliarRoleSystem : EntitySystem
{
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FamiliarRoleComponent, GetBriefingEvent>(OnGetBrief);
        SubscribeLocalEvent<FamiliarComponent, MobStateChangedEvent>(OnFamiliarDeath);
        SubscribeLocalEvent<RoleAddedEvent>(OnStartup);
    }

    private void OnStartup(RoleAddedEvent args)
    {
        if (!_roleSystem.MindHasRole<FamiliarRoleComponent>(args.MindId) && !_roleSystem.MindHasRole<TeamAntagonistRoleComponent>(args.MindId))
            return;

        var familiarEntity = args.Mind.OwnedEntity;
        if (familiarEntity == null)
            return;

        var briefing = GetBriefingText(args.MindId);
        _antag.SendBriefing(familiarEntity.Value, briefing, Color.Crimson, null);
    }

    private string GetBriefingText(EntityUid mind)
    {
        if (!TryComp<MindComponent>(mind, out var mindComponent))
            return string.Empty;

        var ownedEntity = mindComponent.OwnedEntity;
        if (ownedEntity == null || !TryComp<GhostRoleComponent>(ownedEntity, out var ghostRole))
            return string.Empty;

        var owner = ghostRole.SpawnedFromCreature;
        if (owner == null)
            return string.Empty;

        if (!TryComp<MindContainerComponent>(owner.Value, out var mindContainer) ||
            mindContainer.Mind == null ||
            !TryComp<MindComponent>(mindContainer.Mind.Value, out var masterMind))
            return string.Empty;

        var ownerName = masterMind.CharacterName ?? MetaData(owner.Value).EntityName;
        var ownerRole = _jobs.MindTryGetJobName(mindContainer.Mind.Value);
        var subtype = masterMind.Subtype;

        var roleBriefing = subtype != null
            ? Loc.GetString("role-type-update-message-familiar",
                ("owner", ownerName),
                ("owner-role", ownerRole),
                ("team", subtype))
            : Loc.GetString("role-type-update-message-familiar",
                ("owner", ownerName),
                ("owner-role", ownerRole));

        return roleBriefing;
    }

    private void OnGetBrief(EntityUid uid, FamiliarRoleComponent comp, ref GetBriefingEvent args)
    {
        var briefing = GetBriefingText(args.Mind.Owner);
        args.Append(briefing);
    }

    /// <summary>
    /// Starts up the respawn stuff when
    /// the chaplain's familiar dies.
    /// </summary>
    private void OnFamiliarDeath(EntityUid uid, FamiliarComponent component, MobStateChangedEvent args)
    {
        // Сделать после смерти апдейт чтобы можно было заспавнить вновь существо через n секунд. GhostRoleSystem содержит апдейт.
        if (!TryComp<GhostRoleComponent>(uid, out var ghostRole))
            return;
        if (args.NewMobState != MobState.Dead || ghostRole.SpawnedFromItem == null)
            return;

        var item = ghostRole.SpawnedFromItem;
        EnsureComp<RepeatSpawnCooldownComponent>(item.Value);
    }
}
